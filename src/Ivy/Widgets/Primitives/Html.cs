// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Renders raw HTML content.
/// </summary>
public record Html : WidgetBase<Html>
{
    public Html(string content)
    {
        Content = content;
    }

    internal Html() { }

    [Prop] public string Content { get; set; } = string.Empty;
    [Prop] public bool DangerouslyAllowScripts { get; set; }
}

public static class HtmlExtensions
{
    public static Html DangerouslyAllowScripts(this Html html, bool allow = true)
    {
        return html with { DangerouslyAllowScripts = allow };
    }
}