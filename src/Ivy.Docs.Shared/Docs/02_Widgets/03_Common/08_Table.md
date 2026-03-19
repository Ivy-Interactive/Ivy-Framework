---
searchHints:
  - grid
  - data
  - rows
  - columns
  - cells
  - table
---

# Table

<Ingress>
Display structured data in a clean, organized format with powerful table [widgets](../../01_Onboarding/02_Concepts/03_Widgets.md) that support sorting, filtering, and custom formatting.
</Ingress>

The `Table` [widget](../../01_Onboarding/02_Concepts/03_Widgets.md) is a layout container designed to render data in a tabular format. It accepts rows composed of `TableRow` elements, making it suitable for structured display of content like data listings, reports, or grids.

## Basic Usage

There is a recommended way to create tables from data arrays.
The [ToTable()](../../01_Onboarding/02_Concepts/07_ContentBuilders.md) extension method automatically converts collections into formatted tables.

```csharp demo-tabs
public class BasicRowTable : ViewBase
{
    public class Product
    {
        public required string Sku { get; set; }
        public required string Name { get; set; }
        public required decimal Price { get; set; }
        public required string Url { get; set; }
    }

    public override object? Build()
    {
        var products = new[] {
            new {Sku = "1234", Name = "T-shirt", Price = 10, Url = "http://example.com/tshirt"},
            new {Sku = "1235", Name = "Jeans", Price = 20, Url = "http://example.com/jeans"},
            new {Sku = "1236", Name = "Sneakers", Price = 30, Url = "http://example.com/sneakers"},
        };

        return products.ToTable()
            .Width(Size.Full());
    }
}
```

### Custom Column Builders

**Width([Size](../../04_ApiReference/Ivy/Size.md).Full())** - sets the overall table width

**ColumnWidth(p => p.ColumnName, Size.Units())** – sets the column width with [Size](../../04_ApiReference/Ivy/Size.md)

**ColumnWidth(p => p.ColumnName, Size.Fraction())** – sets the column width as a fraction (percentage) of available space

Long text in cells automatically gets truncated with ellipsis (...) and shows full content in tooltips on hover

**Header(p => p.ColumnName)** is used to show custom header text of the table

**Align(p => p.ColumnName, [Align](../../04_ApiReference/Ivy/Align.md).Left|Center|Right)** - sets the alignment for both the header and data cells in the selected column. The alignment applies to the content within cells, not the entire column structure.

**Order(p => p.ColumnNameFirst, p.ColumnNameSecond, p.ColumnNameThird, ...)** - is used to order columns in a specific way

**Remove(p => p.ColumnName)** - makes possible not to show column in the table.

**Totals(p => p.ColumnName)** calculates the sum of the column if it contains numbers

**Empty(new [Card](04_Card.md)(""))** shows content when the table is empty.

```csharp demo-tabs
public class TableConfigurationExample : ViewBase
{
    public override object? Build()
    {
        var products = new[] {
            new {Sku = "1234", Name = "T-shirt", Price = 10, Url = "http://example.com/tshirt", _hiddenNotes = "archived"},
            new {Sku = "1235", Name = "Jeans", Price = 20, Url = "http://example.com/jeans", _hiddenNotes = "best-seller"}
        };

        return products.ToTable()
            .Width(Size.Full())
            .ColumnWidth(p => p.Price, Size.Units(100))
            .ColumnWidth(p => p.Sku, Size.Fraction(0.15f))
            .ColumnWidth(p => p.Name, Size.Fraction(0.3f))
            .ColumnWidth(p => p.Url, Size.Fraction(0.55f))
            .Header(p => p.Price, "Unit Price")
            .Header(p => p._hiddenNotes, "Internal Notes") // underscore + letter hidden automatically
            .Align(p => p.Price, Align.Right)
            .Order(p => p.Name, p => p.Price, p => p.Sku)
            .Remove(p => p.Url)
            .Totals(p => p.Price)
            .Empty(new Card("No products found").Width(Size.Full()));
    }
}
```

