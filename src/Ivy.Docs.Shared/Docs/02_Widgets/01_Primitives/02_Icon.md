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
Display crisp Lucide vector icons with customizable colors, sizes, and animation-ready styling. Use [Colors](../../04_ApiReference/Ivy/Colors.md) and the [Animation](../05_Effects/Animation.md) widget with `.ToIcon().Color().WithAnimation()` to create expressive, interactive icons.
</Ingress>

The `Icon` [widget](../../01_Onboarding/02_Concepts/03_Widgets.md) displays vector icons from Ivy's built-in icon set. Icons enhance visual communication and can be customized with different colors, sizes, and animations.

## Lucide Icons

We use the [Lucide Icons](https://lucide.dev/icons/) set, which is a collection of 1000+ free icons. You can find the full set [here](https://lucide.dev/icons/).

```csharp demo-tabs
public class LucideIconsView : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        var searchState = UseState("code");
        var iconsState = UseState<Icons[]>(Array.Empty<Icons>());
        
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

You can customize the color of the icons using the [`Color`](../../04_ApiReference/Ivy/Colors.md) parameter.

```csharp demo-tabs
Layout.Horizontal()
    | new Icon(Icons.Clipboard, Colors.Red)
    | new Icon(Icons.Settings, Colors.Green)
    | new Icon(Icons.User, Colors.Blue)
    | new Icon(Icons.Calendar, Colors.Purple)
    | new Icon(Icons.Mail, Colors.Orange)
```

## Sizes

You can also customize the size of the icons using the [`Size`](../../04_ApiReference/Ivy/Size.md) parameter.

```csharp demo-tabs
Layout.Horizontal()
    | new Icon(Icons.Cloud).Small()
    | new Icon(Icons.Cloud)
    | new Icon(Icons.Cloud).Large()
```

## Animation

You can animate icons using the [`Animation`](../../02_Widgets/05_Effects/Animation.md) widget and the `.WithAnimation()` extension methods.

```csharp demo-tabs
Layout.Horizontal().Align(Align.Center)
    | Icons.LoaderCircle
        .ToIcon()
        .Color(Colors.Blue)
        .WithAnimation(AnimationType.Rotate)
        .Trigger(AnimationTrigger.Auto)
        .Duration(1)
    | Icons.Heart
        .ToIcon()
        .Color(Colors.Red)
        .WithAnimation(AnimationType.Pulse)
        .Trigger(AnimationTrigger.Click)
    | Icons.MousePointer
        .ToIcon()
        .Color(Colors.Blue)
        .WithAnimation(AnimationType.Hover)
        .Trigger(AnimationTrigger.Hover)
    | Icons.Target
        .ToIcon()
        .Color(Colors.Green)
        .WithAnimation(AnimationType.Pulse)
        .Trigger(AnimationTrigger.Hover)
        .Duration(0.5)
    | Icons.Bell
        .ToIcon()
        .Color(Colors.Orange)
        .WithAnimation(AnimationType.Shake)
        .Trigger(AnimationTrigger.Click)
        .Duration(0.6)
    | Icons.Rocket
        .ToIcon()
        .Color(Colors.Red)
        .WithAnimation(AnimationType.Bounce)
        .Trigger(AnimationTrigger.Click)
        .Duration(0.8)
    | Icons.Gift
        .ToIcon()
        .Color(Colors.Pink)
        .WithAnimation(AnimationType.Bounce)
        .Trigger(AnimationTrigger.Hover)
        .Duration(0.7)
```

## Faq

<Details>
<Summary>
How do I change the size of an Icon?
</Summary>
<Body>

Use the `.Small()` or `.Large()` extension methods:

```csharp
new Icon(Icons.Star).Small()   // small icon
new Icon(Icons.Star)            // default size
new Icon(Icons.Star).Large()   // large icon
```

**Important:** There is no `.WithIconSize()` method or `IconSize` enum. Use the simple `.Small()` and `.Large()` fluent modifiers.

</Body>
</Details>

<WidgetDocs Type="Ivy.Icon" ExtensionTypes="Ivy.IconExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/src/Ivy/Widgets/Primitives/Icon.cs" Docs="../../02_Widgets/05_Effects/Animation.md"/>
