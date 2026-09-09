// ReSharper disable once CheckNamespace
namespace Ivy;

public record WireframeRedX : WidgetBase<WireframeRedX>
{
    public WireframeRedX(Colors color = Colors.Red)
    {
        Color = color;
    }

    [Prop] public Colors Color { get; set; } = Colors.Red;

    public static WireframeRedX operator |(WireframeRedX widget, object child)
    {
        throw new NotSupportedException("WireframeRedX does not support children.");
    }
}

public static class WireframeRedXExtensions
{
    public static WireframeRedX Color(this WireframeRedX redX, Colors color)
        => redX with { Color = color };
}
