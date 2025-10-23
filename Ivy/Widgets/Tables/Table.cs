using Ivy.Core;
using Ivy.Shared;

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

    /// <summary>Gets or sets the size of the table.</summary>
    [Prop] public Sizes Size { get; set; } = Sizes.Medium;

    /// <summary>Allows adding single TableRow using pipe operator for convenient table construction.</summary>
    /// <param name="table">Table to add row to.</param>
    /// <param name="child">TableRow to add to table.</param>
    /// <returns>New Table instance with additional row appended.</returns>
    public static Table operator |(Table table, TableRow child)
    {
        return table with { Children = [.. table.Children, child] };
    }
}

/// <summary> Provides extension methods for configuring table widgets with fluent syntax. </summary>
public static class TableExtensions
{
    /// <summary>Sets the size of the table.</summary>
    /// <param name="widget">The table to configure.</param>
    /// <param name="size">The size to apply to the table.</param>
    public static Table Size(this Table widget, Sizes size) => widget with { Size = size };

    /// <summary>Sets the table size to large for prominent display.</summary>
    /// <param name="widget">The table to configure.</param>
    public static Table Large(this Table widget) => widget.Size(Sizes.Large);

    /// <summary>Sets the table size to small for compact display.</summary>
    /// <param name="widget">The table to configure.</param>
    public static Table Small(this Table widget) => widget.Size(Sizes.Small);

    /// <summary>Sets the table size to medium for medium display.</summary>
    /// <param name="widget">The table to configure.</param>
    public static Table Medium(this Table widget) => widget.Size(Sizes.Medium);
}