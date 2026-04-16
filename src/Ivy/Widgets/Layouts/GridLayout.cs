// ReSharper disable once CheckNamespace
namespace Ivy;

public enum AutoFlow
{
    Row,
    Column,
    RowDense,
    ColumnDense
}

public class GridDefinition
{
    public Responsive<int?>? Columns { get; set; }

    public int? Rows { get; set; }

    public Responsive<int?>? RowGap { get; set; }

    public Responsive<int?>? ColumnGap { get; set; }

    public Thickness Padding { get; set; } = new(0);

    public AutoFlow? AutoFlow { get; set; } = null;

    public Size? Width { get; set; } = null;

    public Size? Height { get; set; } = null;

    public Size?[]? ColumnWidths { get; set; } = null;

    public Size?[]? RowHeights { get; set; } = null;

    public Align? AlignContent { get; set; } = null;

    public Func<int, object, object>? HeaderBuilder { get; set; } = null;

    public Func<int, object, object>? FooterBuilder { get; set; } = null;

    public Func<object, object>? CellBuilder { get; set; } = null;
}

/// <summary>
/// A layout using the CSS Grid system for complex arrangements.
/// </summary>
internal record GridLayout : WidgetBase<GridLayout>
{
    public GridLayout(GridDefinition def, params object[] children) : base(children)
    {
        Columns = def.Columns;
        Rows = def.Rows;
        RowGap = def.RowGap;
        ColumnGap = def.ColumnGap;
        Padding = def.Padding;
        AutoFlow = def.AutoFlow;
        AlignContent = def.AlignContent;
        Width = def.Width.ToResponsive();
        Height = def.Height.ToResponsive();
        ColumnWidths = def.ColumnWidths;
        RowHeights = def.RowHeights;
    }

    internal GridLayout()
    {
        RowGap = 4;
        ColumnGap = 4;
    }

    [Prop] public Responsive<int?>? Columns { get; set; }

    [Prop] public int? Rows { get; set; }

    [Prop] public Responsive<int?>? RowGap { get; set; }

    [Prop] public Responsive<int?>? ColumnGap { get; set; }

    [Prop] public Thickness Padding { get; set; } = new(0);

    [Prop] public AutoFlow? AutoFlow { get; set; }

    [Prop] public Align? AlignContent { get; set; }

    [Prop] public Size?[]? ColumnWidths { get; set; }

    [Prop] public Size?[]? RowHeights { get; set; }

    [Prop(attached: nameof(GridExtensions.GridColumn))] public int?[] ChildColumn { get; set; } = null!;

    [Prop(attached: nameof(GridExtensions.GridColumnSpan))] public int?[] ChildColumnSpan { get; set; } = null!;

    [Prop(attached: nameof(GridExtensions.GridRow))] public int?[] ChildRow { get; set; } = null!;

    [Prop(attached: nameof(GridExtensions.GridRowSpan))] public int?[] ChildRowSpan { get; set; } = null!;

    [Prop(attached: nameof(StackLayoutExtensions.AlignSelf))] public Align?[] ChildAlignSelf { get; set; } = null!;
}

public static class GridExtensions
{
    public static WidgetBase<T> GridColumn<T>(this WidgetBase<T> child, int column) where T : WidgetBase<T>
    {
        child.SetAttachedValue(typeof(GridLayout), nameof(GridColumn), column);
        return child;
    }

    public static WidgetBase<T> GridColumnSpan<T>(this WidgetBase<T> child, int columnSpan) where T : WidgetBase<T>
    {
        child.SetAttachedValue(typeof(GridLayout), nameof(GridColumnSpan), columnSpan);
        return child;
    }

    public static WidgetBase<T> GridRow<T>(this WidgetBase<T> child, int row) where T : WidgetBase<T>
    {
        child.SetAttachedValue(typeof(GridLayout), nameof(GridRow), row);
        return child;
    }

    public static WidgetBase<T> GridRowSpan<T>(this WidgetBase<T> child, int rowSpan) where T : WidgetBase<T>
    {
        child.SetAttachedValue(typeof(GridLayout), nameof(GridRowSpan), rowSpan);
        return child;
    }
}