---
searchHints:
  - lucide
  - symbols
  - graphics
  - glyphs
  - svg
  - icons
---

# Icon

<Ingress>
Display beautiful vector icons from the comprehensive Lucide icon set with customizable colors, sizes, and styling options.
</Ingress>

The `Icon` [widget](../../01_Onboarding/02_Concepts/03_Widgets.md) displays vector icons from Ivy's built-in icon set. Icons enhance visual communication and can be customized with different colors and sizes.

## Lucide Icons

We use the [Lucide Icons](https://lucide.dev/icons/) set, which is a collection of 1000+ free icons. You can find the full set [here](https://lucide.dev/icons/).

```csharp demo-tabs
public class LucideIconsView : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var searchState = this.UseState("code");
        var iconsState = this.UseState<Icons[]>(Array.Empty<Icons>());
        
        UseEffect(() =>
        {
            var allIcons = Enum.GetValues<Icons>().Where(e => e != Icons.None);
            iconsState.Set(string.IsNullOrEmpty(searchState.Value)
                ? []
                : allIcons.Where(e => e.ToString().Contains(searchState.Value, StringComparison.OrdinalIgnoreCase)).Take(10).ToArray());
        }, [ EffectTrigger.OnMount(), searchState.Throttle(TimeSpan.FromMilliseconds(500)).ToTrigger() ]);
        
        var searchInput = searchState.ToSearchInput().Placeholder("Type an icon name");
        
        var icons = iconsState.Value.Select(e => Layout.Horizontal().Gap(2)
            | new Button(null, @event =>
            {
                var iconCode = "Icons." + e.ToString();
                client.CopyToClipboard(iconCode);
                client.Toast($"Copied '{iconCode}' to clipboard", "Icon Code Copied");
            }, ButtonVariant.Ghost, e).Small().WithTooltip($"Click to copy {e.ToString()}")
            | Text.Label("Icons." + e.ToString())
        );

        return Layout.Vertical()
               | searchInput
               | icons
            ; 
    }
}
```

## Colors

You can customize the color of the icons using the [`Color`](../../04_ApiReference/IvyShared/Colors.md) parameter.

```csharp demo-tabs
Layout.Horizontal()
    | new Icon(Icons.Clipboard, Colors.Red)
    | new Icon(Icons.Settings, Colors.Green)
    | new Icon(Icons.User, Colors.Blue)
    | new Icon(Icons.Calendar, Colors.Purple)
    | new Icon(Icons.Mail, Colors.Orange)
```

## Sizes

You can also customize the size of the icons using the [`Size`](../../04_ApiReference/IvyShared/Size.md) parameter.

```csharp demo-tabs
Layout.Horizontal()
    | new Icon(Icons.Cloud).Small()
    | new Icon(Icons.Cloud)
    | new Icon(Icons.Cloud).Large()
```

<WidgetDocs Type="Ivy.Icon" ExtensionTypes="Ivy.IconExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/src/Ivy/Widgets/Primitives/Icon.cs"/>
