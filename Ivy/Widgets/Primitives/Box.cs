using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Container widget with borders, padding, margins, colors, and content alignment. Default: primary border, rounded, centered.</summary>
public record Box : WidgetBase<Box>
{
    /// <summary>Initializes box container.</summary>
    /// <param name="content">Content elements.</param>
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
    /// <summary>Sets box color.</summary>
    public static Box Color(this Box box, Colors color) => box with { Color = color };

    /// <summary>Sets uniform border thickness.</summary>
    public static Box BorderThickness(this Box box, int thickness) => box with { BorderThickness = new(thickness) };

    /// <summary>Sets border thickness per side.</summary>
    public static Box BorderThickness(this Box box, Thickness thickness) => box with { BorderThickness = thickness };

    /// <summary>Sets border radius.</summary>
    public static Box BorderRadius(this Box box, BorderRadius radius) => box with { BorderRadius = radius };

    /// <summary>Sets border style.</summary>
    public static Box BorderStyle(this Box box, BorderStyle style) => box with { BorderStyle = style };

    /// <summary>Sets uniform padding.</summary>
    public static Box Padding(this Box box, int padding) => box with { Padding = new(padding) };

    /// <summary>Sets padding per side.</summary>
    public static Box Padding(this Box box, Thickness padding) => box with { Padding = padding };

    /// <summary>Sets uniform margin.</summary>
    public static Box Margin(this Box box, int margin) => box with { Margin = new(margin) };

    /// <summary>Sets margin per side.</summary>
    public static Box Margin(this Box box, Thickness margin) => box with { Margin = margin };

    /// <summary>Sets box content.</summary>
    public static Box Content(this Box box, params object[] content) => box with { Children = content };

    /// <summary>Sets content alignment.</summary>
    public static Box ContentAlign(this Box box, Align? align) => box with { ContentAlign = align };
}