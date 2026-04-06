// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A scrollable container that automatically scrolls to the bottom when content grows.
/// </summary>
public record AutoScroll : WidgetBase<AutoScroll>
{
    public AutoScroll(params object[] children) : base(children)
    {
    }

    internal AutoScroll()
    {
    }

    /// <summary>
    /// Builds an <see cref="AutoScroll"/> from a sequence of child widgets so callers do not need
    /// <c>ToArray&lt;object&gt;()</c> (C# does not treat <c>T[]</c> as <c>object[]</c> for <c>params</c>).
    /// </summary>
    public static AutoScroll FromChildren<T>(IEnumerable<T> children)
        where T : class =>
        new(children.Cast<object>().ToArray());

    /// <summary>
    /// When <c>false</c> (default), new content keeps the view pinned to the bottom. When <c>true</c>, the user scrolls manually and the view does not jump on updates.
    /// </summary>
    [Prop] public bool Disabled { get; set; }
}

public static class AutoScrollExtensions
{
    public static AutoScroll Disabled(this AutoScroll auto, bool disabled = true)
    {
        auto.Disabled = disabled;
        return auto;
    }
}
