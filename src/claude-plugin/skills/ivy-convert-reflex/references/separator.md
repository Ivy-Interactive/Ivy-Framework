# Separator

Visually or semantically separates content. Creates a divider line between sections to organize information and improve readability.

## Reflex

```python
rx.vstack(
    rx.text("Section A"),
    rx.separator(size="4"),
    rx.text("Section B"),
    rx.separator(size="4", color_scheme="red"),
    rx.text("Section C"),
)
```

## Ivy

```csharp
Layout.Vertical().Gap(2)
    | Text.H2("Personal Information")
    | (Layout.Horizontal().Gap(2)
        | Text.Strong("Name:")
        | Text.Inline("Alex Johnson"))

    | new Separator()

    | Text.H2("Work Information")
    | (Layout.Horizontal().Gap(2)
        | Text.Strong("Role:")
        | Text.Inline("Senior Developer"))

    | new Separator()

    | Text.H2("Skills")
    | (Layout.Horizontal().Gap(2).Wrap(true)
        | new Badge("C#").Variant(BadgeVariant.Secondary)
        | new Badge("React").Variant(BadgeVariant.Secondary))
```

## Parameters

| Parameter      | Documentation                                                        | Ivy                                            |
|----------------|----------------------------------------------------------------------|-------------------------------------------------|
| `size`         | `"1"` to `"4"` - controls separator length; `"4"` fills container   | Not supported                                   |
| `color_scheme` | Color theme (e.g. `"tomato"`, `"red"`)                               | Not supported                                   |
| `orientation`  | `"horizontal"` \| `"vertical"` - layout direction                    | `Orientation` (`Horizontal` \| `Vertical`)      |
| `decorative`   | `bool` - marks as purely decorative for accessibility                | Not supported                                   |
| `text`         | Not supported                                                        | `string` - optional label text on the separator |
