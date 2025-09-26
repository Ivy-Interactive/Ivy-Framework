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

        return
            Layout.Vertical()
                | searchState.ToInput("Search")
                | (loadingState.Value ? "Loading..." : Layout.Wrap(
                    iconsState.Value.Select(e => new Button(null, onIconClick, icon: e, variant: ButtonVariant.Outline).WithTooltip(e.ToString()))))
                ;
    }
}