# Container

Constrains the maximum width of page content while keeping flexible margins for responsive layouts. Typically used to wrap the main content of a page.

## Reflex

```python
rx.box(
    rx.container(
        rx.card("This content is constrained to a max width of 448px.", width="100%"),
        size="1",
    ),
    rx.container(
        rx.card("This content is constrained to a max width of 688px.", width="100%"),
        size="2",
    ),
    rx.container(
        rx.card("This content is constrained to a max width of 880px.", width="100%"),
        size="3",
    ),
    rx.container(
        rx.card("This content is constrained to a max width of 1136px.", width="100%"),
        size="4",
    ),
    background_color="var(--gray-3)",
    width="100%",
)
```

## Ivy

Ivy does not have a dedicated Container widget. The closest equivalent is `Box`, a general-purpose container with manual width control. There are no preset size tiers that map to max-width breakpoints.

```csharp
new Box(
    new Text("This content is constrained by the box width.")
)
.Width(Size.Fraction(2 / 3f))
.Padding(4)
.Margin(new Thickness(horizontal: 0)) // auto-centering not built-in
```

## Parameters

| Parameter          | Documentation                                         | Ivy                                                    |
|--------------------|-------------------------------------------------------|--------------------------------------------------------|
| `size`             | `"1"\|"2"\|"3"\|"4"` - preset max-width (default `"3"`) | Not supported (no preset max-width tiers)              |
| `padding`          | Internal spacing                                      | `Box.Padding(Thickness)`                               |
| `margin`           | External spacing / centering                          | `Box.Margin(Thickness)`                                |
| `width`            | CSS width value                                       | `Box.Width(Size)`                                      |
| `height`           | CSS height value                                      | `Box.Height(Size)`                                     |
| `background_color` | Background color                                      | `Box.Color(Colors)`                                    |
