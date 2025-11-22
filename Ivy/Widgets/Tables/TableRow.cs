using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record TableRow : WidgetBase<TableRow>
{
    public TableRow(params TableCell[] cells) : base(cells.Cast<object>().ToArray())
    {
    }

    [Prop] public bool IsHeader { get; set; }

    [Prop] public Sizes Size { get; set; } = Sizes.Medium;

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

    public static TableRow Size(this TableRow row, Sizes size) => row with { Size = size };

    public static TableRow Large(this TableRow row) => row.Size(Sizes.Large);

    public static TableRow Small(this TableRow row) => row.Size(Sizes.Small);

    public static TableRow Medium(this TableRow row) => row.Size(Sizes.Medium);
}