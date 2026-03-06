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

    /// <summary>
    /// When set to true, allows JavaScript execution including script tags and inline event handlers.
    /// WARNING: Only use this with trusted HTML content. Never use with user-supplied HTML.
    /// </summary>
    [Prop] public bool DangerouslyAllowScripts { get; set; }
}

public static class HtmlExtensions
{
    /// <summary>
    /// Enables JavaScript execution in the HTML content.
    /// WARNING: Only use this with trusted HTML content. Never use with user-supplied HTML.
    /// </summary>
    /// <param name="html">The Html widget.</param>
    /// <param name="allow">Whether to allow scripts. Default is true when called.</param>
    /// <returns>A new Html instance with the DangerouslyAllowScripts property set.</returns>
    public static Html DangerouslyAllowScripts(this Html html, bool allow = true)
    {
        return html with { DangerouslyAllowScripts = allow };
    }
}