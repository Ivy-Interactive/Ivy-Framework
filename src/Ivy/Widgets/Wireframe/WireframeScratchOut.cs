// ReSharper disable once CheckNamespace
namespace Ivy;

public record WireframeScratchOut : WidgetBase<WireframeScratchOut>
{
    public WireframeScratchOut(Colors color = Colors.Black)
    {
        Color = color;
    }

    [Prop] public Colors Color { get; set; } = Colors.Black;

    public static WireframeScratchOut operator |(WireframeScratchOut widget, object child)
    {
        throw new NotSupportedException("WireframeScratchOut does not support children.");
    }
}

public static class WireframeScratchOutExtensions
{
    public static WireframeScratchOut Color(this WireframeScratchOut scratchOut, Colors color)
        => scratchOut with { Color = color };
}
