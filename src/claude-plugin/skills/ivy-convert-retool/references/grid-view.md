# Grid View

A repeatable grid of components with values that map to a list of data. Grid View is a preset version of List View, preconfigured to use a grid layout. It maps an array of data to repeating instances, where each instance renders a template of child components.

## Retool

```toolscript
// Grid View is a List View preset with grid layout.
// Access the current item in each instance with `item` and index with `i`.

// Data source (array of objects)
gridView1.data = {{ query1.data }}

// Reference item properties inside each grid cell
{{ item.name }}
{{ item.price }}
{{ item.imageUrl }}

// Configuration
gridView1.numColumns = 3
gridView1.direction = "row"
gridView1.overflowType = "pagination"  // or "scroll"
gridView1.heightType = "fixed"         // "fixed", "auto", or "fill"
gridView1.itemWidth = 200

// Instance values aggregation (collect form inputs across instances)
gridView1.enableInstanceValues = true
gridView1.instanceValues  // read-only array of { formDataKey: value }

// Methods
gridView1.scrollToIndex(5)
gridView1.scrollIntoView({ behavior: "auto", block: "nearest" })
gridView1.resetInstanceValues()
gridView1.clearInstanceValues()
gridView1.setHidden(true)
```

## Ivy

Ivy does not have a direct Grid View repeater widget. The equivalent pattern uses LINQ's `Select` to map data into widgets, combined with a layout widget (`WrapLayout` for flowing grids or `GridLayout` for explicit grids).

```csharp
public class ProductGridDemo : ViewBase
{
    public override object? Build()
    {
        var products = new[]
        {
            new { Name = "Laptop", Price = 999.99m, ImageUrl = "laptop.png" },
            new { Name = "Mouse", Price = 29.99m, ImageUrl = "mouse.png" },
            new { Name = "Keyboard", Price = 79.99m, ImageUrl = "keyboard.png" },
            new { Name = "Monitor", Price = 499.99m, ImageUrl = "monitor.png" },
        };

        // WrapLayout for a responsive flowing grid (closest to Grid View)
        var cards = products.Select(product =>
            new Card(
                content: Layout.Vertical().Gap(2)
                    | Text.P(product.Name).Large()
                    | Text.P($"${product.Price}")
            ).Width(200)
        );

        return new WrapLayout(cards.ToArray(), gap: 8);
    }
}

// Alternative: Using GridLayout for explicit column control
public class ProductGridLayoutDemo : ViewBase
{
    public override object? Build()
    {
        var products = new[]
        {
            new { Name = "Laptop", Price = 999.99m },
            new { Name = "Mouse", Price = 29.99m },
            new { Name = "Keyboard", Price = 79.99m },
        };

        var cards = products.Select(product =>
            new Card(
                content: Layout.Vertical().Gap(2)
                    | Text.P(product.Name).Large()
                    | Text.P($"${product.Price}")
            )
        );

        return Layout.Grid().Columns(3).Gap(8)
            | cards.ToArray();
    }
}

// Using List for a vertical list of items (like List View)
public class ProductListDemo : ViewBase
{
    public override object? Build()
    {
        var products = new[]
        {
            new { Name = "Laptop", Price = 999.99m, Stock = 15 },
            new { Name = "Mouse", Price = 29.99m, Stock = 50 },
        };

        var items = products.Select(product => new ListItem(
            title: product.Name,
            subtitle: $"${product.Price} - {product.Stock} in stock"
        ));

        return new List(items);
    }
}
```

## Parameters

| Parameter | Documentation | Ivy |
|---|---|---|
| `data` | Array of data mapped to repeating instances. Each instance accesses its entry via `item` and index via `i`. | Use LINQ `.Select()` to map data to widgets. Data is passed directly as C# objects. |
| `numColumns` | Number of columns in the grid layout. | `Layout.Grid().Columns(n)` or use `WrapLayout` for automatic wrapping based on item width. |
| `direction` | Layout orientation: `row` or `column`. | `GridLayout.AutoFlow` (`Row`, `Column`, `RowDense`, `ColumnDense`). |
| `itemWidth` | Width of individual grid items. | `.Width(Size)` on each child widget (e.g., `Card().Width(200)`). |
| `overflowType` | Overflow behavior: `scroll` or `pagination`. | Not supported |
| `heightType` | Height mode: `fixed`, `auto`, or `fill`. | `.Height(Size)` on the layout widget. |
| `hidden` | Toggle visibility of the component. | `.Visible` property on layout widgets. |
| `isHiddenOnMobile` | Hide component on mobile devices. | Not supported |
| `isHiddenOnDesktop` | Hide component on desktop. | Not supported |
| `enableInstanceValues` | Aggregate form input values across all instances as an array. | Not supported (use `UseState` to manage collected form data manually). |
| `instanceValues` | Read-only array of aggregated `{ formDataKey: value }` from instances. | Not supported |
| `primaryKeyFieldNameOverride` | Custom primary key for correct state during data updates. | Not needed (C# object identity / LINQ handles this). |
| `margin` | Outer spacing around the component. | `.Margin(Thickness)` on layout widgets. |
| `padding` | Inner spacing within the component. | `.Padding(Thickness)` on layout widgets. |
| `style` | Custom CSS styling. | Not supported (use Ivy theming system instead). |
| `formDataKey` | Key used to construct form data from instance values. | Not supported |
| `scrollToIndex()` | Method to scroll to a specific item index. | Not supported |
| `scrollIntoView()` | Method to scroll the component into the visible area. | Not supported |
| `resetInstanceValues()` | Restore instance values to initial state. | Not supported |
| `clearInstanceValues()` | Remove all instance values. | Not supported |
| `setHidden()` | Programmatically toggle visibility. | Not supported |
| Nested repeatables | Supports up to 3 levels of nested repeatable components. | Supported via nested LINQ `.Select()` calls within layout widgets. |
