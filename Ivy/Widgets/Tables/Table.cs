using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record Table : WidgetBase<Table>
{
    public Table(params TableRow[] rows) : base(rows.Cast<object>().ToArray())
    {
    }

    [Prop] public Sizes Size { get; set; } = Sizes.Medium;

    public static Table operator |(Table table, TableRow child)
    {
        return table with { Children = [.. table.Children, child] };
    }
}

public static class TableExtensions
{
    public static Table Size(this Table widget, Sizes size) => widget with { Size = size };

    public static Table Large(this Table widget) => widget.Size(Sizes.Large);

    public static Table Small(this Table widget) => widget.Size(Sizes.Small);

    public static Table Medium(this Table widget) => widget.Size(Sizes.Medium);
}