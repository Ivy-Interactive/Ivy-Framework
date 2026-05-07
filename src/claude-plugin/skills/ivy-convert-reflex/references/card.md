# Card

A container component used for grouping related content and actions. It provides a bordered, themed surface with consistent spacing and border radius.

## Reflex

```python
rx.flex(
    rx.card("Card 1", size="1"),
    rx.card("Card 2", size="2"),
    rx.card("Card 3", size="3"),
    rx.card("Card 4", size="4"),
    rx.card("Card 5", size="5"),
    spacing="2",
    align_items="flex-start",
    flex_wrap="wrap",
)

# Clickable card using as_child
rx.card(
    rx.link(
        rx.flex(
            rx.avatar(src="/reflex_banner.png"),
            rx.box(
                rx.heading("Quick Start"),
                rx.text("Get started with Reflex in 5 minutes."),
            ),
            spacing="2",
        ),
    ),
    as_child=True,
)
```

## Ivy

```csharp
new Card(
    "Lorem ipsum dolor sit amet, consectetur adipiscing elit.",
    new Button("Sign Me Up", _ => client.Toast("You have signed up!"))
).Title("Card App").Description("This is a card app.").Width(Size.Units(100))

// Clickable card
new Card(
    "This card is clickable."
).Title("Clickable Card")
 .Description("Demonstrating click and mouse hover.")
 .OnClick(_ => client.Toast("Card clicked!"))
 .Width(Size.Units(100))
```

## Parameters

| Parameter      | Documentation                                                                 | Ivy                                                    |
|----------------|-------------------------------------------------------------------------------|--------------------------------------------------------|
| `size`         | Controls spacing/margin on a `"1"` - `"5"` Radix scale                       | `Scale` (scaling option)                               |
| `variant`      | Visual style: `"surface"` or `"classic"`                                      | `HoverVariant` (hover styling only)                    |
| `as_child`     | Renders the Card as a child element (e.g. link or button) to make it clickable | `OnClick` (click handler achieves similar result)  |
| `content`      | Passed as children to `rx.card(...)`                                          | Passed as constructor arguments                        |
| `title`        | Not supported (use child heading component)                                   | `.Title("...")` fluent method                          |
| `description`  | Not supported (use child text component)                                      | `.Description("...")` fluent method                    |
| `header`       | Not supported (compose with child components)                                 | `header` property                                      |
| `footer`       | Not supported (compose with child components)                                 | `footer` property                                      |
| `width`        | Set via `style` or `width` CSS prop                                           | `.Width(Size.Units(n))`                                |
| `height`       | Set via `style` or `height` CSS prop                                          | `.Height(Size.Units(n))`                               |
| `visible`      | Controlled via conditional rendering                                          | `.Visible(bool)`                                       |
