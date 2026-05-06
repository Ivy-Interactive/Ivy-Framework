# Flex

The Flex component creates flexbox layouts, arranging child components in horizontal or vertical directions. It supports wrapping, content justification and alignment, spacing, and automatic sizing based on available space — ideal for responsive layouts. By default, children arrange horizontally without wrapping.

In Ivy the closest equivalent is `StackLayout` for linear (row/column) arrangements, with `WrapLayout` covering the wrapping use-case separately. Ivy also offers shorthand factory methods `Layout.Horizontal()` and `Layout.Vertical()`.

## Reflex

```python
rx.flex(
    rx.box("Card 1"),
    rx.box("Card 2"),
    rx.box("Card 3"),
    direction="row",
    spacing="4",
    align="center",
    justify="between",
    wrap="wrap",
)
```

## Ivy

```csharp
// Horizontal stack (equivalent to direction="row")
new StackLayout([
    new Text("Card 1"),
    new Text("Card 2"),
    new Text("Card 3"),
], orientation: Orientation.Horizontal, gap: 4).Center()

// Or using the Layout factory shorthand
Layout.Horizontal([
    new Text("Card 1"),
    new Text("Card 2"),
    new Text("Card 3"),
])

// For wrapping behavior, use WrapLayout instead
new WrapLayout([
    new Text("Card 1"),
    new Text("Card 2"),
    new Text("Card 3"),
], gap: 4)
```

## Parameters

| Parameter      | Documentation                                       | Ivy                                                                 |
|----------------|------------------------------------------------------|----------------------------------------------------------------------|
| `direction`    | `"row"` or `"column"` — axis for children            | `orientation: Orientation.Horizontal` / `Orientation.Vertical`       |
| `spacing`      | Gap between child elements (`"0"`–`"9"`)             | `gap: int` (default 4)                                               |
| `align`        | Cross-axis alignment (`"start"`, `"center"`, etc.)   | `.Center()`, `.Left()`, `.Right()` fluent methods / `align: Align?`  |
| `justify`      | Main-axis alignment (`"start"`, `"between"`, etc.)   | Not supported                                                        |
| `wrap`         | `"nowrap"`, `"wrap"`, `"wrap-reverse"`               | Not supported (use `WrapLayout` for wrapping)                        |
| `flex_grow`    | Growth factor along main axis                        | Not supported                                                        |
| `flex_shrink`  | Shrink factor along main axis                        | Not supported                                                        |
| `flex_basis`   | Optimal size along main axis before grow/shrink      | Not supported                                                        |
| `as_child`     | Render as child element instead of wrapping in a div | Not supported                                                        |
| `padding`      | Internal spacing (via style props)                   | `padding: Thickness?`                                                |
| `margin`       | External spacing (via style props)                   | `margin: Thickness?`                                                 |
| `background`   | Background color (via style props)                   | `background: Colors?`                                                |
| `width`        | Width of the component (via style props)             | `.Width(Size)` fluent method                                         |
| `height`       | Height of the component (via style props)            | `.Height(Size)` fluent method                                        |
| `visible`      | Visibility toggle (via conditional rendering)        | `Visible: bool`                                                      |
