# Grid

Component for creating two-dimensional grid layouts. Allows arranging child elements into rows and columns with control over spacing, alignment, and flow direction.

## Reflex

```python
rx.grid(
    rx.foreach(
        rx.Var.range(12),
        lambda i: rx.card(f"Card {i + 1}", height="10vh"),
    ),
    columns="3",
    spacing="4",
    width="100%",
)
```

## Ivy

```csharp
Layout.Grid()
    .Columns(2)
    .Rows(2)
    | Text.Block("Cell 1")
    | Text.Block("Cell 2")
    | Text.Block("Cell 3")
    | Text.Block("Cell 4")
```

## Parameters

| Parameter   | Documentation                                          | Ivy                                                                 |
|-------------|--------------------------------------------------------|---------------------------------------------------------------------|
| `columns`   | Number of columns in the grid                          | `Columns(int)`                                                      |
| `rows`      | Number of rows in the grid                             | `Rows(int)`                                                         |
| `flow`      | How items are placed: `"row"`, `"column"`, etc.        | `AutoFlow(AutoFlow.Row)` / `AutoFlow(AutoFlow.Column)` etc.         |
| `spacing`   | Gap between grid items (uniform)                       | `Gap(int)` (default: 4)                                             |
| `spacing_x` | Horizontal gap between grid items                      | Not supported                                                       |
| `spacing_y` | Vertical gap between grid items                        | Not supported                                                       |
| `align`     | Vertical alignment: `"start"`, `"center"`, etc.        | Not supported                                                       |
| `justify`   | Horizontal alignment: `"start"`, `"center"`, etc.      | Not supported                                                       |
| `as_child`  | Render as child element instead of wrapping             | Not supported                                                       |
| N/A         | Padding around the grid                                | `Padding(int)` (default: 0)                                         |
| N/A         | Position a child at a specific column                  | `.GridColumn(int)` extension on child                               |
| N/A         | Position a child at a specific row                     | `.GridRow(int)` extension on child                                  |
| N/A         | Span a child across multiple columns                   | `.GridColumnSpan(int)` extension on child                           |
| N/A         | Span a child across multiple rows                      | `.GridRowSpan(int)` extension on child                              |
