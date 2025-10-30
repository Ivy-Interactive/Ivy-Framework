using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Table row widget containing horizontal collection of table cells with support for different row types and pipe operator for cell addition.</summary>
public record TableRow : WidgetBase<TableRow>
{
    /// <summary>Initializes TableRow with specified table cells.</summary>
    /// <param name="cells">TableCell elements defining row's content and structure.</param>
    public TableRow(params TableCell[] cells) : base(cells.Cast<object>().ToArray())
    {
    }

    /// <summary>Whether row should be treated as header row with special styling. Default is false.</summary>
    [Prop] public bool IsHeader { get; set; }

    /// <summary>Gets or sets the size of the table row.</summary>
    [Prop] public Sizes Size { get; set; } = Sizes.Medium;

    /// <summary>Allows adding single TableCell using pipe operator for convenient row construction.</summary>
    /// <param name="row">TableRow to add cell to.</param>
    /// <param name="child">TableCell to add to row.</param>
    /// <returns>New TableRow instance with additional cell appended.</returns>
    public static TableRow operator |(TableRow row, TableCell child)
    {
        return row with { Children = [.. row.Children, child] };
    }
}

/// <summary>Extension methods for TableRow widget providing fluent API for configuring row properties and styling.</summary>
public static class TableRowExtensions
{
    /// <summary>Sets whether table row should be treated as header row.</summary>
    /// <param name="row">TableRow to configure.</param>
    /// <param name="isHeader">Whether row should be treated as header. Default is true.</param>
    /// <returns>New TableRow instance with updated header status.</returns>
    public static TableRow IsHeader(this TableRow row, bool isHeader = true)
    {
        return row with { IsHeader = isHeader };
    }

    /// <summary>Sets the size of the table row.</summary>
    /// <param name="row">The table row to configure.</param>
    /// <param name="size">The size to apply to the table row.</param>
    public static TableRow Size(this TableRow row, Sizes size) => row with { Size = size };

    /// <summary>Sets the table row size to large for prominent display.</summary>
    /// <param name="row">The table row to configure.</param>
    public static TableRow Large(this TableRow row) => row.Size(Sizes.Large);

    /// <summary>Sets the table row size to small for compact display.</summary>
    /// <param name="row">The table row to configure.</param>
    public static TableRow Small(this TableRow row) => row.Size(Sizes.Small);

    /// <summary>Sets the table row size to medium for medium display.</summary>
    /// <param name="row">The table row to configure.</param>
    public static TableRow Medium(this TableRow row) => row.Size(Sizes.Medium);
}