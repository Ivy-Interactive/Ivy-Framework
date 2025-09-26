using Ivy.Core;
using Ivy.Shared;

namespace Ivy.Views;

/// <summary>
/// Represents a tabbed view that displays multiple tabs with their associated
/// content.
/// </summary>
public class TabView : ViewBase
{
    private readonly List<Tab> _tabs = new();
    private Size _width = Shared.Size.Full();
    private Size _height = Shared.Size.Full();
    private TabsVariant _variant = TabsVariant.Content;
    private bool _removeParentPadding = false;
    private Thickness? _padding = new Thickness(4);

    /// <summary>
    /// Internal constructor that initializes a TabView with predefined tabs.
    /// </summary>
    /// <param name="cells">Array of Tab objects that define the tab structure and content.</param>
    internal TabView(Tab[] cells)
    {
        _tabs.AddRange(cells);
    }

    /// <summary>
    /// Sets the width of the tab view using a Size value.
    /// </summary>
    /// <param name="width">The Size value that determines the tab view's width.</param>
    /// <returns>The current TabView instance for method chaining.</returns>
    public TabView Width(Size width)
    {
        _width = width;
        return this;
    }

    /// <summary>
    /// Sets the width of the tab view in units.
    /// </summary>
    /// <param name="unit">The width in units.</param>
    /// <returns>The current TabView instance for method chaining.</returns>
    public TabView Width(int unit)
    {
        _width = Shared.Size.Units(unit);
        return this;
    }

    /// <summary>
    /// Sets the width of the tab view as a fraction of available space.
    /// </summary>
    /// <param name="fraction">The width as a fraction (0.0 to 1.0).</param>
    /// <returns>The current TabView instance for method chaining.</returns>
    public TabView Width(float fraction)
    {
        _width = Shared.Size.Fraction(fraction);
        return this;
    }

    /// <summary>
    /// Sets the width of the tab view as a fraction of available space.
    /// </summary>
    /// <param name="fraction">The width as a fraction (0.0 to 1.0).</param>
    /// <returns>The current TabView instance for method chaining.</returns>
    public TabView Width(double fraction)
    {
        _width = Shared.Size.Fraction(Convert.ToSingle(fraction));
        return this;
    }

    /// <summary>
    /// Sets the height of the tab view using a Size value.
    /// </summary>
    /// <param name="height">The Size value that determines the tab view's height.</param>
    /// <returns>The current TabView instance for method chaining.</returns>
    public TabView Height(Size height)
    {
        _height = height;
        return this;
    }

    /// <summary>
    /// Sets the height of the tab view in units.
    /// </summary>
    /// <param name="unit">The height in units.</param>
    /// <returns>The current TabView instance for method chaining.</returns>
    public TabView Height(int unit)
    {
        _height = Shared.Size.Units(unit);
        return this;
    }

    /// <summary>
    /// Sets the height of the tab view as a fraction of available space.
    /// </summary>
    /// <param name="fraction">The height as a fraction (0.0 to 1.0).</param>
    /// <returns>The current TabView instance for method chaining.</returns>
    public TabView Height(float fraction)
    {
        _height = Shared.Size.Fraction(fraction);
        return this;
    }

    /// <summary>
    /// Sets the height of the tab view as a fraction of available space.
    /// </summary>
    /// <param name="fraction">The height as a fraction (0.0 to 1.0).</param>
    /// <returns>The current TabView instance for method chaining.</returns>
    public TabView Height(double fraction)
    {
        _height = Shared.Size.Fraction(Convert.ToSingle(fraction));
        return this;
    }

    /// <summary>
    /// Sets both width and height to the same Size value.
    /// </summary>
    /// <param name="size">The Size value for both width and height.</param>
    /// <returns>The current TabView instance for method chaining.</returns>
    public TabView Size(Size size)
    {
        _width = _height = size;
        return this;
    }

    /// <summary>
    /// Sets both width and height to the same unit value.
    /// </summary>
    /// <param name="unit">The unit value for both width and height.</param>
    /// <returns>The current TabView instance for method chaining.</returns>
    public TabView Size(int unit)
    {
        _width = Shared.Size.Units(unit);
        _height = Shared.Size.Units(unit);
        return this;
    }

