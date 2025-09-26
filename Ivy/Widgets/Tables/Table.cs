using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Table widget displaying structured data in tabular format with <see cref="TableRow"/> elements supporting pipe operator for easy row addition.</summary>
public record Table : WidgetBase<Table>
{
    /// <summary>Initializes Table with specified table rows.</summary>
    /// <param name="rows">TableRow elements defining table structure and content.</param>
    public Table(params TableRow[] rows) : base(rows.Cast<object>().ToArray())
    {
    }

    /// <summary>Allows adding single TableRow using pipe operator for convenient table construction.</summary>
    /// <param name="table">Table to add row to.</param>
    /// <param name="child">TableRow to add to table.</param>
    /// <returns>New Table instance with additional row appended.</returns>
    public static Table operator |(Table table, TableRow child)
    {
        return table with { Children = [.. table.Children, child] };
    }
}