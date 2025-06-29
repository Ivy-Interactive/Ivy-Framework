using Ivy.Shared;
using Ivy.Tables;

namespace Ivy.Samples.Apps.Widgets;

public class Product
{
    public required string Sku { get; set; }
    public required string Name { get; set; }
    public required double Price { get; set; }
    public required string Url { get; set; }
}

[App(icon: Icons.Table, path: ["Widgets"])]
public class TableApp : SampleBase
{
    protected override object? BuildSample()
    {
        //Anonymous type array

        var products = new[] {
            new {Sku = "1234", Name = "T-shirt", Price = 10.0, Url = "http://example.com/tshirt"},
            new {Sku = "1235", Name = "Jeans", Price = 20.0, Url = "http://example.com/jeans"},
            new {Sku = "1236", Name = "Sneakers", Price = 30.0, Url = "http://example.com/sneakers"},
            new {Sku = "1237", Name = "Hat", Price = 5.0, Url = "http://example.com/hat"},
            new {Sku = "1238", Name = "Socks", Price = 2.0, Url = "http://example.com/socks"}
        };

        return products.ToTable()
            .Width(Size.Full())
            // .Width(e => e.Sku, Size.Fraction(0))
            // .Width(e => e.Name, Size.Fraction(1 / 3f))
            // .Width(e => e.Price, Size.Fraction(1 / 3f))
            // .Width(e => e.Url, Size.Fraction(1 / 3f))
            ;

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