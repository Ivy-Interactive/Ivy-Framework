using Ivy.Core;
using Ivy.Shared;

namespace Ivy.Views;

public class TabView : ViewBase
{
    private readonly List<Tab> _tabs = new();
    private Size _width = Shared.Size.Full();
    private Size _height = Shared.Size.Full();
    private TabsVariant _variant = TabsVariant.Content;
    private bool _removeParentPadding = false;
    private Thickness? _padding = new Thickness(4);

    internal TabView(Tab[] cells)
    {
        _tabs.AddRange(cells);
    }

    public TabView Width(Size width)
    {
        _width = width;
        return this;
    }

    public TabView Width(int unit)
    {
        _width = Shared.Size.Units(unit);
        return this;
    }

    public TabView Width(float fraction)
    {
        _width = Shared.Size.Fraction(fraction);
        return this;
    }

    public TabView Width(double fraction)
    {
        _width = Shared.Size.Fraction(Convert.ToSingle(fraction));
        return this;
    }

    public TabView Height(Size height)
    {
        _height = height;
        return this;
    }

    public TabView Height(int unit)
    {
        _height = Shared.Size.Units(unit);
        return this;
    }

    public TabView Height(float fraction)
    {
        _height = Shared.Size.Fraction(fraction);
        return this;
    }

    public TabView Height(double fraction)
    {
        _height = Shared.Size.Fraction(Convert.ToSingle(fraction));
        return this;
    }

    public TabView Size(Size size)
    {
        _width = _height = size;
        return this;
    }

    public TabView Size(int unit)
    {
        _width = Shared.Size.Units(unit);
        _height = Shared.Size.Units(unit);
        return this;
    }

    public TabView Size(float fraction)
    {
        _width = Shared.Size.Fraction(fraction);
        _height = Shared.Size.Fraction(fraction);
        return this;
    }

    public TabView Size(double fraction)
    {
        _width = Shared.Size.Fraction(Convert.ToSingle(fraction));
        _height = Shared.Size.Fraction(Convert.ToSingle(fraction));
        return this;
    }

    public TabView Grow()
    {
        _width = Shared.Size.Grow();
        _height = Shared.Size.Grow();
        return this;
    }

    public TabView Shrink()
    {
        _width = Shared.Size.Shrink();
        _height = Shared.Size.Shrink();
        return this;
    }

    public TabView Variant(TabsVariant variant)
    {
        _variant = variant;
        return this;
    }

    public TabView RemoveParentPadding(bool removeParentPadding = true)
    {
        _removeParentPadding = removeParentPadding;
        return this;
    }

    public TabView Padding(int padding)
    {
        _padding = new Thickness(padding);
        return this;
    }

    public TabView Padding(int horizontal, int vertical)
    {
        _padding = new Thickness(horizontal, vertical);
        return this;
    }

    public TabView Padding(int left, int top, int right, int bottom)
    {
        _padding = new Thickness(left, top, right, bottom);
        return this;
    }

    public override object? Build()
    {
        var selectedIndex = this.UseState(0);

        void OnTabSelect(Event<TabsLayout, int> @event)
        {
            selectedIndex.Set(@event.Value);
        }

        return new TabsLayout(OnTabSelect, null, null, selectedIndex.Value,
            _tabs.ToArray()
        ).Variant(_variant).Width(_width).Height(_height).RemoveParentPadding(_removeParentPadding).Padding(_padding);
    }
}