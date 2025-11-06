using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Container widget with borders, padding, margins, colors, and content alignment. Default: primary border, rounded, centered.</summary>
public record Box : WidgetBase<Box>
{
    /// <summary>Initializes box container.</summary>
    public Box(params IEnumerable<object> content) : base(content.ToArray())
    {
    }

    /// <summary>Border color. Default: Primary.</summary>
    [Prop] public Colors? Color { get; set; } = Colors.Primary;

    /// <summary>Border thickness. Default: 2 units all sides.</summary>
    [Prop] public Thickness BorderThickness { get; set; } = new(2);

    /// <summary>Border radius. Default: Rounded.</summary>
    [Prop] public BorderRadius BorderRadius { get; set; } = BorderRadius.Rounded;

    /// <summary>Border style. Default: Solid.</summary>
    [Prop] public BorderStyle BorderStyle { get; set; } = BorderStyle.Solid;

    /// <summary>Internal padding. Default: 2 units all sides.</summary>
    [Prop] public Thickness Padding { get; set; } = new(2);

    /// <summary>External margin. Default: 0.</summary>
    [Prop] public Thickness Margin { get; set; } = new(0);

    /// <summary>Content alignment. Default: Center.</summary>
    [Prop] public Align? ContentAlign { get; set; } = Align.Center;
}

/// <summary>Extension methods for Box widget configuration.</summary>
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