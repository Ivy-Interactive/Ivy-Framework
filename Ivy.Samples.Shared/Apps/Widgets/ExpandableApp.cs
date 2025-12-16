using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets;

[App(icon: Icons.ChevronsUpDown, searchHints: ["accordion", "collapse", "expand", "toggle", "disclosure", "details"])]
public class ExpandableApp : SampleBase
{
    protected override object? BuildSample()
    {
        // Original basic expandable
        var basicExpandable = new Expandable("This is an expandable", "This is the content of the expandable");

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

        // Built-in enable toggle expandable
        var enableToggleExpandable = new Expandable(
            Layout.Horizontal()
            | Text.Block("Apps")
            | new Icon(Icons.ChevronRight)
            | new Icon(Icons.Bell)
            | Text.Block("Notifications"),
            Text.Block("Configure your notification preferences here.")
        ).WithEnableToggle();

        return Layout.Vertical()
            | Text.H2("Original Basic Expandable")
            | basicExpandable
            | Text.H2("Scale Variations")
            | Text.Block("Use the Scale helpers (Small / Medium / Large) to match the density of the surrounding layout.")
            | smallScaleExpandable
            | mediumScaleExpandable
            | largeScaleExpandable
            | Text.H2("Built-in Enable Toggle")
            | Text.Block("Use WithEnableToggle(state) to add a switch that controls the enabled state:")
            | enableToggleExpandable;
    }
}
