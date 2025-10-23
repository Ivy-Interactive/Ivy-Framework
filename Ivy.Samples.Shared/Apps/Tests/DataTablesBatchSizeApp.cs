using Ivy.Samples.Shared.Apps;
using Ivy.Shared;
using Ivy.Views.DataTables;

namespace Ivy.Samples.Shared.Apps.Tests;

public record BatchSizeData(
    int Id,
    string Value,
    DateTime CreatedAt
);

[App(icon: Icons.Database, path: ["Tests"], isVisible: true, searchHints: ["datatable", "batch", "size", "pagination", "performance"])]
public class DataTablesBatchSizeApp : SampleBase
{
    protected override object? BuildSample()
    {
        // Generate 500,000 rows of data
        var batchRows = Enumerable.Range(1, 500_000)
            .Select(i => new BatchSizeData(
                Id: i,
                Value: $"Batch Row {i:N0}",
                CreatedAt: DateTime.Now.AddSeconds(-i)
            )).AsQueryable();

        return batchRows.ToDataTable()
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
            .Icon(row => row.Id, Icons.Hash)
            .Icon(row => row.Value, Icons.Text)
            .Icon(row => row.CreatedAt, Icons.Calendar)
            // Configure for performance with large datasets
            .Config(config => config.AllowLlmFiltering = true)
            // Use large batch size for better performance
            .BatchSize(50000); // Load 50,000 rows per batch
    }
}
