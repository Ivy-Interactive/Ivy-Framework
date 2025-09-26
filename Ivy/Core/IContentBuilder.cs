namespace Ivy.Core;

/// <summary>
/// Interface for transforming and formatting content into visual representations for display in Ivy.
/// Follows a middleware pattern allowing flexible and extensible content transformation pipelines.
/// </summary>
public interface IContentBuilder
{
    /// <summary>
    /// Determines whether this content builder can process the specified content type.
    /// </summary>
    /// <param name="content">The content object to evaluate for processing capability.</param>
    /// <returns>True if this builder can handle the content type; false otherwise.</returns>
    public bool CanHandle(object? content);

    /// <summary>
    /// Transforms the content into its appropriate visual representation (widgets, views, or formatted data).
    /// </summary>
    /// <param name="content">The content object to format and transform.</param>
    /// <returns>The formatted content as a widget, view, or other displayable object.</returns>
    public object? Format(object? content);
}