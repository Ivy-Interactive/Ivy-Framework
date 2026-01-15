using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A highly performant table widget for displaying structured data rows.
/// </summary>
public record Table : WidgetBase<Table>
{
    public Table(params TableRow[] rows) : base(rows.Cast<object>().ToArray())
    {
    }

    internal Table() { }

    public static Table operator |(Table table, TableRow child)
    {
        return table with { Children = [.. table.Children, child] };
    }
}

public static class TableExtensions
{
}