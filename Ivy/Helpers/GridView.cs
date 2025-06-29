using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Shared;

namespace Ivy.Helpers;

public class GridView : ViewBase, IStateless
{
    private readonly GridDefinition _definition;

    private readonly List<object> _cells = new();

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
        _definition.Gap = gap;
        return this;
    }

    public GridView Padding(int padding)
    {
        _definition.Padding = padding;
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

    public GridView Add(object cell)
    {
        _cells.Add(cell);
        return this;
    }

    public override object? Build()
    {
        return new GridLayout(_definition, _cells.ToArray());
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