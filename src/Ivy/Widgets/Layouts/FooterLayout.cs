// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A layout container for footer content, typically pinned to the bottom.
/// </summary>
[Slot("Footer")]
[Slot("Content")]
public record FooterLayout : WidgetBase<FooterLayout>
{
    public FooterLayout(object footer, object content) : base([new Slot("Footer", footer), new Slot("Content", content)])
    {
    }

    internal FooterLayout()
    {
    }

    [Prop] public Scroll ContentScroll { get; init; } = Scroll.Auto;

    public static FooterLayout operator |(FooterLayout widget, object child)
    {
        throw new NotSupportedException("FooterLayout does not support children.");
    }
}

public static class FooterLayoutExtensions
{
    public static FooterLayout Scroll(this FooterLayout footerLayout, Scroll scroll = Ivy.Scroll.Auto)
    {
        return footerLayout with { ContentScroll = scroll };
    }
}