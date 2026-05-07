# Spacer

Creates an adjustable, empty space that can be used to tune the spacing between child elements within a flex layout. Useful for pushing elements apart or creating consistent gaps in `flex`, `stack`, `vstack`, and `hstack` containers.

## Reflex

```python
rx.flex(
    rx.center(rx.text("Example"), bg="lightblue"),
    rx.spacer(),
    rx.center(rx.text("Example"), bg="lightgreen"),
    rx.spacer(),
    rx.center(rx.text("Example"), bg="salmon"),
    width="100%",
)
```

## Ivy

```csharp
new StackLayout(
    new Label("Example").Background(Colors.LightBlue),
    new Spacer().Width(Size.Grow()),
    new Label("Example").Background(Colors.LightGreen),
    new Spacer().Width(Size.Grow()),
    new Label("Example").Background(Colors.Salmon)
)
```

## Parameters

| Parameter   | Documentation                                                                 | Ivy                                                      |
|-------------|-------------------------------------------------------------------------------|----------------------------------------------------------|
| `as_child`  | Renders the component as its child element                                    | Not supported                                            |
| `direction` | Flex direction: `"row"`, `"column"`, `"row-reverse"`, `"column-reverse"`      | Not supported (direction is set on the parent layout)     |
| `align`     | Alignment of children: `"start"`, `"center"`, `"end"`, `"baseline"`, etc.     | Not supported (alignment is set on the parent layout)     |
| `justify`   | Justification of children: `"start"`, `"center"`, `"end"`, `"between"`, etc.  | Not supported (justification is set on the parent layout) |
| `wrap`      | Flex wrap: `"nowrap"`, `"wrap"`, `"wrap-reverse"`                             | Not supported                                            |
| `spacing`   | Gap between children: `"0"` through `"9"`                                     | `Height(Size)` / `Width(Size)` for fixed spacing         |
