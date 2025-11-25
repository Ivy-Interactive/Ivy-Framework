using Ivy.Samples.Shared.Apps;
using Ivy.Shared;
using Ivy.Views.DataTables;

namespace Ivy.Samples.Shared.Apps.Tests;

public record MillionRowData(
    int Id,
    string Value,
    DateTime CreatedAt
);

[App(icon: Icons.Database, path: ["Tests"], isVisible: false, searchHints: ["datatable", "million", "performance", "large", "data"])]
public class DataTablesMillionRowsApp : SampleBase
{
    protected override object? BuildSample()
    {
        // Generate 1 million rows of data
        var millionRows = Enumerable.Range(1, 1_000_000)
            .Select(i => new MillionRowData(
                Id: i,
                Value: $"Row {i:N0}",
                CreatedAt: DateTime.Now.AddSeconds(-i)
            )).AsQueryable();

        return millionRows.ToDataTable()
            .Header(row => row.Id, "ID")
            .Header(row => row.Value, "Value")
            .Header(row => row.CreatedAt, "Created At")
            // Set column widths
            .Width(row => row.Id, Size.Px(100))
            .Width(row => row.Value, Size.Px(200))
            .Width(row => row.CreatedAt, Size.Px(200))
            // Set alignment
            .Align(row => row.Id, Align.Left)
            .Align(row => row.Value, Align.Left)
            .Align(row => row.CreatedAt, Align.Left)
            // Add icons to headers
            .Icon(row => row.Id, Icons.Hash.ToString())
            .Icon(row => row.Value, Icons.Text.ToString())
            .Icon(row => row.CreatedAt, Icons.Calendar.ToString())
            // Configure for performance with large datasets
            .Config(config => config.AllowLlmFiltering = true)
            // Configure to load all 1 million rows at once
            .LoadAllRows(true);
        // Alternative: Set a large batch size instead of loading all at once
        // .BatchSize(100000); // Uncomment this and comment LoadAllRows(true) to use batch loading
    }
}
