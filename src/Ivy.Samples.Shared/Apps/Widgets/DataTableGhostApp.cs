namespace Ivy.Samples.Shared.Apps.Widgets;

[App(icon: Icons.Ghost, group: ["Widgets"])]
public class DataTableGhostApp : SampleBase
{
    protected override object? BuildSample()
    {
        var products = new[]
        {
            new { Id = 1, Product = "Widget A", Price = 29.99m, Stock = 150 },
            new { Id = 2, Product = "Widget B", Price = 49.99m, Stock = 85 },
            new { Id = 3, Product = "Widget C", Price = 19.99m, Stock = 320 },
            new { Id = 4, Product = "Gadget X", Price = 99.99m, Stock = 42 },
            new { Id = 5, Product = "Gadget Y", Price = 149.99m, Stock = 18 },
        }.AsQueryable();

        var ghostTable = products.ToDataTable()
            .Ghost()
            .Height(Size.Full());

        var defaultTable = products.ToDataTable()
            .Height(Size.Units(60));

        return Layout.Vertical()
            | Text.H2("Ghost variant inside a Card")
            | new Card(ghostTable).Height(Size.Units(60))
            | Text.H2("Default variant for comparison")
            | defaultTable;
    }
}
