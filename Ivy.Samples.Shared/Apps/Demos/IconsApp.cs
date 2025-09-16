using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Demos;

[App(icon: Icons.Star, path: ["Demos"])]
public class IconsApp : SampleBase
{
    protected override object? BuildSample()
    {
        var searchState = UseState(string.Empty);
        var iconsState = UseState(Array.Empty<Icons>());
        var loadingState = UseState(false);
        var client = UseService<IClientProvider>();

        UseEffect(() =>
        {
            loadingState.Set(true);
        }, [searchState]);

        UseEffect(() =>
        {
            var allIcons = Enum.GetValues<Icons>().Where(e => e != Icons.None);
            iconsState.Set(string.IsNullOrEmpty(searchState.Value)
                ? []
                : allIcons.Where(e => e.ToString().Contains(searchState.Value, StringComparison.OrdinalIgnoreCase)).Take(50).ToArray());
            loadingState.Set(false);
        }, [searchState.Throttle(TimeSpan.FromMilliseconds(500)).ToTrigger()]);

        Action<Event<Button>> onIconClick = e =>
        {
            client.CopyToClipboard(e.Sender.Icon?.ToString() ?? "");
            client.Toast($"Copied '{e.Sender.Icon?.ToString()}' to clipboard", "Icon Copied");
        };

        // Recently fixed icons - these were updated to match available Lucide icons
        var recentlyFixedIcons = new[]
        {
            (Icons.BadgeQuestionMark, "BadgeHelp → BadgeQuestionMark"),
            (Icons.CircleQuestionMark, "CircleHelp → CircleQuestionMark"),
            (Icons.FileQuestionMark, "FileQuestion → FileQuestionMark"),
            (Icons.FileVideoCamera, "FileVideo → FileVideoCamera"),
            (Icons.ListFilterPlus, "FilterX → ListFilterPlus"),
            (Icons.HandGrab, "Grab → HandGrab"),
            (Icons.Layers2, "Layers3 → Layers2"),
            (Icons.MailQuestionMark, "MailQuestion → MailQuestionMark"),
            (Icons.MessageCircleQuestionMark, "MessageCircleQuestion → MessageCircleQuestionMark"),
            (Icons.ShieldQuestionMark, "ShieldQuestion → ShieldQuestionMark")
        };

        var recentlyFixedSection = Layout.Vertical()
            | Text.H2("Recently Fixed Icons")
            | Text.Block("These icons were updated to match available Lucide icons:")
            | Layout.Wrap(recentlyFixedIcons.Select(iconInfo =>
                Layout.Vertical().Gap(1)
                | new Icon(iconInfo.Item1)
                | Text.Small(iconInfo.Item2)
                | new Button("Copy", _ =>
                {
                    client.CopyToClipboard(iconInfo.Item1.ToString());
                    client.Toast($"Copied '{iconInfo.Item1}' to clipboard", "Icon Copied");
                }, ButtonVariant.Ghost).Small()
            ))
            ;

        return Layout.Vertical().Gap(4)
            | recentlyFixedSection
            | Text.H2("Icon Search")
            | searchState.ToInput("Search icons...")
            | (loadingState.Value ? "Loading..." : Layout.Wrap(
                iconsState.Value.Select(e =>
                    Layout.Vertical().Gap(1)
                    | new Button(null, onIconClick, icon: e, variant: ButtonVariant.Outline).WithTooltip($"Click to copy: {e}")
                    | Text.Small(e.ToString())
                )))
            ;
    }
}