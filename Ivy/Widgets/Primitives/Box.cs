using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record Box : WidgetBase<Box>
{
    public Box(params IEnumerable<object> content) : base(content.ToArray())
    {
    }

    [Prop] public Colors? Color { get; set; } = Colors.Primary;

    [Prop] public Thickness BorderThickness { get; set; } = new(2);

    [Prop] public BorderRadius BorderRadius { get; set; } = BorderRadius.Rounded;

    [Prop] public BorderStyle BorderStyle { get; set; } = BorderStyle.Solid;

    [Prop] public Thickness Padding { get; set; } = new(2);

    [Prop] public Thickness Margin { get; set; } = new(0);

    [Prop] public Align? ContentAlign { get; set; } = Align.Center;
}

public static class BoxExtensions
{
    public static Box Color(this Box box, Colors color) => box with { Color = color };

    public static Box BorderThickness(this Box box, int thickness) => box with { BorderThickness = new(thickness) };
    public static Box BorderThickness(this Box box, Thickness thickness) => box with { BorderThickness = thickness };

    public static Box BorderRadius(this Box box, BorderRadius radius) => box with { BorderRadius = radius };

    public static Box BorderStyle(this Box box, BorderStyle style) => box with { BorderStyle = style };

    public static Box Padding(this Box box, int padding) => box with { Padding = new(padding) };
    public static Box Padding(this Box box, Thickness padding) => box with { Padding = padding };

    public static Box Margin(this Box box, int margin) => box with { Margin = new(margin) };
    public static Box Margin(this Box box, Thickness margin) => box with { Margin = margin };

    public static Box Content(this Box box, params object[] content) => box with { Children = content };
    public static Box ContentAlign(this Box box, Align? align) => box with { ContentAlign = align };
}