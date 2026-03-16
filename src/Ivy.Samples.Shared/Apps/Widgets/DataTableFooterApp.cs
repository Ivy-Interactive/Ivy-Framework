namespace Ivy.Samples.Shared.Apps.Widgets;

[App(icon: Icons.Calculator)]
public class DataTableFooterApp : SampleBase
{
    protected override object? BuildSample()
    {
        var invoiceLines = new[]
        {
            new { Product = "Pro License", Qty = 15, UnitPrice = 99.00m, Amount = 1485.00m },
            new { Product = "Support Hours", Qty = 40, UnitPrice = 125.00m, Amount = 5000.00m },
            new { Product = "Training Session", Qty = 3, UnitPrice = 500.00m, Amount = 1500.00m },
            new { Product = "Custom Development", Qty = 80, UnitPrice = 150.00m, Amount = 12000.00m },
            new { Product = "Hosting (Annual)", Qty = 1, UnitPrice = 1200.00m, Amount = 1200.00m }
        }.AsQueryable();

        return invoiceLines.ToDataTable()
            .Header(x => x.Product, "Product / Service")
            .Header(x => x.Qty, "Quantity")
                .Footer(x => x.Qty, "Total", values => values.Sum())
            .Header(x => x.UnitPrice, "Unit Price")
                .Footer(x => x.UnitPrice, "Avg", values => values.Average())
            .Header(x => x.Amount, "Amount")
                .Footer(x => x.Amount, "Total", values => values.Sum())
            .Width(x => x.Product, Size.Px(200))
            .Width(x => x.Qty, Size.Units(30))
            .Width(x => x.UnitPrice, Size.Units(30))
            .Width(x => x.Amount, Size.Units(30))
            .Align(x => x.Qty, Align.Right)
            .Align(x => x.UnitPrice, Align.Right)
            .Align(x => x.Amount, Align.Right)
            .Height(Size.Units(80));
    }
}

[App(icon: Icons.TrendingUp)]
public class DataTableMultiAggApp : SampleBase
{
    protected override object? BuildSample()
    {
        var salesData = new[]
        {
            new { Region = "North", Sales = 45000m, Target = 50000m },
            new { Region = "South", Sales = 62000m, Target = 60000m },
            new { Region = "East", Sales = 38000m, Target = 45000m },
            new { Region = "West", Sales = 71000m, Target = 70000m }
        }.AsQueryable();

        return salesData.ToDataTable()
            .Header(x => x.Region, "Sales Region")
            .Header(x => x.Sales, "Actual Sales")
                .Footer(x => x.Sales, new[]
                {
                    ("Total", (Func<IEnumerable<decimal>, object>)(values => values.Sum())),
                    ("Avg", (Func<IEnumerable<decimal>, object>)(values => values.Average()))
                })
            .Header(x => x.Target, "Target")
                .Footer(x => x.Target, new[]
                {
                    ("Total", (Func<IEnumerable<decimal>, object>)(values => values.Sum())),
                    ("Avg", (Func<IEnumerable<decimal>, object>)(values => values.Average()))
                })
            .Align(x => x.Sales, Align.Right)
            .Align(x => x.Target, Align.Right)
            .Height(Size.Units(60));
    }
}
