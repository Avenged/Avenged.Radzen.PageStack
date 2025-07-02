namespace Avenged.Radzen.PageStack;

public sealed class PageStackPage(Type type, Dictionary<string, object?>? parameters)
{
    public string UniqueID { get; } = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("/", "-").Replace("+", "-").Substring(0, 10);
    public Type Type { get; init; } = type;
    public Dictionary<string, object?>? Parameters { get; init; } = parameters;
}
