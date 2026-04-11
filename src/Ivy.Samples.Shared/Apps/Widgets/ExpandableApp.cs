
namespace Ivy.Samples.Shared.Apps.Widgets;

[App(icon: Icons.ChevronsUpDown, searchHints: ["accordion", "collapse", "expand", "toggle", "disclosure", "details"])]
public class ExpandableApp : SampleBase
{
    protected override object? BuildSample()
    {
        // PROBLEMATIC CASE - Switch in Header (replicates the exact issue from HTML)
        var headerSwitchState1 = UseState(false);
        var headerSwitchState2 = UseState(true);
        var headerSwitchState3 = UseState(true);
        var headerSwitchState4 = UseState(false);

        // Original basic expandable
        var basicExpandable = new Expandable("This is an expandable", "This is the content of the expandable");

        // Expandable with icon
        var iconExpandable = new Expandable("Settings", "Configure your application preferences here.")
            .Icon(Icons.Settings);

        var smallIconExpandable = new Expandable("Small Settings", "Configure your application preferences here.")
            .Icon(Icons.Settings)
            .Small();

        var mediumIconExpandable = new Expandable("Medium Settings", "Configure your application preferences here.")
            .Icon(Icons.Settings)
            .Medium();

        var largeIconExpandable = new Expandable("Large Settings", "Configure your application preferences here.")
            .Icon(Icons.Settings)
            .Large();

        object BuildDensityContent(string emphasis, string body)
        {
            return Layout.Vertical()
                | Text.Block(emphasis)
                | Text.Block(body);
        }

        var smallDensityExpandable = new Expandable(
            Text.Block("Small scale (compact task list)"),
            BuildDensityContent(
                "Ideal where space is at a premium.",
                "Tighter padding keeps related details visible without overwhelming the page.")
        ).Small();

        var mediumDensityExpandable = new Expandable(
            Text.Block("Medium scale (default)"),
            BuildDensityContent(
                "Balanced defaults for most layouts.",
                "Comfortable spacing that pairs well with mixed content like text, lists or buttons.")
        ).Medium();

        var largeDensityExpandable = new Expandable(
            Text.Block("Large scale (emphasis)"),
            BuildDensityContent(
                "Use when the header should stand out.",
                "Generous spacing gives the content breathing room and improves readability.")
        ).Large();

        var switchInHeaderExpandable1 = new Expandable(
            Layout.Horizontal().AlignContent(Align.Left)
            | headerSwitchState1.ToBoolInput(variant: BoolInputVariant.Switch)
            | (Layout.Horizontal().AlignContent(Align.Left)
               | Text.Block("Apps")
               | new Icon(Icons.ChevronRight)
               | new Icon(Icons.Paperclip)
               | Text.Block("Attachments")),
            Text.Block("This is the content for Attachments")
        ).Disabled(true);

        var switchInHeaderExpandable2 = new Expandable(
            Layout.Horizontal().AlignContent(Align.Left)
            | headerSwitchState2.ToBoolInput(variant: BoolInputVariant.Switch)
            | (Layout.Horizontal().AlignContent(Align.Left)
               | Text.Block("Apps")
               | new Icon(Icons.ChevronRight)
               | new Icon(Icons.MessageCircle)
               | Text.Block("Comments")),
            Text.Block("This is the content for Comments")
        ).Disabled(true);

        var switchInHeaderExpandable3 = new Expandable(
            Layout.Horizontal().AlignContent(Align.Left)
            | headerSwitchState3.ToBoolInput(variant: BoolInputVariant.Switch)
            | (Layout.Horizontal().AlignContent(Align.Left)
               | Text.Block("Apps")
               | new Icon(Icons.ChevronRight)
               | new Icon(Icons.Bug)
               | Text.Block("Issues")),
            Text.Block("This is the content for Issues")
        );

        var switchInHeaderExpandable4 = new Expandable(
            Layout.Horizontal().AlignContent(Align.Left)
            | headerSwitchState4.ToBoolInput(variant: BoolInputVariant.Switch)
            | (Layout.Horizontal().AlignContent(Align.Left)
               | Text.Block("Settings")
               | new Icon(Icons.ChevronRight)
               | new Icon(Icons.Users)
               | Text.Block("Project Users")),
            Text.Block("This is the content for Project Users")
        ).Disabled(true);

        var ghostExpandable = new Expandable("Ghost Expandable", "This expandable uses the Ghost variant — minimal border, no shadow.")
            .Ghost();

        var ghostWithIconExpandable = new Expandable("Ghost with Icon", "Ghost variant combined with an icon.")
            .Ghost()
            .Icon(Icons.Ghost);

        return Layout.Vertical()
            | Text.H2("Original Basic Expandable")
            | basicExpandable
            | Text.H2("Expandable with Icon")
            | iconExpandable
            | Text.H2("Expandable with Icon + Density Variations")
            | smallIconExpandable
            | mediumIconExpandable
            | largeIconExpandable
            | Text.H2("Density Variations")
            | Text.Block("Use the Density helpers (Small / Medium / Large) to match the density of the surrounding layout.")
            | smallDensityExpandable
            | mediumDensityExpandable
            | largeDensityExpandable
            | Text.H2("Problematic Case - Switch in Header")
            | Text.Block("Nested switches should not be blocked by the expandable:")
            | Text.Block($"Switch states: {headerSwitchState1.Value}, {headerSwitchState2.Value}, {headerSwitchState3.Value}, {headerSwitchState4.Value}")
            | switchInHeaderExpandable1
            | switchInHeaderExpandable2
            | switchInHeaderExpandable3
            | switchInHeaderExpandable4
            | Text.H2("Ghost Variant")
            | ghostExpandable
            | ghostWithIconExpandable;
    }
}
