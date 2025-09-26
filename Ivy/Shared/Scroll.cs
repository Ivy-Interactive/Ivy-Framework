namespace Ivy.Shared;

/// <summary>
/// Specifies the scrolling behavior for containers when content exceeds available space.
/// </summary>
public enum Scroll
{
    /// <summary>No scrolling - content that overflows is clipped or hidden.</summary>
    None,

    /// <summary>Automatic scrolling - displays scrollbars when content exceeds container bounds.</summary>
    Auto
}