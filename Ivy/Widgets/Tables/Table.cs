using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record Table : WidgetBase<Table>
{
    public Table(params TableRow[] rows) : base(rows.Cast<object>().ToArray())
    {
    }

    public static Table operator |(Table table, TableRow child)
    {
        return table with { Children = [.. table.Children, child] };
    }
}