<Callout Type="tip">
Columns whose names start with an underscore followed by a letter (for example `_hidden`, `_internalId`) are automatically removed by default. Properties that use an underscore followed by a digit or symbol (such as `_1` or `_$special`) now stay visible unless you explicitly hide them.
</Callout>

### Column Management Examples

The `Clear()` method hides all columns, allowing you to selectively show only the columns you need.
Use `Add()` to show specific columns in the order you want them to appear.

```csharp demo-tabs
public class ColumnManagementTable : ViewBase
{
    public override object? Build()
    {
        var products = new[] {
            new {Sku = "1234", Name = "T-shirt", Price = 10, Category = "Clothing", Stock = 50, _hiddenInternal = "archived"},
            new {Sku = "1235", Name = "Jeans", Price = 20, Category = "Clothing", Stock = 30, _hiddenInternal = "featured"},
            new {Sku = "1236", Name = "Sneakers", Price = 30, Category = "Footwear", Stock = 25, _hiddenInternal = "featured"}
        };

        return products.ToTable()
            .Width(Size.Full())
            .Clear()                                    // Hide all columns first
            .Add(p => p.Name)                          // Show only Name column
            .Add(p => p.Price)                         // Add Price column
            .Add(p => p.Stock)                         // Add Stock column
            .Header(p => p._hiddenInternal, "Internal Flag") // hidden by default due to underscore
            .Header(p => p.Price, "Unit Price")
            .Align(p => p.Price, Align.Right)
            .Align(p => p.Stock, Align.Center);
    }
}
```

### Advanced Aggregations

The `Totals()` method supports custom aggregation functions.
You can use LINQ methods like `Count()`, `Average()`, `Sum()`, `Max()`, and `Min()` to create sophisticated calculations for your data.

```csharp demo-tabs
public class AdvancedAggregationsTable : ViewBase
{
    public override object? Build()
    {
        var products = new[] {
            new {Sku = "1234", Name = "T-shirt", Price = 10, Stock = 50},
            new {Sku = "1235", Name = "Jeans", Price = 20, Stock = 30},
            new {Sku = "1236", Name = "Sneakers", Price = 30, Stock = 25}
        };

        return products.ToTable()
            .Width(Size.Full())
            .Header(p => p.Price, "Unit Price")
            .Header(p => p.Stock, "In Stock")
            .Align(p => p.Price, Align.Right)
            .Align(p => p.Stock, Align.Center)
            .Totals(p => p.Price)                      // Sum of prices
            .Totals(p => p.Stock, items => items.Count()) // Count of items
            .Totals(p => p.Price, items => items.Average(p => p.Price)) // Average price
            .Totals(p => p.Stock, items => items.Sum(p => p.Stock)); // Total stock
    }
}
```

### Pivot Tables

Pivot tables allow you to aggregate and summarize data by grouping on dimensions and calculating measures. Use the `ToPivotTable()` extension method to transform data into aggregated results that can be displayed as tables.

**Dimension** - A field to group by (e.g., Category, Region, Date)

**Measure** - An aggregated calculation (e.g., Sum, Count, Average, Max, Min)

