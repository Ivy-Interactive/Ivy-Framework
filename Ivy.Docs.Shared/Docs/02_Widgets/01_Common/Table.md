# Table

<Ingress>
Display structured data in a clean, organized format with powerful table widgets that support sorting, filtering, and custom formatting.
</Ingress>

The `Table` widget is a layout container designed to render data in a tabular format. It accepts rows composed of `TableRow` elements, making it suitable for structured display of content like data listings, reports, or grids.

## Basic Usage

There is a recommended way to create tables from data arrays.
The `ToTable()` extension method automatically converts collections into formatted tables.

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

**Width(Size.Full())** - sets the overall table width

**Width(p => p.ColumnName, Size.Units())** – sets the column width

**Header(p => p.ColumnName)** is used to show custom header text of the table

**Align(p => p.ColumnName)** - right alignment for selected column

**Order(p => p.ColumnNameFirst, p.ColumnNameSecond, p.ColumnNameThird, ...)** - is used to order columns in a specific way

**Remove(p => p.ColumnName)** - makes possible not to show column in the table

**Totals(p => p.ColumnName)** calculates the sum of the column if it contains numbers

**Empty(new Card(""))** shows content when the table is empty.

```csharp demo-tabs 
public class CustomBuilderTable : ViewBase
{
    public override object? Build()
    {
        var products = new[] {
            new {Sku = "1234", Name = "T-shirt", Price = 10, Url = "http://example.com/tshirt"},
            new {Sku = "1235", Name = "Jeans", Price = 20, Url = "http://example.com/jeans"}
        };

        return products.ToTable()
            .Width(Size.Full())
            .Width(p => p.Price, Size.Units(100))
            .Header(p => p.Price, "Unit Price")
            .Align(p => p.Price, Align.Right)
            .Order(p => p.Name, p => p.Price, p => p.Sku)
            .Remove(p => p.Url)
            .Totals(p => p.Price)
            .Empty(new Card("No products found").Width(Size.Full()));
    }
}
```

### Column Management Examples

The `Clear()` method hides all columns, allowing you to selectively show only the columns you need.
Use `Add()` to show specific columns in the order you want them to appear.

```csharp demo-tabs 
public class ColumnManagementTable : ViewBase
{
    public override object? Build()
    {
        var products = new[] {
            new {Sku = "1234", Name = "T-shirt", Price = 10, Category = "Clothing", Stock = 50},
            new {Sku = "1235", Name = "Jeans", Price = 20, Category = "Clothing", Stock = 30},
            new {Sku = "1236", Name = "Sneakers", Price = 30, Category = "Footwear", Stock = 25}
        };

        return products.ToTable()
            .Width(Size.Full())
            .Clear()                                    // Hide all columns first
            .Add(p => p.Name)                          // Show only Name column
            .Add(p => p.Price)                         // Add Price column
            .Add(p => p.Stock)                         // Add Stock column
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

### Empty Columns Handling

The `RemoveEmptyColumns()` method automatically hides columns that contain no data (empty strings, null values, or zero values). This is useful for dynamic data where some columns might be empty across all rows.

```csharp demo-tabs 
public class EmptyColumnsTable : ViewBase
{
    public override object? Build()
    {
        var products = new[] {
            new {Sku = "1234", Name = "T-shirt", Price = 10, Description = "", Notes = ""},
            new {Sku = "1235", Name = "Jeans", Price = 20, Description = "Blue jeans", Notes = ""},
            new {Sku = "1236", Name = "Sneakers", Price = 30, Description = "", Notes = "Limited edition"}
        };

        return products.ToTable()
            .Width(Size.Full())
            .RemoveEmptyColumns()                      // Hide columns with no data
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
            new {Sku = "1234", Name = "T-shirt", Price = 10, Category = "Clothing"},
            new {Sku = "1235", Name = "Jeans", Price = 20, Category = "Clothing"}
        };

        return products.ToTable()
            .Width(Size.Full())
            .Remove(p => p.Category)                   // Hide Category column
            .Align(p => p.Price, Align.Right)          // Set alignment
            .Header(p => p.Price, "Unit Price")        // Custom header
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
        );
    }
}
```

### Builder Factory Methods

The `Builder()` method allows you to specify how different data types should be rendered. Use the builder factory methods to create appropriate renderers for your data.

```csharp demo-tabs
public class CustomBuildersTable : ViewBase
{
    public override object? Build()
    {
        var products = new[] {
            new {Sku = "1234", Name = "T-shirt", Price = 10, Url = "http://example.com/tshirt", Description = "Comfortable cotton shirt"},
            new {Sku = "1235", Name = "Jeans", Price = 20, Url = "http://example.com/jeans", Description = "Blue denim jeans"}
        };

        return products.ToTable()
            .Width(Size.Full())
            .Builder(p => p.Url, f => f.Link())               // Link builder
            .Builder(p => p.Description, f => f.Text())       // Text builder
            .Builder(p => p.Sku, f => f.CopyToClipboard())    // Copy to clipboard
            .Builder(p => p.Name, f => f.Default())           // Default builder
            .Header(p => p.Price, "Unit Price")
            .Align(p => p.Price, Align.Right);
    }
}
```

### Automatic Table Conversion

Any `IEnumerable` is automatically converted to a table when returned from a view. This works through the `DefaultContentBuilder` which detects collections and converts them to tables.

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

Tables integrate seamlessly with other Ivy widgets, allowing you to create rich, interactive interfaces.

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
                    | Layout.Horizontal().Gap(2)
                        | new Button("Add Product", addProduct).Variant(ButtonVariant.Secondary)
                        | new Button("Clear All", clearProducts).Variant(ButtonVariant.Destructive)
                        | Text.Block($"Total Products: {products.Value.Length}")
            ).Title("Product List").Width(Size.Full());
    }
}
```

### Missing Context and Examples

- `Empty(object)` - Sets content to display when the table has no data

  ```csharp
  .Empty(new Card("No products found").Width(Size.Full()))
  ```

<WidgetDocs Type="Ivy.Table" ExtensionTypes="Ivy.Views.Tables.TableExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Tables/Table.cs"/>
