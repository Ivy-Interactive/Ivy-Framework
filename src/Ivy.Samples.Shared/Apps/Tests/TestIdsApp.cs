namespace Ivy.Samples.Shared.Apps.Tests;

[App(icon: Icons.Code, group: ["Tests"], isVisible: false, searchHints: ["testid", "test-id", "e2e"])]
public class TestIdsApp : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical(
            Text.H1("TestId Support").TestId("heading"),
            Text.P("Each element below has a TestId set.").TestId("description"),
            Layout.Horizontal(
                Text.P("Left"),
                Text.P("Right")
            ).TestId("horizontal-layout"),
            Layout.Grid(
                Text.P("Cell 1"),
                Text.P("Cell 2"),
                Text.P("Cell 3"),
                Text.P("Cell 4")
            ).Columns(2).TestId("grid-layout"),
            Layout.Tabs(
                new Tab("Tab 1", Text.P("Content 1")),
                new Tab("Tab 2", Text.P("Content 2"))
            ).TestId("tabs-layout")
        ).TestId("root-layout");
    }
}

