using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets;

[App(icon: Icons.ChevronsUpDown)]
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
            | Text.H2("Problematic Case - Switch in Header")
            | Text.Block("Nested switches should not be blocked by the expandable:")
            | Text.Block($"Switch states: {headerSwitchState1.Value}, {headerSwitchState2.Value}, {headerSwitchState3.Value}, {headerSwitchState4.Value}")
            | switchInHeaderExpandable1
            | switchInHeaderExpandable2
            | switchInHeaderExpandable3
            | switchInHeaderExpandable4;
    }
}