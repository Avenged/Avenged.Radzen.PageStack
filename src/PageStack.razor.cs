using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Avenged.Radzen.Abstractions;
using Radzen;
using System.Reflection;
using Rz = Radzen;

namespace Avenged.Radzen.PageStack;

public partial class PageStack : ComponentBase, IAsyncDisposable
{
    private readonly Stack<string> _titles = [];
    private readonly Stack<PageStackPage> _pages = [];
    private readonly List<TaskCompletionSource<dynamic>> _tasks = [];
    private Rz.TooltipService? _ts;
    private string? _lastNavigationUri;
    private ExtendedJSRuntime _extendedJS = null!;

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public EventCallback<PageStackCloseArgs> Close { get; set; }

    [Parameter]
    public string? AnimationClass { get; set; }

    protected override void OnInitialized()
    {
        _ts = SP.GetService<TooltipService>();
        _lastNavigationUri = NM.Uri;
        _extendedJS = JS.GetExtendedJsRuntime();
        NM.LocationChanged += HandleLocationChanged;
    }

    private void HandleLocationChanged(object? sender, LocationChangedEventArgs args)
    {
        StringComparison sc = StringComparison.InvariantCultureIgnoreCase;
        bool sameLocation = _lastNavigationUri?.Equals(args.Location, sc) ?? false;
        if (!sameLocation)
        {
            _lastNavigationUri = args.Location;
            _pages.Clear();
        }
    }

    public void Push(Type type, Dictionary<string, object?>? parameters = null)
    {
        // Close any tooltips that were left visible so that they are not overlapped
        _ts?.Close();

        parameters ??= [];

        if (HasOpenModeProperty(type))
        {
            parameters.Remove(nameof(ViewBase.OpenMode));
            parameters.Add(nameof(ViewBase.OpenMode), OpenMode.Stack);
        }

        _pages.Push(new PageStackPage(type, parameters));
        StateHasChanged();
    }

    public void Push<T>(Dictionary<string, object?>? parameters = null)
    {
        // Close any tooltips that were left visible so that they are not overlapped
        _ts?.Close();

        parameters ??= [];

        if (HasOpenModeProperty<T>())
        {
            parameters.Remove(nameof(ViewBase.OpenMode));
            parameters.Add(nameof(ViewBase.OpenMode), OpenMode.Stack);
        }

        _pages.Push(new PageStackPage(typeof(T), parameters));
        StateHasChanged();
    }

    public Task<dynamic> PushAsync(Type type, Dictionary<string, object?>? parameters = null)
    {
        // Close any tooltips that were left visible so that they are not overlapped
        _ts?.Close();

        TaskCompletionSource<dynamic> task = new();
        _tasks.Add(task);

        Task backupTitleTask = BackupTitle();

        Push(type, parameters);

        return WhenAll(backupTitleTask, task.Task);
    }

    public Task<dynamic> PushAsync<T>(Dictionary<string, object?>? parameters = null)
    {
        // Close any tooltips that were left visible so that they are not overlapped
        _ts?.Close();

        parameters ??= [];

        if (HasOpenModeProperty<T>())
        {
            parameters.Remove(nameof(ViewBase.OpenMode));
            parameters.Add(nameof(ViewBase.OpenMode), OpenMode.Stack);
        }

        TaskCompletionSource<dynamic> task = new();
        _tasks.Add(task);

        Task backupTitleTask = BackupTitle();

        Push<T>(parameters);

        return WhenAll(backupTitleTask, task.Task);
    }

    private static async Task<T> WhenAll<T>(Task task1, Task<T> task2)
    {
        await Task.WhenAll(task1, task2);
        return await task2;
    }

    private async Task BackupTitle()
    {
        string title = await _extendedJS.GetValueAsync<string>("document.title");
        _titles.Push(title);
    }

    public async Task Pop(dynamic? result = null)
    {
        if (_pages.Count == 0)
        {
            return;
        }

        bool exception = false;
        PageStackPage? page = default;

        await RestoreTitle();

        try
        {
            page = _pages.Pop();
        }
        catch (Exception ex)
        {
            await JS.ConsoleError("An error occurred while popping a page from the stack. Error: " + ex.Message);
            exception = true;
        }

        // TODO: allow a way to animate the page removal
        //if (!exception && page is not null)
        //{
        //    try
        //    {
        //        await JS.InvokeAsync<bool>("animateStackPagePop", $"#{page.Id}", "fadeOutDown", "slower");
        //    }
        //    catch (Exception)
        //    {
        //        await JS.ConsoleError("Ocurrió un error durante la invocación de la función JS 'animateStackPagePop'");
        //        exception = true;
        //    }
        //}

        if (exception)
        {
            return;
        }

        if (page is not null)
        {
            await Close.InvokeAsync(new PageStackCloseArgs(page, result));
        }

        TaskCompletionSource<dynamic>? task = _tasks.LastOrDefault();
        if (task is not null && task.Task is not null && !task.Task.IsCompleted)
        {
            _tasks.Remove(task);
            task.SetResult(result!);
        }

        StateHasChanged();
    }

    private async Task RestoreTitle()
    {
        if (_titles.TryPop(out string? title))
        {
            await _extendedJS.SetValueAsync("document.title", title);
        }
    }

    private static bool HasOpenModeProperty(Type type)
    {
        return type.GetProperty(nameof(ViewBase.OpenMode), 
            BindingFlags.Public | 
            BindingFlags.Instance) is not null;
    }

    private static bool HasOpenModeProperty<T>()
    {
        return typeof(T).GetProperty(nameof(ViewBase.OpenMode),
            BindingFlags.Public | 
            BindingFlags.Instance) is not null;
    }

    public async ValueTask DisposeAsync()
    {
        NM.LocationChanged -= HandleLocationChanged;
        await _extendedJS.DisposeAsync();
    }
}