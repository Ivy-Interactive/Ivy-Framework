using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Defines how grid items are automatically placed within the grid layout when no explicit positioning is specified.
/// </summary>
public enum AutoFlow
{
    /// <summary>Fill each row completely before moving to the next row. This is the default behavior.</summary>
    Row,
    /// <summary>Fill each column completely before moving to the next column.</summary>
    Column,
    /// <summary>Fill rows sequentially, but try to fill gaps with later items for more efficient space usage.</summary>
    RowDense,
    /// <summary>Fill columns sequentially, but try to fill gaps with later items for more efficient space usage.</summary>
    ColumnDense
}

/// <summary>
/// Defines the configuration for a grid layout, including dimensions, spacing, and behavior.
/// </summary>
public class GridDefinition
{
    /// <summary>
    /// Gets or sets the number of columns in the grid.
    /// </summary>
    public int? Columns { get; set; }

    /// <summary>
    /// Gets or sets the number of rows in the grid.
    /// </summary>
    public int? Rows { get; set; }

    /// <summary>
    /// Gets or sets the space between grid items in pixels.
    /// </summary>
    public int Gap { get; set; } = 4;

    /// <summary>
    /// Gets or sets the padding around the entire grid container in pixels.
    /// </summary>
    public int Padding { get; set; } = 0;

    /// <summary>
    /// Gets or sets how grid items are automatically placed when no explicit positioning is specified.
    /// </summary>
    public AutoFlow? AutoFlow { get; set; } = null;

    /// <summary>
    /// Gets or sets the width of the grid container.
    /// </summary>
    public Size? Width { get; set; } = null;

    /// <summary>
    /// Gets or sets the height of the grid container.
    /// </summary>
    public Size? Height { get; set; } = null;
}

/// <summary>
/// Represents a two-dimensional grid layout widget that arranges child elements in rows and columns with precise control over positioning, spacing, and spanning.
/// </summary>
public record GridLayout : WidgetBase<GridLayout>
{
    /// <summary>
    /// Initializes a new instance of the GridLayout class with the specified grid definition and children.
    /// </summary>
    /// <param name="def">The GridDefinition object containing all grid configuration settings including
    /// columns, rows, gap, padding, auto-flow, and dimensions.</param>
    /// <param name="children">Variable number of child elements to arrange in the grid layout.</param>
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

    /// <summary>
    /// Gets or sets the number of columns in the grid.
    /// </summary>
    [Prop] public int? Columns { get; set; }

    /// <summary>
    /// Gets or sets the number of rows in the grid.
    /// </summary>
    [Prop] public int? Rows { get; set; }

    /// <summary>
    /// Gets or sets the space between grid items in pixels.
    /// </summary>
    [Prop] public int Gap { get; set; }

    /// <summary>
    /// Gets or sets the padding around the entire grid container in pixels.
    /// </summary>
    [Prop] public int Padding { get; set; }

    /// <summary>
    /// Gets or sets how grid items are automatically placed when no explicit positioning is specified.
    /// </summary>
    [Prop] public AutoFlow? AutoFlow { get; set; }

    /// <summary>
    /// Gets or sets the column positions for child elements in the grid.
    /// </summary>
    [Prop(attached: nameof(GridExtensions.GridColumn))] public int?[] ChildColumn { get; set; } = null!;

    /// <summary>
    /// Gets or sets the column span values for child elements in the grid.
    /// </summary>
    [Prop(attached: nameof(GridExtensions.GridColumnSpan))] public int?[] ChildColumnSpan { get; set; } = null!;

    /// <summary>
    /// Gets or sets the row positions for child elements in the grid.
    /// </summary>
    [Prop(attached: nameof(GridExtensions.GridRow))] public int?[] ChildRow { get; set; } = null!;

    /// <summary>
    /// Gets or sets the row span values for child elements in the grid.
    /// </summary>
    [Prop(attached: nameof(GridExtensions.GridRowSpan))] public int?[] ChildRowSpan { get; set; } = null!;
}

/// <summary>
/// Provides extension methods for positioning child elements within grid layouts.
/// </summary>
public static class GridExtensions
{
    /// <summary>
    /// Sets the specific column position for a child element in the grid.
    /// </summary>
    /// <typeparam name="T">The type of the widget being positioned.</typeparam>
    /// <param name="child">The child widget to position in the grid.</param>
    /// <param name="column">The column index where the child should be placed (1-based indexing).</param>
    /// <returns>The child widget with the grid column position set.</returns>
    public static WidgetBase<T> GridColumn<T>(this WidgetBase<T> child, int column) where T : WidgetBase<T>
    {
        child.SetAttachedValue(typeof(GridLayout), nameof(GridColumn), column);
        return child;
    }

    /// <summary>
    /// Sets how many columns a child element should span in the grid.
    /// </summary>
    /// <typeparam name="T">The type of the widget being positioned.</typeparam>
    /// <param name="child">The child widget to configure for column spanning.</param>
    /// <param name="columnSpan">The number of columns the child should span across.</param>
    /// <returns>The child widget with the grid column span set.</returns>
    public static WidgetBase<T> GridColumnSpan<T>(this WidgetBase<T> child, int columnSpan) where T : WidgetBase<T>
    {
        child.SetAttachedValue(typeof(GridLayout), nameof(GridColumnSpan), columnSpan);
        return child;
    }

    /// <summary>
    /// Sets the specific row position for a child element in the grid.
    /// </summary>
    /// <typeparam name="T">The type of the widget being positioned.</typeparam>
    /// <param name="child">The child widget to position in the grid.</param>
    /// <param name="row">The row index where the child should be placed (1-based indexing).</param>
    /// <returns>The child widget with the grid row position set.</returns>
    public static WidgetBase<T> GridRow<T>(this WidgetBase<T> child, int row) where T : WidgetBase<T>
    {
        child.SetAttachedValue(typeof(GridLayout), nameof(GridRow), row);
        return child;
    }

    /// <summary>
    /// Sets how many rows a child element should span in the grid.
    /// </summary>
    /// <typeparam name="T">The type of the widget being positioned.</typeparam>
    /// <param name="child">The child widget to configure for row spanning.</param>
    /// <param name="rowSpan">The number of rows the child should span across.</param>
    /// <returns>The child widget with the grid row span set.</returns>
    public static WidgetBase<T> GridRowSpan<T>(this WidgetBase<T> child, int rowSpan) where T : WidgetBase<T>
    {
        child.SetAttachedValue(typeof(GridLayout), nameof(GridRowSpan), rowSpan);
        return child;
    }
}