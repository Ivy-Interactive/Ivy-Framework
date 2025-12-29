using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets;

[App(icon: Icons.ChevronsUpDown, searchHints: ["accordion", "collapse", "expand", "toggle", "disclosure", "details"])]
public class ExpandableApp : SampleBase
{
    protected override object? BuildSample()
    {
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

        var toggleableExpandable1 = new Expandable(
            Layout.Horizontal()
            | Text.Block("Apps")
            | new Icon(Icons.ChevronRight)
            | new Icon(Icons.Paperclip)
            | Text.Block("Attachments"),
            Text.Block("This is the content for Attachments")
        ).Disabled(true);

        var toggleableExpandable2 = new Expandable(
            Layout.Horizontal()
            | Text.Block("Apps")
            | new Icon(Icons.ChevronRight)
            | new Icon(Icons.MessageCircle)
            | Text.Block("Comments"),
            Text.Block("This is the content for Comments")
        ).Disabled(true);

        var toggleableExpandable3 = new Expandable(
            Layout.Horizontal()
            | Text.Block("Apps")
            | new Icon(Icons.ChevronRight)
            | new Icon(Icons.Bug)
            | Text.Block("Issues"),
            Text.Block("This is the content for Issues")
        ).Disabled(true);

        var toggleableExpandable4 = new Expandable(
            Layout.Horizontal()
            | Text.Block("Settings")
            | new Icon(Icons.ChevronRight)
            | new Icon(Icons.Users)
            | Text.Block("Project Users"),
            Text.Block("This is the content for Project Users")
        ).Disabled(true);

        return Layout.Vertical()
            | Text.H2("Basic Expandable")
            | basicExpandable
            | Text.H2("Scale Variations")
            | Text.Block("Use the Scale helpers (Small / Medium / Large) to match the density of the surrounding layout.")
            | smallScaleExpandable
            | mediumScaleExpandable
            | largeScaleExpandable
            | Text.H2("Toggleable Expandable")
            | Text.Block("Use Disabled(true) to add a switch that controls the disabled state:")
            | toggleableExpandable1
            | toggleableExpandable2
            | toggleableExpandable3
            | toggleableExpandable4;
    }
}
