using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record TableCell : WidgetBase<TableCell>
{
    /// <param name="content">When null, creates empty cell.</param>
    public TableCell(object? content) : base(content != null ? [content] : [])
    {
    }

    [Prop] public bool IsHeader { get; set; }

    [Prop] public bool IsFooter { get; set; }

    [Prop] public Align Align { get; set; }

    /// <summary>Default is false (single-line).</summary>
    [Prop] public bool MultiLine { get; set; }

    [Prop] public Sizes Size { get; set; } = Sizes.Medium;
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

    public static TableCell MultiLine(this TableCell cell, bool multiLine = true)
    {
        return cell with { MultiLine = multiLine };
    }

    public static TableCell Size(this TableCell cell, Sizes size) => cell with { Size = size };

    public static TableCell Large(this TableCell cell) => cell.Size(Sizes.Large);

    public static TableCell Small(this TableCell cell) => cell.Size(Sizes.Small);

    public static TableCell Medium(this TableCell cell) => cell.Size(Sizes.Medium);
}