# Box

A generic container component used to group other components together and apply styles. Rendered as a `div` element in Reflex; rendered as a styled panel with borders and colors in Ivy.

## Reflex

```python
rx.box(
    rx.box(
        "CSS color",
        background_color="yellow",
        border_radius="2px",
        width="20%",
        margin="4px",
        padding="4px",
    ),
    rx.box(
        "CSS color",
        background_color="orange",
        border_radius="5px",
        width="40%",
        margin="8px",
        padding="8px",
    ),
    rx.box(
        "Radix Color",
        background_color="var(--tomato-3)",
        border_radius="5px",
        width="60%",
        margin="12px",
        padding="12px",
    ),
    rx.box(
        "Radix Theme Color",
        background_color="var(--accent-2)",
        radius="full",
        width="100%",
        margin="24px",
        padding="25px",
    ),
    flex_grow="1",
    text_align="center",
)
```

## Ivy

```csharp
public class BoxExampleView : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical().Gap(4)
            | new Box("Solid Border")
                .BorderStyle(BorderStyle.Solid)
                .BorderRadius(BorderRadius.Rounded)
                .BorderThickness(2)
                .Padding(8)
            | new Box("Dashed Border")
                .BorderStyle(BorderStyle.Dashed)
                .Padding(8)
            | new Box("With Color")
                .Color(Colors.Green)
                .BorderRadius(BorderRadius.Full)
                .Padding(12)
            | new Box("Card Style")
                .Color(Colors.White)
                .BorderRadius(BorderRadius.Rounded)
                .BorderThickness(1)
                .Padding(12)
                .Content(
                    Text.Label("Title"),
                    Text.P("Content inside a box")
                );
    }
}
```

## Parameters

| Parameter | Reflex | Ivy |
|-----------|--------|-----|
| `width` | CSS value (e.g. `"20%"`, `"100px"`) | `Width(Size)` e.g. `Size.Fraction(1/3f)` |
| `height` | CSS value (e.g. `"100%"`, `"10vh"`) | `Height(Size)` |
| `padding` | CSS value (e.g. `"4px"`, `"8px"`) | `Padding(Thickness)` e.g. `Padding(8)` or `Padding(new Thickness(24, 12, 6, 18))` |
| `margin` | CSS value (e.g. `"4px"`, `"8px"`) | `Margin(Thickness)` e.g. `Margin(8)` |
| `background_color` | CSS color or Radix variable (e.g. `"yellow"`, `"var(--tomato-3)"`) | `Color(Colors)` e.g. `Colors.Primary`, `Colors.Green` (theme-adaptive presets only) |
| `background` | CSS background shorthand (gradients, images) | Not supported |
| `border_radius` | CSS value (e.g. `"2px"`, `"5px"`) | `BorderRadius(BorderRadius)` enum: `None`, `Rounded`, `Full` |
| `border_style` | CSS value via `style` prop | `BorderStyle(BorderStyle)` enum: `Solid`, `Dashed`, `Dotted`, `None` |
| `border_width` | CSS value via `style` prop | `BorderThickness(Thickness)` e.g. `BorderThickness(2)` |
| `text_align` | CSS value (e.g. `"center"`) | `ContentAlign(Align)` |
| `opacity` | CSS value via `style` prop | `Opacity(float)` |
| `flex_grow` | CSS value (e.g. `"1"`) | Not supported |
| `radius` | Radix preset (e.g. `"full"`) | Mapped to `BorderRadius` enum |
| `scale` | Not a direct prop | `Scale(Scale)` |
| `visible` | Conditional rendering | `Visible(bool)` |
| Event triggers | Full set of DOM event triggers (`on_click`, `on_mouse_enter`, etc.) | Not documented on Box |
