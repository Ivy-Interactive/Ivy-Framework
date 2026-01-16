using Ivy.Core;

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

}

public static class HtmlExtensions
{
}