```csharp demo-tabs
public class PivotTableExample : ViewBase
{
    record SalesData(string Browser, string Region, int Sessions, decimal Revenue);
    record BrowserSummary(string Browser, int TotalSessions, decimal TotalRevenue, decimal AverageRevenue);

    public override async Task<object?> Build()
    {
        var rawData = new[]
        {
            new SalesData("Chrome", "North", 150, 4500m),
            new SalesData("Chrome", "South", 120, 3600m),
            new SalesData("Firefox", "North", 80, 2400m),
            new SalesData("Firefox", "South", 60, 1800m),
            new SalesData("Safari", "North", 50, 1500m),
            new SalesData("Safari", "South", 40, 1200m)
        };

        var pivotByBrowser = await rawData.ToPivotTable()
            .Dimension("Browser", d => d.Browser)
            .Measure("Total Sessions", g => g.Sum(s => s.Sessions))
            .Measure("Total Revenue", g => g.Sum(s => s.Revenue))
            .Measure("Average Revenue", g => g.Average(s => s.Revenue))
            .ExecuteAsync();

        var pivotByBrowserAndRegion = await rawData.ToPivotTable()
            .Dimension("Browser", d => d.Browser)
            .Dimension("Region", d => d.Region)
            .Measure("Sessions", g => g.Sum(s => s.Sessions))
            .Measure("Revenue", g => g.Sum(s => s.Revenue))
            .ExecuteAsync();

        var typedResults = new List<BrowserSummary>();
        await foreach (var item in rawData.ToPivotTable()
            .Dimension("Browser", d => d.Browser)
            .Measure("TotalSessions", g => g.Sum(s => s.Sessions))
            .Measure("TotalRevenue", g => g.Sum(s => s.Revenue))
            .Measure("AverageRevenue", g => g.Average(s => s.Revenue))
            .Produces<BrowserSummary>()
            .ExecuteAsync())
        {
            typedResults.Add(item);
        }

        return Layout.Vertical().Gap(4)
            | Text.H3("Raw Data")
            | rawData.ToTable().Width(Size.Full())
            | pivotByBrowser.ToExpando().ToTable().Width(Size.Full())
            | pivotByBrowserAndRegion.ToExpando().ToTable().Width(Size.Full())
            | Text.H3("Pivot Table - Strongly Typed Result")
            | typedResults.ToTable()
                .Width(Size.Full())
                .Header(r => r.Browser, "Browser")
                .Header(r => r.TotalSessions, "Total Sessions")
                .Header(r => r.TotalRevenue, "Revenue")
                .Header(r => r.AverageRevenue, "Avg Revenue")
                .Align(r => r.TotalSessions, Align.Right)
                .Align(r => r.TotalRevenue, Align.Right)
                .Align(r => r.AverageRevenue, Align.Right);
    }
}
```

### Empty Columns Handling

The `RemoveEmptyColumns()` method automatically hides columns that contain no data (empty strings, null values, or zero values). This is useful for dynamic data where some columns might be empty across all rows.

```csharp demo-tabs
public class EmptyColumnsTable : ViewBase
{
    public override object? Build()
    {
        var products = new[] {
            new {Sku = "1234", Name = "T-shirt", Price = 10, Category = "Clothing", Notes = ""},
            new {Sku = "1235", Name = "Jeans", Price = 20, Category = "Clothing", Notes = ""},
            new {Sku = "1236", Name = "Sneakers", Price = 30, Category = "Footwear", Notes = ""}
        };

        return products.ToTable()
            .Width(Size.Full())
            .RemoveEmptyColumns()                      // Automatically hides "Notes" because it's empty in all rows
            .Header(p => p.Price, "Unit Price")
            .Align(p => p.Price, Align.Right);
    }
}
```

### Reset and Rebuild

The `Reset()` method restores all column settings to their default values. This is useful when you want to start fresh with a new configuration or when building dynamic table configurations.

```csharp demo-tabs
public class ResetTableExample : ViewBase
{
    public override object? Build()
    {
        var products = new[] {
            new {Sku = "1234", Name = "T-shirt", Price = 10, Category = "Clothing", _hiddenMetadata = "legacy"},
            new {Sku = "1235", Name = "Jeans", Price = 20, Category = "Clothing", _hiddenMetadata = "seasonal"}
        };

        return products.ToTable()
            .Width(Size.Full())
            .Remove(p => p.Category)                   // Hide Category column
            .Align(p => p.Price, Align.Right)          // Set alignment
            .Header(p => p.Price, "Unit Price")        // Custom header
            .Header(p => p._hiddenMetadata, "Metadata") // underscore + letter hidden automatically
            .Reset()                                   // Reset all settings to defaults
            .Order(p => p.Name, p => p.Price)          // Apply new order
            .Totals(p => p.Price);                     // Add totals
    }
}
```

