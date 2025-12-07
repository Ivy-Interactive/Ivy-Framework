using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets;

[App(icon: Icons.ChevronsUpDown, searchHints: ["accordion", "collapse", "expand", "toggle", "disclosure", "details"])]
public class ExpandableApp : SampleBase
{
    protected override object? BuildSample()
    {
        // Original basic expandable
        var basicExpandable = new Expandable("This is an expandable", "This is the content of the expandable");

        // PROBLEMATIC CASE - Switch in Header (replicates the exact issue from HTML)
        var headerSwitchState1 = UseState(false);
        var headerSwitchState2 = UseState(true);
        var headerSwitchState3 = UseState(true);
        var headerSwitchState4 = UseState(false);

        object BuildScaleContent(string emphasis, string body)
        {
            return Layout.Vertical()
                | Text.Block(emphasis)
                | Text.Block(body);
        }

        var smallScaleExpandable = new Expandable(
            Text.Block("Small scale (compact task list)"),
            BuildScaleContent(
                "Ideal where space is at a premium.",
                "Tighter padding keeps related details visible without overwhelming the page.")
        ).Small();

        var mediumScaleExpandable = new Expandable(
            Text.Block("Medium scale (default)"),
            BuildScaleContent(
                "Balanced defaults for most layouts.",
                "Comfortable spacing that pairs well with mixed content like text, lists or buttons.")
        ).Medium();

        var largeScaleExpandable = new Expandable(
            Text.Block("Large scale (emphasis)"),
            BuildScaleContent(
                "Use when the header should stand out.",
                "Generous spacing gives the content breathing room and improves readability.")
        ).Large();

        var switchInHeaderExpandable1 = new Expandable(
            Layout.Horizontal()
            | headerSwitchState1.ToBoolInput(variant: BoolInputs.Switch)
            | (Layout.Horizontal()
               | Text.Block("Apps")
               | new Icon(Icons.ChevronRight)
               | new Icon(Icons.Paperclip)
               | Text.Block("Attachments")),
            Text.Block("This is the content for Attachments")
        ).Disabled(true);

        var switchInHeaderExpandable2 = new Expandable(
            Layout.Horizontal()
            | headerSwitchState2.ToBoolInput(variant: BoolInputs.Switch)
            | (Layout.Horizontal()
               | Text.Block("Apps")
               | new Icon(Icons.ChevronRight)
               | new Icon(Icons.MessageCircle)
               | Text.Block("Comments")),
            Text.Block("This is the content for Comments")
        ).Disabled(true);

        var switchInHeaderExpandable3 = new Expandable(
            Layout.Horizontal()
            | headerSwitchState3.ToBoolInput(variant: BoolInputs.Switch)
            | (Layout.Horizontal()
               | Text.Block("Apps")
               | new Icon(Icons.ChevronRight)
               | new Icon(Icons.Bug)
               | Text.Block("Issues")),
            Text.Block("This is the content for Issues")
        );

        var switchInHeaderExpandable4 = new Expandable(
            Layout.Horizontal()
            | headerSwitchState4.ToBoolInput(variant: BoolInputs.Switch)
            | (Layout.Horizontal()
               | Text.Block("Settings")
               | new Icon(Icons.ChevronRight)
               | new Icon(Icons.Users)
               | Text.Block("Project Users")),
            Text.Block("This is the content for Project Users")
        ).Disabled(true);

        return Layout.Vertical()
            | Text.H2("Original Basic Expandable")
            | basicExpandable
            | Text.H2("Scale Variations")
            | Text.Block("Use the Scale helpers (Small / Medium / Large) to match the density of the surrounding layout.")
            | smallScaleExpandable
            | mediumScaleExpandable
            | largeScaleExpandable
            | Text.H2("Problematic Case - Switch in Header")
            | Text.Block("Nested switches should not be blocked by the expandable:")
            | Text.Block($"Switch states: {headerSwitchState1.Value}, {headerSwitchState2.Value}, {headerSwitchState3.Value}, {headerSwitchState4.Value}")
            | switchInHeaderExpandable1
            | switchInHeaderExpandable2
            | switchInHeaderExpandable3
            | switchInHeaderExpandable4;
    }
}