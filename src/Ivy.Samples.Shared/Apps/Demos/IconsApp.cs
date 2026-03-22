using System.Reactive.Linq;

namespace Ivy.Samples.Shared.Apps.Demos;

[App(icon: Icons.Star, group: ["Demos"], searchHints: ["lucide", "symbols", "graphics", "glyphs", "library", "search"])]
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

        Action<Event<Baton>> onIconClick = e =>
        {
            client.CopyToClipboard(e.Sender.Icon.ToString() ?? "");
            client.Toast($"Copied '{e.Sender.Icon.ToString()}' to clipboard", "Icon Copied");
        };

        return
            Layout.Vertical()
                | searchState.ToInput("Search")
                | (loadingState.Value ? "Loading..." : Layout.Wrap(
                    iconsState.Value.Select(e => new Baton(null, onIconClick, icon: e, variant: BatonVariant.Outline).WithTooltip(e.ToString()))))
                ;
    }
}