### Manual Table

It's also possible to create manual tables with headers and other methods using rows and cells:

```csharp demo-tabs
public class ManualTableDemo : ViewBase
{
    public override object? Build()
    {
        return new Table(
            new TableRow(
                new TableCell("Name").IsHeader().Align(Align.Left),
                new TableCell("Age").IsHeader().Align(Align.Center),
                new TableCell("Email").IsHeader().Align(Align.Left)
            ),
            new TableRow(
                new TableCell("Alice"),
                new TableCell("30").Align(Align.Center),
                new TableCell("alice@example.com")
            )
        ).Width(Size.Full());
    }
}
```

### Builder Factory Methods

The `Builder()` method allows you to specify how different data types should be rendered. Use the builder factory methods to create appropriate renderers for your data.

```csharp demo-tabs
public class CellBuildersExample : ViewBase
{
    public override object? Build()
    {
        var products = new[] {
            new {Sku = "1234", Name = "T-shirt", Price = 10, Url = "http://example.com/tshirt", Description = "High quality cotton T-shirt with a comfortable fit and durable construction. Perfect for everyday wear and available in multiple colors."},
            new {Sku = "1235", Name = "Jeans", Price = 20, Url = "http://example.com/jeans", Description = "Classic denim jeans with a modern cut and premium stitching. Features include reinforced pockets, comfortable waistband, and fade-resistant fabric."}
        };

        return products.ToTable()
            .Width(Size.Full())
            .ColumnWidth(p => p.Sku, Size.Fraction(0.15f))          // 15% for SKU
            .ColumnWidth(p => p.Name, Size.Fraction(0.25f))         // 25% for Name
            .ColumnWidth(p => p.Price, Size.Fraction(0.15f))        // 15% for Price
            .ColumnWidth(p => p.Url, Size.Fraction(0.2f))           // 20% for URL
            .ColumnWidth(p => p.Description, Size.Fraction(0.25f))  // 25% for Description
            .Multiline(p => p.Description)                    // Enable multiline for the Description column
            .Builder(p => p.Url, f => f.Link())               // Link builder
            .Builder(p => p.Description, f => f.Text())       // Text builder
            .Builder(p => p.Sku, f => f.CopyToClipboard())    // Copy to clipboard
            .Builder(p => p.Name, f => f.Default())           // Default builder
            .Header(p => p.Price, "Unit Price")
            .Align(p => p.Price, Align.Right);
    }
}
```

### Progress Builder

The `Progress()` builder renders numeric values as inline [progress](10_Progress.md) bars within table cells.

```csharp demo-tabs
public class ProgressBuilderExample : ViewBase
{
    public override object? Build()
    {
        var tasks = new[] {
            new {Name = "Design Review", Progress = 100},
            new {Name = "Implementation", Progress = 75},
            new {Name = "Testing", Progress = 45},
            new {Name = "Documentation", Progress = 20}
        };

        return tasks.ToTable()
            .Width(Size.Full())
            .Builder(t => t.Progress, f => f.Progress().AutoColor().Format("%d%"))
            .ColumnWidth(t => t.Name, Size.Fraction(0.5f))
            .ColumnWidth(t => t.Progress, Size.Fraction(0.5f));
    }
}
```

### Automatic Table Conversion

Any `IEnumerable` is automatically converted to a table when returned from a view. This works through the [DefaultContentBuilder](../../01_Onboarding/02_Concepts/12_ContentBuilders.md) which detects collections and converts them to tables.

```csharp demo-tabs
public class AutomaticTableConversion : ViewBase
{
    public override object? Build()
    {
        // Any IEnumerable is automatically converted to a table
        object data = GetProductData();
        return data; // Automatically becomes a table via DefaultContentBuilder
    }

    private object GetProductData()
    {
        return new[] {
            new {Sku = "1234", Name = "T-shirt", Price = 10},
            new {Sku = "1235", Name = "Jeans", Price = 20}
        };
    }
}
```

