# Center

A layout component that centers its children both horizontally and vertically. In Reflex it is built on the `Flex` component with centering defaults applied. In Ivy, the closest equivalents are `StackLayout` with `Align.Center` or `Box` with `ContentAlign`.

## Reflex

```python
rx.center(
    rx.text("Hello World!"),
    width="50%",
    border_width="thick",
    border_radius="15px",
)
```

## Ivy

```csharp
// Using StackLayout with centered alignment
new StackLayout(
    [new Text("Hello World!")],
    align: Align.Center,
    padding: new Thickness(2)
);

// Or using Box which centers content by default
new Box("Hello World!")
    .ContentAlign(Align.Center)
    .BorderRadius(BorderRadius.Rounded)
    .BorderThickness(2);
```

## Parameters

| Parameter   | Documentation                                      | Ivy                                                      |
|-------------|-----------------------------------------------------|----------------------------------------------------------|
| `children`  | Child elements to render inside the center container | `children` array in `StackLayout` or content in `Box`    |
| `direction` | Flex direction (`"row"`, `"column"`, etc.)           | `Orientation.Horizontal` / `Orientation.Vertical`        |
| `align`     | Cross-axis alignment (`"start"`, `"center"`, etc.)   | `Align.Center` via `align` parameter                     |
| `justify`   | Main-axis justification (`"start"`, `"center"`, etc.)| Not supported (alignment is single-axis via `Align`)     |
| `wrap`      | Wrap behavior (`"nowrap"`, `"wrap"`, etc.)            | Not supported (use `WrapLayout` instead)                 |
| `spacing`   | Spacing between children (`"0"` - `"9"`)             | `gap` parameter (integer)                                |
| `as_child`  | Render as child element instead of wrapper           | Not supported                                            |
| `width`     | Width of the container                               | `.Width()` method on `Box`                               |
| `padding`   | Internal spacing                                     | `padding` parameter (`Thickness`)                        |
| `margin`    | External spacing                                     | `margin` parameter (`Thickness`)                         |
| `background`| Background color/style                               | `background` / `.Color()` parameter                      |
