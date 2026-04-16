// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A standard header layout with branding and navigation areas.
/// </summary>
public record HeaderLayout : WidgetBase<HeaderLayout>
{
    public HeaderLayout(object header, object content) : base([new Slot("Header", header), new Slot("Content", content)])
    {
    }

    internal HeaderLayout()
    {
    }

    [Prop] public bool ShowHeaderDivider { get; init; } = true;

    [Prop] public Scroll ContentScroll { get; init; } = Scroll.Auto;

    public static HeaderLayout operator |(HeaderLayout widget, object child)
    {
        throw new NotSupportedException("HeaderLayout does not support children.");
    }
}

public static class HeaderLayoutExtensions
{
    public static HeaderLayout Scroll(this HeaderLayout headerLayout, Scroll scroll = Ivy.Scroll.Auto)
    {
        var result = headerLayout with { ContentScroll = scroll };

        // When scroll is disabled, automatically set height to Full if no height is explicitly set
        if (scroll == Ivy.Scroll.None && result.Height == null)
        {
            return result.Height(Size.Full());
        }

        return result;
    }
}