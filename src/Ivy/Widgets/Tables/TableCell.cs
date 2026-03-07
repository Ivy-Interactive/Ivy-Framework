using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A data cell within a TableRow.
/// </summary>
public record TableCell : WidgetBase<TableCell>
{
    public TableCell(object? content) : base(content != null ? [content] : [])
    {
    }

    internal TableCell()
    {
    }

    [Prop] public bool IsHeader { get; set; }

    [Prop] public bool IsFooter { get; set; }

    [Prop] public Align Align { get; set; } = Align.Left;

    [Prop] public bool Multiline { get; set; }
}

public static class TableCellExtensions
{
    public static TableCell IsHeader(this TableCell cell, bool isHeader = true)
    {
        return cell with { IsHeader = isHeader };
    }

    public static TableCell IsFooter(this TableCell cell, bool isFooter = true)
    {
        return cell with { IsFooter = isFooter };
    }

    public static TableCell Align(this TableCell cell, Align align)
    {
        return cell with { Align = align };
    }

    public static TableCell Multiline(this TableCell cell, bool multiline = true)
    {
        return cell with { Multiline = multiline };
    }
}