using Ivy.Core;

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
    [Prop] public bool Enabled { get; set; } = true;
}

public static class AutoScrollExtensions
{
    public static AutoScroll Enabled(this AutoScroll auto, bool enabled)
    {
        auto.Enabled = enabled;
        return auto;
    }
}
