using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A standard HTML table container.
/// </summary>
public record Table : WidgetBase<Table>
{
    public Table(params TableRow[] rows) : base(rows.Cast<object>().ToArray())
    {
    }

    internal Table() { }

    [Prop] public string Layout { get; set; } = "Auto";

    public static Table operator |(Table table, TableRow child)
    {
        return table with { Children = [.. table.Children, child] };
    }
}

public static partial class TableExtensions
{
    public static Table Layout(this Table table, string layout)
    {
        return table with { Layout = layout };
    }
}