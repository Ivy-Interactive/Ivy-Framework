using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Shared;

namespace Ivy.Views;

/// <summary>
/// Represents a grid view that provides a fluent API for creating and
/// configuring grid layouts with dynamic cell management.
/// </summary>
public class GridView : ViewBase, IStateless
{
    private readonly GridDefinition _definition;

    private readonly List<object> _cells = new();

    /// <summary>
    /// Internal constructor that initializes a GridView with predefined cells.
    /// </summary>
    /// <param name="cells">Array of objects to use as initial grid cells.</param>
    internal GridView(object[] cells)
    {
        _definition = new GridDefinition();
        _cells.AddRange(cells);
    }

    /// <summary>
    /// Sets the number of columns for the grid layout.
    /// </summary>
    /// <param name="columns">The number of columns to create in the grid layout.</param>
    /// <returns>The current GridView instance for method chaining.</returns>
    public GridView Columns(int columns)
    {
        _definition.Columns = columns;
        return this;
    }

    /// <summary>
    /// Sets the number of rows for the grid layout.
    /// </summary>
    /// <param name="rows">The number of rows to create in the grid layout.</param>
    /// <returns>The current GridView instance for method chaining.</returns>
    public GridView Rows(int rows)
    {
        _definition.Rows = rows;
        return this;
    }

    /// <summary>
    /// Sets the gap between grid cells.
    /// </summary>
    /// <param name="gap">The gap size in pixels between grid cells.</param>
    /// <returns>The current GridView instance for method chaining.</returns>
    public GridView Gap(int gap)
    {
        _definition.Gap = gap;
        return this;
    }

    /// <summary>
    /// Sets the padding around the entire grid layout.
    /// </summary>
    /// <param name="padding">The padding size in pixels around the grid layout.</param>
    /// <returns>The current GridView instance for method chaining.</returns>
    public GridView Padding(int padding)
    {
        _definition.Padding = padding;
        return this;
    }

    /// <summary>
    /// Sets the auto-flow behavior for grid items.
    /// </summary>
    /// <param name="flow">The AutoFlow value that determines item placement behavior.</param>
    /// <returns>The current GridView instance for method chaining.</returns>
    public GridView AutoFlow(AutoFlow flow)
    {
        _definition.AutoFlow = flow;
        return this;
    }

    /// <summary>
    /// Sets the width of the grid layout.
    /// </summary>
    /// <param name="width">The Size value that determines the grid's width.</param>
    /// <returns>The current GridView instance for method chaining.</returns>
    public GridView Width(Size width)
    {
        _definition.Width = width;
        return this;
    }

    /// <summary>
    /// Adds a single cell to the grid layout.
    /// </summary>
    /// <param name="cell">The object to add as a new grid cell.</param>
    /// <returns>The current GridView instance for method chaining.</returns>
    public GridView Add(object cell)
    {
        _cells.Add(cell);
        return this;
    }

    /// <summary>
    /// Builds the final grid layout using the configured definition
    /// and collected cells, creating a GridLayout widget for display.
    /// </summary>
    /// <returns>A GridLayout widget configured with the grid definition and cells.</returns>
    public override object? Build()
    {
        return new GridLayout(_definition, _cells.ToArray());
    }

    /// <summary>
    /// Operator overload that allows adding content to the grid using the pipe operator.
    /// </summary>
    /// <param name="view">The GridView to add content to.</param>
    /// <param name="child">The content to add, which can be a single object, array, or enumerable.</param>
    /// <returns>The updated GridView instance with the new content added.</returns>
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