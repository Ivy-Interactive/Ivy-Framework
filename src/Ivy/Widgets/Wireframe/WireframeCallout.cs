// ReSharper disable once CheckNamespace
namespace Ivy;

public record WireframeCallout : WidgetBase<WireframeCallout>
{
    public WireframeCallout(string? label = null, Colors color = Colors.Yellow)
    {
        Label = label;
        Color = color;
    }

    internal WireframeCallout() { }

    [Prop] public string? Label { get; set; }

    [Prop] public Colors Color { get; set; } = Colors.Yellow;

    public static WireframeCallout operator |(WireframeCallout widget, object child)
    {
        throw new NotSupportedException("WireframeCallout does not support children. Use the Label property.");
    }
}

public static class WireframeCalloutExtensions
{
    public static WireframeCallout Label(this WireframeCallout callout, string label)
        => callout with { Label = label };

    public static WireframeCallout Color(this WireframeCallout callout, Colors color)
        => callout with { Color = color };
}
