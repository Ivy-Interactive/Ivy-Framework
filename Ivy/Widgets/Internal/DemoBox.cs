using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record DemoBox : WidgetBase<DemoBox>
{
    public DemoBox(params IEnumerable<object> content) : base(content.ToArray())
    {
    }

    [Prop] public Thickness BorderThickness { get; set; } = new(1);

    [Prop] public BorderRadius BorderRadius { get; set; } = BorderRadius.Rounded;

    [Prop] public BorderStyle BorderStyle { get; set; } = BorderStyle.Solid;

    [Prop] public Thickness Padding { get; set; } = new(4);

    [Prop] public Thickness Margin { get; set; } = new(0);

    [Prop] public Align? ContentAlign { get; set; } = Align.TopLeft;
}

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
