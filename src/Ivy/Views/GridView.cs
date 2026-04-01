using Ivy.Core;
using Ivy.Core.Hooks;

// ReSharper disable once CheckNamespace
namespace Ivy;

public class GridView : ViewBase, IStateless
{
    private readonly GridDefinition _definition;

    private readonly List<object> _cells = new();
    private string? _testId = null;

    internal GridView(object[] cells)
    {
        _definition = new GridDefinition();
        _cells.AddRange(cells);
    }

    public GridView Columns(int columns)
    {
        _definition.Columns = columns;
        return this;
    }

    public GridView Rows(int rows)
    {
        _definition.Rows = rows;
        return this;
    }

    public GridView Gap(int gap)
    {
        _definition.RowGap = gap;
        _definition.ColumnGap = gap;
        return this;
    }

    public GridView RowGap(int gap)
    {
        _definition.RowGap = gap;
        return this;
    }

    public GridView ColumnGap(int gap)
    {
        _definition.ColumnGap = gap;
        return this;
    }

    public GridView Padding(int padding)
    {
        _definition.Padding = new(padding);
        return this;
    }

    public GridView AlignContent(Align align)
    {
        _definition.AlignContent = align;
        return this;
    }

    public GridView AutoFlow(AutoFlow flow)
    {
        _definition.AutoFlow = flow;
        return this;
    }

    public GridView Width(Size width)
    {
        _definition.Width = width;
        return this;
    }

    public GridView Height(Size height)
    {
        _definition.Height = height;
        return this;
    }

    public GridView ColumnWidths(params Size[] widths)
    {
        _definition.ColumnWidths = widths;
        return this;
    }

    public GridView RowHeights(params Size[] heights)
    {
        _definition.RowHeights = heights;
        return this;
    }

    public GridView HeaderBuilder(Func<int, object, object> builder)
    {
        _definition.HeaderBuilder = builder;
        return this;
    }

    public GridView FooterBuilder(Func<int, object, object> builder)
    {
        _definition.FooterBuilder = builder;
        return this;
    }

    public GridView CellBuilder(Func<object, object> builder)
    {
        _definition.CellBuilder = builder;
        return this;
    }

    public GridView Add(object cell)
    {
        _cells.Add(cell);
        return this;
    }

    public GridView TestId(string testId)
    {
        _testId = testId;
        return this;
    }

    public override object? Build()
    {
        var cells = _cells.ToArray();
        var columnsCount = _definition.Columns ?? 1;

        // Apply builders to transform cells before creating GridLayout
        if (_definition.HeaderBuilder != null || _definition.FooterBuilder != null || _definition.CellBuilder != null)
        {
            var transformedCells = new List<object>(cells);

            // Determine header range (first row)
            int headerStart = 0;
            int headerEnd = Math.Min(columnsCount, transformedCells.Count);

            // Determine footer range (last complete row)
            int footerStart = -1;
            int footerEnd = -1;
            if (transformedCells.Count > columnsCount)
            {
                int lastRowStart = ((transformedCells.Count - 1) / columnsCount) * columnsCount;
                // Only treat as footer if it's a complete row
                if (lastRowStart + columnsCount <= transformedCells.Count)
                {
                    footerStart = lastRowStart;
                    footerEnd = lastRowStart + columnsCount;
                }
            }

            // Apply transformations
            for (int i = 0; i < transformedCells.Count; i++)
            {
                var cell = transformedCells[i];

                // Header
                if (_definition.HeaderBuilder != null && i >= headerStart && i < headerEnd)
                {
                    int columnIndex = i - headerStart;
                    transformedCells[i] = _definition.HeaderBuilder(columnIndex, cell);
                }
                // Footer
                else if (_definition.FooterBuilder != null && footerStart != -1 && i >= footerStart && i < footerEnd)
                {
                    int columnIndex = i - footerStart;
                    transformedCells[i] = _definition.FooterBuilder(columnIndex, cell);
                }
                // Regular cells
                else if (_definition.CellBuilder != null)
                {
                    transformedCells[i] = _definition.CellBuilder(cell);
                }
            }

            cells = transformedCells.ToArray();
        }

        var grid = new GridLayout(_definition, cells);
        if (_testId != null) grid.TestId = _testId;
        return grid;
    }

    public static GridView operator |(GridView view, object child)
    {
        if (child is object[] array)
        {
            foreach (var item in array)
            {
                view.Add(item);
            }
            return view;
        }

        if (child is IEnumerable<object> enumerable)
        {
            foreach (var item in enumerable)
            {
                view.Add(item);
            }
            return view;
        }

        view.Add(child);
        return view;
    }
}
