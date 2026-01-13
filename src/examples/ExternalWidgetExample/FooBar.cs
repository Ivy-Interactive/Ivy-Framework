using Ivy;
using Ivy.Core;
using Ivy.Core.ExternalWidgets;

namespace ExternalWidgetExample;

[ExternalWidget("frontend/dist/ExternalWidgets.js", ExportName = "Bar")]
public record Bar : WidgetBase<Bar>
{
    public Bar(string? label = null)
    {
        Label = label;
    }

    internal Bar()
    {
    }

    [Prop] public string? Label { get; set; }

    [Prop] public string Color { get; set; } = "#10b981";
}

[ExternalWidget("frontend/dist/ExternalWidgets.js", ExportName = "Foo")]
public record Foo : WidgetBase<Foo>
{
    public Foo(string? title = null, params Bar[] children)
        : base(children)
    {
        Title = title;
    }

    internal Foo()
    {
    }

    [Prop] public string? Title { get; set; }
}

public static class BarExtensions
{
    public static Bar Label(this Bar bar, string label) =>
        bar with { Label = label };

    public static Bar Color(this Bar bar, string color) =>
        bar with { Color = color };
}

public static class FooExtensions
{
    public static Foo Title(this Foo foo, string title) =>
        foo with { Title = title };
}
