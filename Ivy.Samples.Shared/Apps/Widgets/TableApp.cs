using System.Reflection.Emit;
using Ivy.Shared;
using Ivy.Views.Builders;
using Ivy.Views.Tables;

namespace Ivy.Samples.Shared.Apps.Widgets;

public class Product
{
    public required string Sku { get; set; }
    public required bool Foo { get; set; } = true; // Example of a boolean property
    public required string Name { get; set; }
    public required double Price { get; set; }
    public required string Url { get; set; }
}

public class Record
{
    public required string Reference { get; set; }
    public required string Contact { get; set; }
    public required int Views { get; set; }
}

[App(icon: Icons.Table, path: ["Widgets"], searchHints: ["grid", "data", "rows", "columns", "cells", "spreadsheet"])]
public class TableApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Tabs(
            new Tab("Table Sizes", new TableSizesExample()),
            new Tab("Column Widths", new ColumnWidthsExample()),
            new Tab("Alignment", new AlignmentExample())
        ).Variant(TabsVariant.Content);
    }
}

public class TableSizesExample : ViewBase
{
    public override object? Build()
    {
        //Anonymous type array

        var products = new[] {
            new {Sku = "1234", Foo = true, Name = "T-shirt", Price = 10.0, Url = "http://example.com/tshirt"},
            new {Sku = "1235", Foo = true, Name = "Jeans", Price = 20.0, Url = "http://example.com/jeans"},
            new {Sku = "1236", Foo = true, Name = "Sneakers", Price = 30.0, Url = "http://example.com/sneakers"},
            new {Sku = "1237", Foo = true, Name = "Hat", Price = 5.0, Url = "http://example.com/hat"},
            new {Sku = "1238", Foo = true, Name = "Premium Luxury Extra-Soft Organic Cotton Socks with Reinforced Heel and Toe - Perfect for All-Day Comfort and Athletic Performance - Available in Multiple Colors", Price = 2.0, Url = "http://example.com/socks"}
        };

        // Table with long headers to test overflow and tooltips
        var longHeaderTable = new Table(
            new TableRow(
                new TableCell("Very Long Column Name That Should Cause Overflow And Show Tooltips").IsHeader(),
                new TableCell("Another Extremely Long Column Header Name For Testing Truncation Purposes").IsHeader(),
                new TableCell("Super Long Descriptive Column Name That Explains Everything In Great Detail").IsHeader(),
                new TableCell("Ultra Wide Column Header With Lots Of Words And Characters To Test Overflow").IsHeader()
            )
            { IsHeader = true },
            new TableRow(
                new TableCell("Short Data"),
                new TableCell("Medium length data value"),
                new TableCell("This is a very long data value that should also get truncated and show a tooltip when hovered"),
                new TableCell("Result A")
            ),
            new TableRow(
                new TableCell("Data 2"),
                new TableCell("Value B"),
                new TableCell("Another long piece of data that exceeds the normal cell width and should be truncated"),
                new TableCell("Result B")
            ),
            new TableRow(
                new TableCell("Data 3"),
                new TableCell("Value C"),
                new TableCell("Short"),
                new TableCell("Result C")
            )
        );


        return Layout.Vertical(
            Text.H3("Table Sizes"),
            Text.Label("Small Size:"),
            products
                .ToTable().Small()
                .Builder(e => e.Url, e => e.Link())
                .Width(Size.Full())
                .MultiLine(e => e.Name)
                // Add explicit column widths to test overflow
                .ColumnWidth(e => e.Sku, Size.Fraction(0.15f))      // 15% for SKU
                .ColumnWidth(e => e.Foo, Size.Fraction(0.1f))       // 10% for Foo  
                .ColumnWidth(e => e.Name, Size.Fraction(0.3f))      // 30% for Name
                .ColumnWidth(e => e.Price, Size.Fraction(0.15f))    // 15% for Price
                .ColumnWidth(e => e.Url, Size.Fraction(0.3f)),      // 30% for URL

            Text.Label("Medium Size:"),
            products
                .ToTable()
                .Builder(e => e.Url, e => e.Link())
                .Width(Size.Full())
                .MultiLine(e => e.Name)
                // Add explicit column widths to test overflow
                .ColumnWidth(e => e.Sku, Size.Fraction(0.15f))      // 15% for SKU
                .ColumnWidth(e => e.Foo, Size.Fraction(0.1f))       // 10% for Foo  
                .ColumnWidth(e => e.Name, Size.Fraction(0.3f))      // 30% for Name
                .ColumnWidth(e => e.Price, Size.Fraction(0.15f))    // 15% for Price
                .ColumnWidth(e => e.Url, Size.Fraction(0.3f)),      // 30% for URL

            Text.Label("Large Size:"),
            products
                .ToTable().Large()
                .Builder(e => e.Url, e => e.Link())
                .Width(Size.Full())
                .MultiLine(e => e.Name)
                // Add explicit column widths to test overflow
                .ColumnWidth(e => e.Sku, Size.Fraction(0.15f))      // 15% for SKU
                .ColumnWidth(e => e.Foo, Size.Fraction(0.1f))       // 10% for Foo  
                .ColumnWidth(e => e.Name, Size.Fraction(0.3f))      // 30% for Name
                .ColumnWidth(e => e.Price, Size.Fraction(0.15f))    // 15% for Price
                .ColumnWidth(e => e.Url, Size.Fraction(0.3f)),      // 30% for URL


            Text.H3("Long Headers Table (Test Overflow & Tooltips)"),
            longHeaderTable.Width(Size.Full())
        );
    }
}

