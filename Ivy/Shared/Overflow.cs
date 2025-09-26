namespace Ivy.Shared;

/// <summary>
/// Specifies how content that exceeds the available space should be handled.
/// </summary>
public enum Overflow
{
    /// <summary>Browser default overflow behavior (typically shows scrollbars when needed).</summary>
    Auto,

    /// <summary>Clips content that exceeds the container bounds, hiding the overflow.</summary>
    Clip,

    /// <summary>Clips content and displays ellipsis (...) to indicate truncated text.</summary>
    Ellipsis
}