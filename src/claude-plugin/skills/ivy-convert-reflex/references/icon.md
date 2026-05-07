# Icon

Displays a vector icon from the Lucide icon library. Both Reflex and Ivy use the same underlying Lucide icon set (1000+ icons).

## Reflex

```python
import reflex as rx

rx.icon("heart")
rx.icon("search", size=24, color="indigo", stroke_width=2)
```

## Ivy

```csharp
new Icon(Icons.Heart)
new Icon(Icons.Search, Colors.Indigo).Width(24).Height(24)
```

## Parameters

| Parameter      | Documentation                                                      | Ivy                                                     |
|----------------|--------------------------------------------------------------------|---------------------------------------------------------|
| `tag`          | Icon name from Lucide (snake_case or kebab-case)                   | `Icons` enum, e.g. `Icons.Heart`                        |
| `size`         | Adjusts icon dimensions                                            | `.Width()` / `.Height()` using `Size`, or `.Small()` / `.Large()` |
| `color`        | Icon color (name, hex, or Radix color scale via `rx.color()`)      | `Colors` enum, e.g. `Colors.Red`                        |
| `stroke_width` | Controls line thickness (1, 1.5, 2, 2.5)                          | Not supported                                           |
| N/A            | N/A                                                                | `.Scale()` adjusts icon scale                           |
| N/A            | N/A                                                                | `.WithAnimation()` supports rotate and other animations |
| N/A            | N/A                                                                | `.Visible()` toggles visibility                         |