    /// <summary>
    /// Sets both width and height to the same fraction value.
    /// </summary>
    /// <param name="fraction">The fraction value for both width and height.</param>
    /// <returns>The current TabView instance for method chaining.</returns>
    public TabView Size(float fraction)
    {
        _width = Shared.Size.Fraction(fraction);
        _height = Shared.Size.Fraction(fraction);
        return this;
    }

    /// <summary>
    /// Sets both width and height to the same fraction value.
    /// </summary>
    /// <param name="fraction">The fraction value for both width and height.</param>
    /// <returns>The current TabView instance for method chaining.</returns>
    public TabView Size(double fraction)
    {
        _width = Shared.Size.Fraction(Convert.ToSingle(fraction));
        _height = Shared.Size.Fraction(Convert.ToSingle(fraction));
        return this;
    }

    /// <summary>
    /// Makes the tab view grow to fill available space in both dimensions.
    /// </summary>
    /// <returns>The current TabView instance for method chaining.</returns>
    public TabView Grow()
    {
        _width = Shared.Size.Grow();
        _height = Shared.Size.Grow();
        return this;
    }

    /// <summary>
    /// Makes the tab view shrink to fit its content in both dimensions.
    /// </summary>
    /// <returns>The current TabView instance for method chaining.</returns>
    public TabView Shrink()
    {
        _width = Shared.Size.Shrink();
        _height = Shared.Size.Shrink();
        return this;
    }

    /// <summary>
    /// Sets the display variant for the tabs.
    /// </summary>
    /// <param name="variant">The TabsVariant value that determines how tabs are displayed.</param>
    /// <returns>The current TabView instance for method chaining.</returns>
    public TabView Variant(TabsVariant variant)
    {
        _variant = variant;
        return this;
    }

    /// <summary>
    /// Controls whether parent padding should be removed from the tab view.
    /// </summary>
    /// <param name="removeParentPadding">True to remove parent padding, false to keep it (defaults to true).</param>
    /// <returns>The current TabView instance for method chaining.</returns>
    public TabView RemoveParentPadding(bool removeParentPadding = true)
    {
        _removeParentPadding = removeParentPadding;
        return this;
    }

    /// <summary>
    /// Sets uniform padding around the tab view content.
    /// </summary>
    /// <param name="padding">The uniform padding size in pixels.</param>
    /// <returns>The current TabView instance for method chaining.</returns>
    public TabView Padding(int padding)
    {
        _padding = new Thickness(padding);
        return this;
    }

    /// <summary>
    /// Sets horizontal and vertical padding around the tab view content.
    /// </summary>
    /// <param name="horizontal">The horizontal padding size in pixels.</param>
    /// <param name="vertical">The vertical padding size in pixels.</param>
    /// <returns>The current TabView instance for method chaining.</returns>
    public TabView Padding(int horizontal, int vertical)
    {
        _padding = new Thickness(horizontal, vertical);
        return this;
    }

    /// <summary>
    /// Sets specific padding values for each side of the tab view content.
    /// </summary>
    /// <param name="left">The left padding size in pixels.</param>
    /// <param name="top">The top padding size in pixels.</param>
    /// <param name="right">The right padding size in pixels.</param>
    /// <param name="bottom">The bottom padding size in pixels.</param>
    /// <returns>The current TabView instance for method chaining.</returns>
    public TabView Padding(int left, int top, int right, int bottom)
    {
        _padding = new Thickness(left, top, right, bottom);
        return this;
    }

    /// <summary>
    /// Builds the final tabbed layout widget with tab selection state management.
    /// </summary>
    /// <returns>A TabsLayout widget configured with the current settings and tab selection state.</returns>
    public override object? Build()
    {
        var selectedIndex = this.UseState(0);

        void OnTabSelect(Event<TabsLayout, int> @event)
        {
            selectedIndex.Set(@event.Value);
        }

        return new TabsLayout(OnTabSelect, null, null, null, selectedIndex.Value,
            _tabs.ToArray()
        ).Variant(_variant).Width(_width).Height(_height).RemoveParentPadding(_removeParentPadding).Padding(_padding);
    }
}