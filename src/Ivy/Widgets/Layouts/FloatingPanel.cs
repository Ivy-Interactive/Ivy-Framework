using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A panel that floats above other content, often used for popovers or tool palettes.
/// </summary>
public record FloatingPanel : WidgetBase<FloatingPanel>
{
    public FloatingPanel(object? child = null, Align alignSelf = Align.BottomRight) : base(child != null ? [child] : [])
    {
        AlignSelf = alignSelf;
    }

    internal FloatingPanel() { }

    [Prop] public Align AlignSelf { get; set; } = Align.BottomRight;

    [Prop] public Thickness? Offset { get; set; }

    public static FloatingPanel operator |(FloatingPanel widget, object child)
    {
        if (child is IEnumerable<object> _)
        {
            throw new NotSupportedException("FloatingLayer does not support multiple children.");
        }

        return widget with { Children = [child] };
    }
}

public static class FloatingLayerExtensions
{
    public static FloatingPanel AlignSelf(this FloatingPanel floatingButton, Align align) => floatingButton with { AlignSelf = align };

    public static FloatingPanel Offset(this FloatingPanel floatingButton, Thickness? offset) => floatingButton with { Offset = offset };

    public static FloatingPanel OffsetLeft(this FloatingPanel floatingButton, int offset) => floatingButton with { Offset = new Thickness(offset, 0, 0, 0) };

    public static FloatingPanel OffsetTop(this FloatingPanel floatingButton, int offset) => floatingButton with { Offset = new Thickness(0, offset, 0, 0) };

    public static FloatingPanel OffsetRight(this FloatingPanel floatingButton, int offset) => floatingButton with { Offset = new Thickness(0, 0, offset, 0) };

    public static FloatingPanel OffsetBottom(this FloatingPanel floatingButton, int offset) => floatingButton with { Offset = new Thickness(0, 0, 0, offset) };
}