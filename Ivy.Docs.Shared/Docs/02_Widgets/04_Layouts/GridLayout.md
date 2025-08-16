---
prepare: |
  var client = this.UseService<IClientProvider>();
---

# GridLayout

<Ingress>
Create responsive two-dimensional grid layouts with precise control over positioning, spacing, and spanning for complex UI arrangements.
</Ingress>

The `GridLayout` widget arranges child elements in a two-dimensional grid system with precise control over positioning, spacing, and spanning. It provides both automatic flow and explicit positioning for flexible grid layouts.

## Basic Usage

Here's a simple 2x2 grid layout:

```csharp
Layout.Grid()
    .Columns(2)
    .Rows(2)
    | Text.Block("Cell 1")
    | Text.Block("Cell 2")
    | Text.Block("Cell 3")
    | Text.Block("Cell 4")
```

```csharp demo
Layout.Grid()
    .Columns(2)
    .Rows(2)
    | new Card("Cell 1")
    | new Card("Cell 2")
    | new Card("Cell 3")
    | new Card("Cell 4")
```

## Grid Definition Properties

### Columns and Rows

Define the number of columns and rows in your grid:

```csharp demo
Layout.Grid()
    .Columns(3)
    .Rows(2)
    | new Card("1")
    | new Card("2")
    | new Card("3")
    | new Card("4")
    | new Card("5")
    | new Card("6")
```

### Gap and Padding

Control spacing between grid items and around the grid:

```csharp demo-tabs
Layout.Vertical()
    | Text.Block("With Gap")
    | Layout.Grid()
        .Columns(3)
        .Gap(2)
        | new Card("A")
        | new Card("B")
        | new Card("C")
    | Text.Block("With Padding")
    | Layout.Grid()
        .Columns(3)
        .Padding(16)
        | new Card("A")
        | new Card("B")
        | new Card("C")
```

### Auto Flow

Control how items are automatically placed in the grid:

```csharp demo-tabs
Layout.Vertical()
    | Text.Block("Row (Default)")
    | Layout.Grid()
        .Columns(2)
        .AutoFlow(AutoFlow.Row)
        | new Card("1")
        | new Card("2")
        | new Card("3")
    | Text.Block("Column")
    | Layout.Grid()
        .Columns(2)
        .AutoFlow(AutoFlow.Column)
        | new Card("1")
        | new Card("2")
        | new Card("3")
```

## Child Positioning

### Grid Column and Row

Position children at specific grid coordinates:

```csharp demo
Layout.Grid()
    .Columns(3)
    .Rows(3)
    | new Card("Top-Left").GridColumn(1).GridRow(1)
    | new Card("Center").GridColumn(2).GridRow(2)
    | new Card("Bottom-Right").GridColumn(3).GridRow(3)
```

### Spanning Multiple Cells

Make children span across multiple columns or rows:

```csharp demo
Layout.Grid()
    .Columns(3)
    .Rows(3)
    | new Card("Header").GridColumn(1).GridRow(1).GridColumnSpan(3)
    | new Card("Sidebar").GridColumn(1).GridRow(2).GridRowSpan(2)
    | new Card("Main").GridColumn(2).GridRow(2).GridColumnSpan(2)
    | new Card("Footer").GridColumn(2).GridRow(3).GridColumnSpan(2)
```

## Construction Patterns

### Using Layout.Grid() (Recommended)

The fluent API provides a clean way to build grids:

```csharp
Layout.Grid()
    .Columns(2)
    .Gap(4)
    .Padding(8)
    | content1
    | content2
    | content3
```

### Using GridLayout Directly

For more control, you can use the GridLayout class directly:

```csharp
new GridLayout(
    new GridDefinition
    {
        Columns = 2,
        Rows = 2,
        Gap = 4,
        Padding = 8,
        AutoFlow = AutoFlow.Row
    },
    content1,
    content2,
    content3
)
```

## Advanced Examples

### Responsive Card Grid

```csharp demo
Layout.Grid()
    .Columns(3)
    .Gap(6)
    | new Card(
        new Button("Action 1", _ => client.Toast("Card 1 clicked"))
      ).GridColumnSpan(2)
    | new Card(
        new Button("Action 2", _ => client.Toast("Card 2 clicked"))
      )
    | new Card(
        new Button("Action 3", _ => client.Toast("Card 3 clicked"))
      )
    | new Card(
        new Button("Action 4", _ => client.Toast("Card 4 clicked"))
      ).GridColumnSpan(2)
```

### Dashboard Layout

```csharp demo
Layout.Grid()
    .Columns(4)
    .Rows(3)
    .Gap(4)
    | new Card("Header").GridColumn(1).GridRow(1).GridColumnSpan(4)
    | new Card("Nav").GridColumn(1).GridRow(2).GridRowSpan(2)
    | new Card("Main Content").GridColumn(2).GridRow(2).GridColumnSpan(2).GridRowSpan(2)
    | new Card("Sidebar").GridColumn(4).GridRow(2).GridRowSpan(2)
```

## AutoFlow Options

The `AutoFlow` enum provides different ways to automatically place grid items:

- **Row**: Fill each row before moving to the next (default)
- **Column**: Fill each column before moving to the next  
- **RowDense**: Fill rows, but try to fill gaps with later items
- **ColumnDense**: Fill columns, but try to fill gaps with later items

```csharp demo-tabs
Layout.Vertical()
    | Text.Block("Row Dense")
    | Layout.Grid()
        .Columns(3)
        .AutoFlow(AutoFlow.RowDense)
        | new Card("Wide Item").GridColumnSpan(2)
        | new Card("1")
        | new Card("2")
        | new Card("3")
    | Text.Block("Column Dense")
    | Layout.Grid()
        .Columns(3)
        .AutoFlow(AutoFlow.ColumnDense)
        | new Card("Tall Item").GridRowSpan(2)
        | new Card("1")
        | new Card("2")
        | new Card("3")
```

## Properties Reference

### GridDefinition Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| Columns | int? | null | Number of columns in the grid |
| Rows | int? | null | Number of rows in the grid |
| Gap | int | 4 | Space between grid items |
| Padding | int | 0 | Padding around the grid |
| AutoFlow | AutoFlow? | null | How items are automatically placed |
| Width | Size? | null | Grid container width |
| Height | Size? | null | Grid container height |

### Child Positioning Extensions

| Extension | Description |
|-----------|-------------|
| GridColumn(int) | Position child at specific column |
| GridRow(int) | Position child at specific row |
| GridColumnSpan(int) | Span child across multiple columns |
| GridRowSpan(int) | Span child across multiple rows |

<WidgetDocs Type="Ivy.GridLayout" ExtensionTypes="Ivy.GridExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Layouts/GridLayout.cs"/>
