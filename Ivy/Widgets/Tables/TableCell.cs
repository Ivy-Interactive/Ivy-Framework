using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Represents a table cell widget that contains individual data or header content within a table row.
/// This widget provides the fundamental building block for table structures.
/// 
/// The TableCell widget supports different cell types (header, footer, data), content alignment,
/// and flexible content display. It integrates seamlessly with <see cref="TableRow"/> and <see cref="Table"/>
/// components to create comprehensive table structures for data presentation and organization.
/// </summary>
public record TableCell : WidgetBase<TableCell>
{
    /// <summary>
    /// Initializes a new instance of the TableCell class with the specified content.
    /// The cell will display the provided content and can be configured with various
    /// styling and semantic properties to define its role and appearance in the table.
    /// </summary>
    /// <param name="content">The content to display within the table cell. This can be any
    /// combination of widgets, text, or other content elements. When null, an empty cell
    /// is created that can be populated later.</param>
    public TableCell(object? content) : base(content != null ? [content] : [])
    {
    }

    /// <summary>
    /// Gets or sets whether this cell should be treated as a header cell.
    /// When true, the cell is semantically marked as a header and typically receives
    /// special styling such as bold text, background colors, or other visual emphasis
    /// to distinguish it from regular data cells.
    /// 
    /// Header cells are commonly used in the first row of a table to provide column
    /// labels or in the first column to provide row labels, creating clear structure
    /// and improving table readability.
    /// Default is false (regular data cell).
    /// </summary>
    [Prop] public bool IsHeader { get; set; }

    /// <summary>
    /// Gets or sets whether this cell should be treated as a footer cell.
    /// When true, the cell is semantically marked as a footer and typically receives
    /// special styling such as bold text, background colors, or other visual emphasis
    /// to distinguish it from regular data cells.
    /// 
    /// Footer cells are commonly used in the last row of a table to provide summary
    /// information, totals, or other aggregated data, creating clear visual separation
    /// from the main data content.
    /// Default is false (regular data cell).
    /// </summary>
    [Prop] public bool IsFooter { get; set; }

    /// <summary>
    /// Gets or sets the alignment for the cell's content within the available space.
    /// This property controls how the cell's content is positioned horizontally and
    /// vertically, allowing for precise control over content layout and visual balance.
    /// 
    /// The alignment affects how text, widgets, and other content are positioned
    /// within the cell boundaries, creating consistent and professional-looking
    /// table layouts.
    /// Default is the default alignment behavior for the current context.
    /// </summary>
    [Prop] public Align Align { get; set; }

    /// <summary>
    /// Gets or sets whether the cell content should be displayed in multi-line format.
    /// When true, the content is rendered with line breaks and expanded spacing,
    /// allowing for better display of longer text content within the cell.
    /// When false, the content is displayed in a compact, single-line format.
    /// Default is false (single-line format).
    /// </summary>
    [Prop] public bool MultiLine { get; set; }
}

/// <summary>
/// Provides extension methods for the TableCell widget that enable a fluent API for
/// configuring cell properties and styling. These methods allow you to easily set
/// header/footer status and content alignment for optimal table appearance and
/// semantic structure.
/// </summary>
public static class TableCellExtensions
{
    /// <summary>
    /// Sets whether the table cell should be treated as a header cell.
    /// This method allows you to configure the cell's semantic role after creation,
    /// enabling or disabling header styling and behavior as needed.
    /// </summary>
    /// <param name="cell">The TableCell to configure.</param>
    /// <param name="isHeader">Whether the cell should be treated as a header. Default is true.</param>
    /// <returns>A new TableCell instance with the updated header status.</returns>
    public static TableCell IsHeader(this TableCell cell, bool isHeader = true)
    {
        return cell with { IsHeader = isHeader };
    }

    /// <summary>
    /// Sets whether the table cell should be treated as a footer cell.
    /// This method allows you to configure the cell's semantic role after creation,
    /// enabling or disabling footer styling and behavior as needed.
    /// </summary>
    /// <param name="cell">The TableCell to configure.</param>
    /// <param name="isFooter">Whether the cell should be treated as a footer. Default is true.</param>
    /// <returns>A new TableCell instance with the updated footer status.</returns>
    public static TableCell IsFooter(this TableCell cell, bool isFooter = true)
    {
        return cell with { IsFooter = isFooter };
    }

    /// <summary>
    /// Sets the alignment for the cell's content within the available space.
    /// This method allows you to control how content is positioned within the cell,
    /// creating consistent and professional-looking table layouts.
    /// </summary>
    /// <param name="cell">The TableCell to configure.</param>
    /// <param name="align">The alignment setting to apply to the cell's content.</param>
    /// <returns>A new TableCell instance with the updated alignment setting.</returns>
    public static TableCell Align(this TableCell cell, Align align)
    {
        return cell with { Align = align };
    }

    /// <summary>
    /// Sets whether the cell content should be displayed in multi-line format.
    /// This method allows you to control how content is displayed within the cell,
    /// enabling better display of longer text content with line breaks.
    /// </summary>
    /// <param name="cell">The TableCell to configure.</param>
    /// <param name="multiLine">Whether the cell content should be displayed in multi-line format.</param>
    /// <returns>A new TableCell instance with the updated multi-line setting.</returns>
    public static TableCell MultiLine(this TableCell cell, bool multiLine = true)
    {
        return cell with { MultiLine = multiLine };
    }
}