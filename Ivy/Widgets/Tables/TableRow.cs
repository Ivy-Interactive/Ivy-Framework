using Ivy.Core;

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
}