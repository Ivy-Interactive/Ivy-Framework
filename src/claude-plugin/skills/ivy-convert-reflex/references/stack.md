# Stack

A layout component that arranges child elements sequentially in a vertical or horizontal direction with configurable spacing and alignment. Reflex provides three variants (`stack`, `hstack`, `vstack`) while Ivy uses a single `StackLayout` with an `orientation` parameter.

## Reflex

```python
import reflex as rx

# Vertical stack (default)
rx.vstack(
    rx.text("Item 1"),
    rx.text("Item 2"),
    rx.text("Item 3"),
    spacing="4",
    align="center",
)

# Horizontal stack
rx.hstack(
    rx.box("A", bg="red", padding="4"),
    rx.box("B", bg="blue", padding="4"),
    spacing="3",
    justify="center",
)

# Generic stack with explicit direction
rx.stack(
    rx.text("Row 1"),
    rx.text("Row 2"),
    direction="column",
    spacing="2",
)
```

## Ivy

```csharp
public class StackExample : ViewBase
{
    public override object? Build()
    {
        var box = new Box().Color(Colors.Primary).Width(2).Height(2);

        // Vertical stack (default)
        return new StackLayout([
            Text.H2("Stack"),
            Text.Label("Item 1"),
            Text.Label("Item 2"),
            Text.Label("Item 3"),
        ], gap: 4, align: Align.Center);
    }
}

public class HorizontalStackExample : ViewBase
{
    public override object? Build()
    {
        var box = new Box().Color(Colors.Primary).Width(2).Height(2);

        // Horizontal stack
        return new StackLayout([box, box],
            Orientation.Horizontal,
            gap: 3,
            margin: new Thickness(4));
    }
}
```

## Parameters

| Parameter   | Documentation                                                    | Ivy                                                        |
|-------------|------------------------------------------------------------------|------------------------------------------------------------|
| `spacing`   | Gap between child elements ("0"-"9")                             | `gap` (int, default `4`)                                   |
| `direction` | Flex direction: "row", "column", "row-reverse", "column-reverse" | `orientation` (Orientation.Vertical / Orientation.Horizontal) |
| `align`     | Cross-axis alignment: "start", "center", "end", etc.            | `align` (Align enum)                                       |
| `justify`   | Main-axis alignment: "start", "center", "end", "between"        | Not supported                                              |
| `wrap`      | Flex wrap: "nowrap", "wrap", "wrap-reverse"                      | Not supported (use WrapLayout)                             |
| `as_child`  | Render as child element (bool)                                   | Not supported                                              |
| `padding`   | Not a direct prop (use style)                                    | `padding` (Thickness)                                      |
| `margin`    | Not a direct prop (use style)                                    | `margin` (Thickness)                                       |
| `background`| Not a direct prop (use style / `bg`)                             | `background` (Colors)                                      |
| N/A         | N/A                                                              | `removeParentPadding` (bool, default `false`)              |