public class ColumnWidthsExample : ViewBase
{
    public override object? Build()
    {
        var records = new[]
        {
            new Record { Reference = "REF-001", Contact = "John Doe", Views = 1250 },
            new Record { Reference = "REF-002", Contact = "Jane Smith", Views = 890 },
            new Record { Reference = "REF-003", Contact = "Bob Johnson", Views = 2100 },
            new Record { Reference = "REF-004", Contact = "Alice Williams", Views = 567 },
            new Record { Reference = "REF-005", Contact = "Charlie Brown", Views = 3450 }
        };

        return Layout.Vertical()
            | Text.H3("Column Width Configurations")

            | Text.Label("Test 1: Fixed Width + Two Fractions (0.5 each) + Fit")
            | Text.P("Width: Size.Units(100), Reference: Fraction(0.5), Contact: Fraction(0.5), Views: Fit()")
            | records
                .ToTable()
                .Width(Size.Units(100))
                .ColumnWidth(e => e.Reference, Size.Fraction(0.5f))
                .ColumnWidth(e => e.Contact, Size.Fraction(0.5f))
                .ColumnWidth(e => e.Views, Size.Fit())

            | Text.Label("Test 2: Full Width + Two Fractions (0.5 each) + Fit")
            | Text.P("Width: Size.Full(), Reference: Fraction(0.5), Contact: Fraction(0.5), Views: Fit()")
            | records
                .ToTable()
                .Width(Size.Full())
                .ColumnWidth(e => e.Reference, Size.Fraction(0.5f))
                .ColumnWidth(e => e.Contact, Size.Fraction(0.5f))
                .ColumnWidth(e => e.Views, Size.Fit())

            | Text.Label("Test 3: Fixed Width (Units) + Mixed Fractions")
            | Text.P("Width: Size.Units(120), Reference: Fraction(0.3), Contact: Fraction(0.4), Views: Fraction(0.3)")
            | records
                .ToTable()
                .Width(Size.Units(120))
                .ColumnWidth(e => e.Reference, Size.Fraction(0.3f))
                .ColumnWidth(e => e.Contact, Size.Fraction(0.4f))
                .ColumnWidth(e => e.Views, Size.Fraction(0.3f))

            | Text.Label("Test 4: Fixed Width + One Fraction + Fixed Width")
            | Text.P("Width: Size.Units(100), Reference: Units(20), Contact: Fraction(1.0), Views: Units(15)")
            | records
                .ToTable()
                .Width(Size.Units(100))
                .ColumnWidth(e => e.Reference, Size.Units(20))
                .ColumnWidth(e => e.Contact, Size.Fraction(1.0f))
                .ColumnWidth(e => e.Views, Size.Units(15))

            | Text.Label("Test 5: Full Width + All Fractions (equal)")
            | Text.P("Width: Size.Full(), Reference: Fraction(0.33), Contact: Fraction(0.33), Views: Fraction(0.34)")
            | records
                .ToTable()
                .Width(Size.Full())
                .ColumnWidth(e => e.Reference, Size.Fraction(0.33f))
                .ColumnWidth(e => e.Contact, Size.Fraction(0.33f))
                .ColumnWidth(e => e.Views, Size.Fraction(0.34f))

            | Text.Label("Test 6: Fixed Width + Fit + Fraction")
            | Text.P("Width: Size.Units(100), Reference: Fit(), Contact: Fraction(0.6), Views: Fraction(0.4)")
            | records
                .ToTable()
                .Width(Size.Units(100))
                .ColumnWidth(e => e.Reference, Size.Fit())
                .ColumnWidth(e => e.Contact, Size.Fraction(0.6f))
                .ColumnWidth(e => e.Views, Size.Fraction(0.4f))

            | Text.Label("Test 7: With Action Buttons Column (like DeckLinksBlade)")
            | Text.P("Width: Size.Units(100), Reference: Fraction(0.5), Contact: Fraction(0.5), Actions: Fit()")
            | records
                .Select(dl => new
                {
                    dl.Reference,
                    dl.Contact,
                    dl.Views,
                    Actions = Layout.Horizontal().Gap(1)
                        | Icons.Ellipsis
                            .ToButton()
                            .Ghost()
                            .WithDropDown(
                                MenuItem.Default("Edit").Icon(Icons.Pencil).HandleSelect(() => { }),
                                MenuItem.Default("Delete").Icon(Icons.Trash).HandleSelect(() => { })
                            )
                        | Icons.Clipboard
                            .ToButton()
                            .Outline()
                            .Tooltip("Copy Link")
                            .HandleClick(() => { })
                })
                .ToTable()
                .Width(Size.Units(100))
                .ColumnWidth(e => e.Reference, Size.Fraction(0.5f))
                .ColumnWidth(e => e.Contact, Size.Fraction(0.5f))
                .ColumnWidth(e => e.Views, Size.Fit())
                .ColumnWidth(e => e.Actions, Size.Fit());
    }
}