### Integration with Other Widgets

Tables integrate seamlessly with other Ivy widgets, allowing you to create rich, interactive [interfaces](../../01_Onboarding/02_Concepts/02_Views.md).

```csharp demo-tabs
public class TableIntegrationExample : ViewBase
{
    record Product(string Sku, string Name, double Price);
    public override object? Build()
    {

        var products = UseState<Product[]>(
            [new Product("1234", "T-shirt", 10.0), new Product("1235", "Jeans", 20.0)]
            );

        var client = UseService<IClientProvider>();

        var addProduct = (Event<Button> e) =>
        {
            var currentCount = products.Value.Length;

            var newProduct = new Product(
                Sku: $"SKU{1000 + currentCount}",
                Name: $"Product {currentCount + 1}",
                Price: 15.0 + currentCount
            );

            var updatedProducts = products.Value.Append(newProduct).ToArray();
            products.Set(updatedProducts);

            client.Toast($"Added {newProduct.Name}", "Product Added");
        };

        var clearProducts = (Event<Button> e) =>
        {
            products.Set(new Product[0]);
            client.Toast("All products cleared", "Products Cleared");
        };

        return Layout.Vertical()
            | new Card(
                Layout.Vertical()
                    | products.Value.ToTable().Width(Size.Full())
                    | (Layout.Horizontal().Gap(2)
                        | new Button("Add Product", addProduct).Variant(ButtonVariant.Secondary)
                        | new Button("Clear All", clearProducts).Variant(ButtonVariant.Destructive))
                    | Text.Block($"Total Products: {products.Value.Length}")
            ).Title("Product List").Width(Size.Full());
    }
}
```

## Faq

<Details>
<Summary>
How do I create a simple Table with headers and rows in Ivy?
</Summary>
<Body>

For simple tabular display, use `.ToTable()` on a collection of records:

```csharp
var data = new[]
{
    new { Name = "Alice", Age = 30, City = "NYC" },
    new { Name = "Bob", Age = 25, City = "LA" },
};
return data.ToTable();
```

For manual Table construction with `TableRow` and `TableCell`:

```csharp
new Table(
    new TableRow(new TableCell("Name"), new TableCell("Age")).IsHeader(),
    new TableRow(new TableCell("Alice"), new TableCell("30")),
    new TableRow(new TableCell("Bob"), new TableCell("25"))
)
```

**Important:** `Table` takes `TableRow[]`, NOT `string[]`. There is no `.Row()` method. For data-heavy tables with sorting, filtering, and pagination, use `.ToDataTable()` on `IQueryable<T>` instead.

</Body>
</Details>

<Details>
<Summary>
When should I use DataTable instead of Table?
</Summary>
<Body>

`Table` is for simple, static layouts. For **interactive data grids** with sorting, filtering, search, and large datasets, use [DataTable](../07_Advanced/01_DataTable.md) instead.

**Use DataTable when you need:**
- Sorting and filtering
- Search functionality
- Pagination or incremental loading
- Row actions (edit, delete)
- Large datasets (100+ rows)
- Performance with IQueryable (Apache Arrow backend)

```csharp
// Use DataTable for interactive data:
employees.AsQueryable().ToDataTable()
    .Header(e => e.Name, "Employee")
    .Config(c => {
        c.AllowSorting = true;
        c.ShowSearch = true;
    })

// Use Table for simple layouts:
new[] {
    new { Label = "Status", Value = "Active" },
    new { Label = "Count", Value = "42" }
}.ToTable()
```

See [DataTable](../07_Advanced/01_DataTable.md) for full grid capabilities.

</Body>
</Details>

<WidgetDocs Type="Ivy.Table" ExtensionTypes="Ivy.TableExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/src/Ivy/Widgets/Tables/Table.cs"/>
