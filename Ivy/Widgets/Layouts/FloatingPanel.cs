using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record FloatingPanel : WidgetBase<FloatingPanel>
{
    public FloatingPanel(object? child = null, Align align = Align.BottomRight) : base(child != null ? [child] : [])
    {
        Align = align;
    }

    [Prop] public Align Align { get; set; }

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
    public static FloatingPanel Align(this FloatingPanel floatingButton, Align align)
    {
        return floatingButton with { Align = align };
    }
}