public class AlignmentExample : ViewBase
{
    public override object? Build()
    {
        var records = new[]
        {
            new Record { Reference = "REF-001", Contact = "John Doe", Views = 1250 },
            new Record { Reference = "REF-002", Contact = "Jane Smith", Views = 890 },
            new Record { Reference = "REF-003", Contact = "Bob Johnson", Views = 2100 },
            new Record { Reference = "REF-004", Contact = "Alice Williams", Views = 567 },
            new Record { Reference = "REF-005", Contact = "Charlie Brown", Views = 3450 }
        };

        return Layout.Vertical()
            | Text.H3("Column Alignment with Different Widths")
            | Text.P("Alignment applies to both headers and data cells (rows). Use .Align() to control how content is aligned within cells.")

            | Text.Label("Example 1: Narrow Column (Fit) - Right Aligned Numbers")
            | Text.P("Views column uses Fit() width with Right alignment to show numbers clearly")
            | records
                .ToTable()
                .Width(Size.Full())
                .ColumnWidth(e => e.Reference, Size.Fraction(0.4f))
                .ColumnWidth(e => e.Contact, Size.Fraction(0.4f))
                .ColumnWidth(e => e.Views, Size.Fit())
                .Align(e => e.Views, Align.Right)

            | Text.Label("Example 2: Wide Column (Fraction) - Center Aligned Text")
            | Text.P("Contact column uses Fraction(0.6) width with Center alignment")
            | records
                .ToTable()
                .Width(Size.Full())
                .ColumnWidth(e => e.Reference, Size.Fraction(0.2f))
                .ColumnWidth(e => e.Contact, Size.Fraction(0.6f))
                .ColumnWidth(e => e.Views, Size.Fraction(0.2f))
                .Align(e => e.Contact, Align.Center)
                .Align(e => e.Views, Align.Right)

            | Text.Label("Example 3: Mixed Alignments with Fixed Widths")
            | Text.P("Reference: Left (default), Contact: Center, Views: Right")
            | records
                .ToTable()
                .Width(Size.Units(100))
                .ColumnWidth(e => e.Reference, Size.Units(25))
                .ColumnWidth(e => e.Contact, Size.Fraction(1.0f))
                .ColumnWidth(e => e.Views, Size.Units(20))
                .Align(e => e.Contact, Align.Center)
                .Align(e => e.Views, Align.Right)

            | Text.Label("Example 4: All Right Aligned in Narrow Columns")
            | Text.P("All columns right-aligned with narrow widths to demonstrate alignment effect")
            | records
                .ToTable()
                .Width(Size.Units(80))
                .ColumnWidth(e => e.Reference, Size.Fraction(0.33f))
                .ColumnWidth(e => e.Contact, Size.Fraction(0.33f))
                .ColumnWidth(e => e.Views, Size.Fraction(0.34f))
                .Align(e => e.Reference, Align.Right)
                .Align(e => e.Contact, Align.Right)
                .Align(e => e.Views, Align.Right)

            | Text.Label("Example 5: Wide Center Column with Narrow Side Columns")
            | Text.P("Center column (Contact) uses Fraction(0.7) with Center alignment, side columns are narrow")
            | records
                .ToTable()
                .Width(Size.Full())
                .ColumnWidth(e => e.Reference, Size.Fraction(0.15f))
                .ColumnWidth(e => e.Contact, Size.Fraction(0.7f))
                .ColumnWidth(e => e.Views, Size.Fraction(0.15f))
                .Align(e => e.Contact, Align.Center)
                .Align(e => e.Views, Align.Right);
    }
}