using ExternalWidgetExample;
using Ivy.Shared;
using Ivy.Views;

namespace Ivy.Samples.Shared.Apps.Concepts;

[App(icon: Icons.Puzzle, searchHints: ["external", "plugin", "nuget", "package", "custom", "widget"])]
public class ExternalWidgetsApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Tabs(
            new Tab("Basic", new BasicDemo()),
            new Tab("Nested", new NestedDemo())
        ).Variant(TabsVariant.Content);
    }
}

public class BasicDemo : ViewBase
{
    public override object? Build()
    {
        var clickedIndex = UseState<int?>(-1);
        var chartColor = UseState("#3b82f6");
        var data = new double[] { 12, 19, 8, 25, 32, 18, 42, 35, 28, 45, 38, 52 };

        return Layout.Vertical(
            Text.H3("Interactive Chart"),
            Text.P("Click on any bar to trigger an event back to the C# backend.").Muted(),

            new SuperChart("Monthly Sales 2024", data)
                .Color(chartColor.Value)
                .ShowLabels()
                .HandlePointClick(index => clickedIndex.Value = index),

            clickedIndex.Value >= 0
                ? Callout.Info($"You clicked bar at index {clickedIndex.Value} (value: {data[clickedIndex.Value.Value]})")
                : Callout.Info("Click a bar to see the event handling in action"),

            new Separator(),

            Text.H4("Change Chart Color"),
            Layout.Horizontal(
                new Button("Blue", _ => chartColor.Value = "#3b82f6").Variant(ButtonVariant.Outline),
                new Button("Green", _ => chartColor.Value = "#10b981").Variant(ButtonVariant.Outline),
                new Button("Purple", _ => chartColor.Value = "#8b5cf6").Variant(ButtonVariant.Outline),
                new Button("Orange", _ => chartColor.Value = "#f59e0b").Variant(ButtonVariant.Outline)
            ).Gap(2)
        );
    }
}

public class NestedDemo : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical(
            Text.H3("Nested External Widgets"),
            Text.P("Foo is a container widget that can hold Bar children, demonstrating widget composition with external widgets.").Muted(),
            new Foo("Programming Languages",
                new Bar("C#").Color("#68217a"),
                new Bar("TypeScript").Color("#3178c6"),
                new Bar("Python").Color("#3776ab"),
                new Bar("Rust").Color("#dea584")
            )
        );
    }
}