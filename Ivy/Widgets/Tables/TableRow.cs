using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record TableRow : WidgetBase<TableRow>
{
    public TableRow(params TableCell[] cells) : base(cells.Cast<object>().ToArray())
    {
    }

    [Prop] public bool IsHeader { get; set; }

    public static TableRow operator |(TableRow row, TableCell child)
    {
        return row with { Children = [.. row.Children, child] };
    }
}

public static class TableRowExtensions
{
    public static TableRow IsHeader(this TableRow row, bool isHeader = true)
    {
        return row with { IsHeader = isHeader };
    }
}