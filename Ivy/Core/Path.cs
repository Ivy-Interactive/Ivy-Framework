using System.Diagnostics;

namespace Ivy.Core;

/// <summary>
/// Represents a single segment in a hierarchical path through the widget/view tree.
/// Used for tracking the location and identity of components during widget tree construction.
/// </summary>
/// <param name="Type">The type name of the view or widget.</param>
/// <param name="Key">The optional key for the component, used for identification.</param>
/// <param name="Index">The index position of the component among its siblings.</param>
/// <param name="IsWidget">True if this segment represents a widget; false if it represents a view.</param>
public readonly record struct PathSegment(string Type, string? Key, int Index, bool IsWidget)
{
    /// <summary>
    /// Returns a string representation of this path segment in the format "Type:Key" or "Type:Index".
    /// </summary>
    /// <returns>A string representation using the key if available, otherwise the index.</returns>
    public override string ToString()
    {
        return $"{Type}:{Key ?? Index.ToString()}";
    }
}

/// <summary>
/// Represents a hierarchical path through the widget/view tree, used for tracking component locations
/// and generating unique identifiers during widget tree construction and traversal.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public class Path : Stack<PathSegment>
{
    /// <summary>
    /// Pushes a view onto the path stack, creating a new path segment for the view.
    /// </summary>
    /// <param name="view">The view to add to the path.</param>
    /// <param name="index">The index position of the view among its siblings.</param>
    public void Push(IView view, int index)
    {
        Push(new PathSegment(view.GetType().Name!, view.Key, index, false));
    }

    /// <summary>
    /// Pushes a widget onto the path stack, creating a new path segment for the widget.
    /// </summary>
    /// <param name="widget">The widget to add to the path.</param>
    /// <param name="index">The index position of the widget among its siblings.</param>
    public void Push(IWidget widget, int index)
    {
        Push(new PathSegment(widget.GetType().Name!, widget.Key, index, true));
    }

    /// <summary>
    /// Creates a deep copy of this path with all segments preserved in the same order.
    /// </summary>
    /// <returns>A new Path instance containing the same segments as this path.</returns>
    public Path Clone()
    {
        Path clone = new();
        var segments = this.ToList();
        for (int i = segments.Count - 1; i >= 0; i--)
        {
            var segment = segments[i];
            clone.Push(new PathSegment(segment.Type, segment.Key, segment.Index, segment.IsWidget));
        }
        return clone;
    }

    /// <summary>
    /// Returns a string representation of the complete path, with segments separated by ">".
    /// Used for generating unique identifiers and debugging widget tree structure.
    /// </summary>
    /// <returns>A string representation of the path hierarchy.</returns>
    public override string ToString()
    {
        return string.Join(">", this.Select(e => e.ToString()).ToArray());
    }
}