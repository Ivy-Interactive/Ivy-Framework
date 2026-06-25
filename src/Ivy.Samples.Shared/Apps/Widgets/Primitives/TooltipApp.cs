
namespace Ivy.Samples.Shared.Apps.Widgets.Primitives;

[App(icon: Icons.MessageSquare, group: ["Widgets"], searchHints: ["hint", "hover", "popover", "help", "info", "overlay"])]
public class TooltipApp : SampleBase
{
    protected override object? BuildSample()
    {
        // Drives a tooltip that appears in response to an event rather than on hover.
        var showError = UseState(false);
        var lastEvent = UseState("No event yet.");

        var validate = new Button("Validate", _ =>
        {
            // A real error event would set this from a failed validation/server call.
            showError.Set(true);
            lastEvent.Set("Validate clicked → error tooltip opened.");
        }, variant: ButtonVariant.Destructive);

        // The tooltip appears on the button's click event, points at it with a bubble,
        // and persists (with a close button) until dismissed.
        var errorTooltip = new Tooltip(validate, "Email address is required.")
            .Open(showError.Value)
            .Bubble()
            .Persist()
            .HandleClose(() =>
            {
                showError.Set(false);
                lastEvent.Set("Error tooltip closed.");
            });

        return Layout.Vertical().Gap(8)
            | Text.H2("Basic")
            | new Tooltip(new Button("Hover Me"), "Hello World!")
            | new Tooltip(new Button("Save"), new Kbd("⌘S"))
            | new Button("Delete").WithTooltip(new Kbd("Del"))

            | Text.H2("Bubble (arrow)")
            | new Tooltip(new Button("Pointing Tooltip"), "I point at my trigger.").Bubble()

            | Text.H2("Persist (open until closed)")
            | new Tooltip(new Button("Hover to pin"), "Hover opens me; click the X to dismiss.").Persist()

            | Text.H2("Appear on an event")
            | Text.Muted(lastEvent.Value)
            | errorTooltip;
    }
}
