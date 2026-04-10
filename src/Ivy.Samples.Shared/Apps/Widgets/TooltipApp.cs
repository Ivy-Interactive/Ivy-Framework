
namespace Ivy.Samples.Shared.Apps.Widgets;

[App(icon: Icons.MessageSquare, group: ["Widgets"], searchHints: ["hint", "hover", "popover", "help", "info", "overlay"])]
public class TooltipApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Vertical()
            | new Tooltip(new Button("Hover Me"), "Hello World!")
            | new Tooltip(new Button("Save"), new Kbd("⌘S"))
            | new Button("Delete").WithTooltip(new Kbd("Del"));
    }
}
