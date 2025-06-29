using Ivy.Core;
using Ivy.Shared;

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
    public int? Columns { get; set; }
    public int? Rows { get; set; }
    public int Gap { get; set; } = 4;
    public int Padding { get; set; } = 0;
    public AutoFlow? AutoFlow { get; set; } = null;
    public Size? Width { get; set; } = null;
    public Size? Height { get; set; } = null;
}

public record GridLayout : WidgetBase<GridLayout>
{
    public GridLayout(GridDefinition def, params object[] children) : base(children)
    {
        Columns = def.Columns;
        Rows = def.Rows;
        Gap = def.Gap;
        Padding = def.Padding;
        AutoFlow = def.AutoFlow;
        Width = def.Width;
        Height = def.Height;
    }

    [Prop] public int? Columns { get; set; }
    [Prop] public int? Rows { get; set; }
    [Prop] public int Gap { get; set; }
    [Prop] public int Padding { get; set; }
    [Prop] public AutoFlow? AutoFlow { get; set; }

    [Prop(attached: nameof(GridExtensions.GridColumn))] public int?[] ChildColumn { get; set; } = null!;
    [Prop(attached: nameof(GridExtensions.GridColumnSpan))] public int?[] ChildColumnSpan { get; set; } = null!;
    [Prop(attached: nameof(GridExtensions.GridRow))] public int?[] ChildRow { get; set; } = null!;
    [Prop(attached: nameof(GridExtensions.GridRowSpan))] public int?[] ChildRowSpan { get; set; } = null!;
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