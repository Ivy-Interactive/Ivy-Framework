using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Represents a demo box widget that provides a simplified container for demonstration and testing purposes.
/// This widget offers basic styling capabilities including borders, padding, margins, and content alignment,
/// making it ideal for creating example layouts, testing design patterns, and building demonstration interfaces.
/// 
/// DemoBox is designed as a lightweight alternative to the full Box widget, providing essential styling
/// properties while maintaining simplicity for demonstration scenarios. This widget is not intended for production use.
/// </summary>
public record DemoBox : WidgetBase<DemoBox>
{
    /// <summary>
    /// Initializes a new instance of the DemoBox class with the specified content.
    /// </summary>
    /// <param name="content">Variable number of content elements to display within the demo box.</param>
    public DemoBox(params IEnumerable<object> content) : base(content.ToArray())
    {
    }

    /// <summary>
    /// Gets or sets the thickness of the border around the demo box.
    /// </summary>
    [Prop] public Thickness BorderThickness { get; set; } = new(1);

    /// <summary>
    /// Gets or sets the border radius for the demo box corners.
    /// </summary>
    [Prop] public BorderRadius BorderRadius { get; set; } = BorderRadius.Rounded;

    /// <summary>
    /// Gets or sets the style of the border around the demo box.
    /// </summary>
    [Prop] public BorderStyle BorderStyle { get; set; } = BorderStyle.Solid;

    /// <summary>
    /// Gets or sets the internal spacing within the demo box.
    /// </summary>
    [Prop] public Thickness Padding { get; set; } = new(4);

    /// <summary>
    /// Gets or sets the external spacing around the demo box.
    /// </summary>
    [Prop] public Thickness Margin { get; set; } = new(0);

    /// <summary>
    /// Gets or sets the alignment of content within the demo box.
    /// </summary>
    [Prop] public Align? ContentAlign { get; set; } = Align.TopLeft;
}

/// <summary>
/// Extension methods for the DemoBox class that provide a fluent API for easy configuration.
/// </summary>
public static class DemoBoxExtensions
{
    public static DemoBox BorderThickness(this DemoBox demoBox, int thickness) => demoBox with { BorderThickness = new(thickness) };

    public static DemoBox BorderThickness(this DemoBox demoBox, Thickness thickness) => demoBox with { BorderThickness = thickness };

    public static DemoBox BorderRadius(this DemoBox demoBox, BorderRadius radius) => demoBox with { BorderRadius = radius };

    public static DemoBox BorderStyle(this DemoBox demoBox, BorderStyle style) => demoBox with { BorderStyle = style };

    public static DemoBox Padding(this DemoBox demoBox, int padding) => demoBox with { Padding = new(padding) };

    public static DemoBox Padding(this DemoBox demoBox, Thickness padding) => demoBox with { Padding = padding };

    public static DemoBox Margin(this DemoBox demoBox, int margin) => demoBox with { Margin = new(margin) };

    public static DemoBox Margin(this DemoBox demoBox, Thickness margin) => demoBox with { Margin = margin };

    public static DemoBox Content(this DemoBox demoBox, params object[] content) => demoBox with { Children = content };

    public static DemoBox ContentAlign(this DemoBox demoBox, Align? align) => demoBox with { ContentAlign = align };
}
