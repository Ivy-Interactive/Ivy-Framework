using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Widget for rendering raw HTML content directly within application interface. Security Warning: Content should be sanitized to prevent XSS attacks.</summary>
public record Html : WidgetBase<Html>
{
    /// <summary>Initializes HTML widget with specified raw HTML content.</summary>
    /// <param name="content">Raw HTML content to render. Should be properly formatted and sanitized.</param>
    public Html(string content)
    {
        Content = content;
    }

    /// <summary>Raw HTML content rendered directly in application interface.</summary>
    [Prop] public string Content { get; set; }
}