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

[App(icon: Icons.Table, path: ["Widgets"], searchHints: ["grid", "data", "rows", "columns", "cells", "spreadsheet"])]
public class TableApp : SampleBase
{
    protected override object? BuildSample()
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

        // return
        //     Layout.Vertical()
        //         //Low abstraction
        //         | new Table(
        //             new TableRow(new TableCell("Name"), new TableCell("Age")).IsHeader(),
        //             new TableRow(new TableCell("Niels"), new TableCell("25")),
        //             new TableRow(new TableCell("John"), new TableCell("30"))
        //         )
        //         //High abstraction
        //         | products.ToTable()
        //         ;
    }
}