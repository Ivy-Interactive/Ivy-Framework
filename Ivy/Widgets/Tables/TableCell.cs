using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Table cell widget containing individual data or header content with support for different cell types, alignment, and flexible display.</summary>
public record TableCell : WidgetBase<TableCell>
{
    /// <summary>Initializes TableCell with specified content.</summary>
    /// <param name="content">Content to display within table cell. When null, creates empty cell.</param>
    public TableCell(object? content) : base(content != null ? [content] : [])
    {
    }

    /// <summary>Whether cell should be treated as header cell with special styling. Default is false.</summary>
    [Prop] public bool IsHeader { get; set; }

    /// <summary>Whether cell should be treated as footer cell with special styling. Default is false.</summary>
    [Prop] public bool IsFooter { get; set; }

    /// <summary>Alignment for cell's content within available space controlling positioning and layout.</summary>
    [Prop] public Align Align { get; set; }

    /// <summary>Whether cell content should be displayed in multi-line format. Default is false (single-line).</summary>
    [Prop] public bool MultiLine { get; set; }
}

/// <summary>Extension methods for TableCell widget providing fluent API for configuring cell properties and styling.</summary>
public static class TableCellExtensions
{
    /// <summary>Sets whether table cell should be treated as header cell.</summary>
    /// <param name="cell">TableCell to configure.</param>
    /// <param name="isHeader">Whether cell should be treated as header. Default is true.</param>
    /// <returns>New TableCell instance with updated header status.</returns>
    public static TableCell IsHeader(this TableCell cell, bool isHeader = true)
    {
        return cell with { IsHeader = isHeader };
    }

    /// <summary>Sets whether table cell should be treated as footer cell.</summary>
    /// <param name="cell">TableCell to configure.</param>
    /// <param name="isFooter">Whether cell should be treated as footer. Default is true.</param>
    /// <returns>New TableCell instance with updated footer status.</returns>
    public static TableCell IsFooter(this TableCell cell, bool isFooter = true)
    {
        return cell with { IsFooter = isFooter };
    }

    /// <summary>Sets alignment for cell's content within available space.</summary>
    /// <param name="cell">TableCell to configure.</param>
    /// <param name="align">Alignment setting to apply to cell's content.</param>
    /// <returns>New TableCell instance with updated alignment setting.</returns>
    public static TableCell Align(this TableCell cell, Align align)
    {
        return cell with { Align = align };
    }

    /// <summary>Sets whether cell content should be displayed in multi-line format.</summary>
    /// <param name="cell">TableCell to configure.</param>
    /// <param name="multiLine">Whether cell content should be displayed in multi-line format.</param>
    /// <returns>New TableCell instance with updated multi-line setting.</returns>
    public static TableCell MultiLine(this TableCell cell, bool multiLine = true)
    {
        return cell with { MultiLine = multiLine };
    }
}