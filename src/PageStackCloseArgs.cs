namespace Avenged.Radzen.PageStack;

public sealed class PageStackCloseArgs(PageStackPage page, dynamic? result)
{
    public PageStackPage Page { get; init; } = page;
    public dynamic? Result { get; init; } = result;
}