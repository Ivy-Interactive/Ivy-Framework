namespace Ivy.Tests.Views.DataTables;

public class DataTableFooterFormatTests
{
    private record SalesRow(string Product, decimal Amount, double Rate, int Count);

    private static IQueryable<SalesRow> SampleData() =>
        new[]
        {
            new SalesRow("A", 10_543.56m, 0.152, 100),
            new SalesRow("B", 150_587.95m, 0.348, 200),
        }.AsQueryable();

    private static List<string> GetFooter(DataTableBuilder<SalesRow> builder, string columnName)
    {
        // Access the private _columns dictionary via reflection
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        var columnsField = builder.GetType().GetField("_columns", flags)!;
        var columnsObj = columnsField.GetValue(builder)!;

        // Get the InternalColumn via the dictionary's Item property (indexer)
        var dictType = columnsObj.GetType();
        var itemProp = dictType.GetProperty("Item")!;
        var internalColumn = itemProp.GetValue(columnsObj, [columnName])!;

        // Get the Column property from InternalColumn
        var columnProp = internalColumn.GetType().GetProperty("Column")!;
        var column = (DataTableColumn)columnProp.GetValue(internalColumn)!;
        return column.Footer ?? [];
    }

    [Fact]
    public void Footer_WithCurrencyFormat_FormatsAggregateWithCurrencySymbol()
    {
        var builder = SampleData().ToDataTable()
            .Format(x => x.Amount, NumberFormatStyle.Currency, precision: 2, currency: "USD")
            .Footer(x => x.Amount, "Total", values => values.Sum());

        var footer = GetFooter(builder, "Amount");
        Assert.Single(footer);
        Assert.Contains("$", footer[0]);
        Assert.Contains("161,131.51", footer[0]);
        Assert.StartsWith("Total: ", footer[0]);
    }

    [Fact]
    public void Footer_WithPercentFormat_FormatsAsPercentage()
    {
        var builder = SampleData().ToDataTable()
            .Format(x => x.Rate, NumberFormatStyle.Percent, precision: 1)
            .Footer(x => x.Rate, "Avg", values => values.Average());

        var footer = GetFooter(builder, "Rate");
        Assert.Single(footer);
        // Average of 0.152 and 0.348 = 0.25 → 25.0%
        Assert.Contains("25.0", footer[0]);
        Assert.Contains("%", footer[0]);
    }

    [Fact]
    public void Footer_WithDecimalFormat_FormatsWithGroupingSeparators()
    {
        var builder = SampleData().ToDataTable()
            .Format(x => x.Count, NumberFormatStyle.Decimal, precision: 0)
            .Footer(x => x.Count, "Total", values => values.Sum());

        var footer = GetFooter(builder, "Count");
        Assert.Single(footer);
        Assert.Equal("Total: 300", footer[0]);
    }

    [Fact]
    public void Footer_WithoutFormatStyle_PassesThroughToString()
    {
        var builder = SampleData().ToDataTable()
            .Footer(x => x.Count, "Total", values => values.Sum());

        var footer = GetFooter(builder, "Count");
        Assert.Single(footer);
        Assert.Equal("Total: 300", footer[0]);
    }

    [Fact]
    public void Footer_MultipleAggregates_WithCurrencyFormat_EachGetsFormatted()
    {
        var builder = SampleData().ToDataTable()
            .Format(x => x.Amount, NumberFormatStyle.Currency, precision: 2, currency: "USD")
            .Footer(x => x.Amount, new (string, Func<IEnumerable<decimal>, object>)[]
            {
                ("Sum", values => values.Sum()),
                ("Avg", values => values.Average()),
            });

        var footer = GetFooter(builder, "Amount");
        Assert.Equal(2, footer.Count);
        Assert.Contains("$", footer[0]);
        Assert.Contains("161,131.51", footer[0]);
        Assert.Contains("$", footer[1]);
        Assert.Contains("80,565.76", footer[1]);
    }
}
