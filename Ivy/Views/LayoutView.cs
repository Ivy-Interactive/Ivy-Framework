using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Shared;

namespace Ivy.Views;

public class LayoutView : ViewBase, IStateless
{
    private enum LayoutType
    {
        Stack,
        Wrap
    }

    private readonly List<LayoutElement> _elements = new();
    private Orientation _orientation = Orientation.Vertical;
    private LayoutType _layoutType = LayoutType.Stack;

    private class LayoutElement(object content)
    {
        public object Content { get; set; } = content;
    }

    internal LayoutView()
    {
    }

    private int _gap = 4;
    private Thickness? _padding = null;
    private Thickness? _margin = null;
    private Size? _width = null;
    private Size? _height = null;
    private Colors? _background = null;
    private Align? _alignment = null;
    private Scroll _scroll = Shared.Scroll.None;
    private bool _removeParentPadding = false;
    private bool _visible = true;

    public LayoutView Gap(bool gap)
    {
        _gap = gap ? 4 : 0;
        return this;
    }

    public LayoutView Gap(int gap)
    {
        _gap = gap;
        return this;
    }

    public LayoutView Visible(bool visible)
    {
        _visible = visible;
        return this;
    }

    public LayoutView Width(Size width)
    {
        _width = width;
        return this;
    }

    public LayoutView Grow()
    {
        if (_orientation == Orientation.Vertical)
        {
            _height = Shared.Size.Grow();
        }
        else
        {
            _width = Shared.Size.Grow();
        }
        return this;
    }

    public LayoutView Shrink()
    {
        if (_orientation == Orientation.Vertical)
        {
            _height = Shared.Size.Shrink();
        }
        else
        {
            _width = Shared.Size.Shrink();
        }
        return this;
    }

    public LayoutView Width(int unit)
    {
        _width = Shared.Size.Units(unit);
        return this;
    }

    public LayoutView Width(float fraction)
    {
        _width = Shared.Size.Fraction(fraction);
        return this;
    }

    public LayoutView Width(double fraction)
    {
        _width = Shared.Size.Fraction(Convert.ToSingle(fraction));
        return this;
    }

    public LayoutView Height(Size height)
    {
        _height = height;
        return this;
    }

    public LayoutView Height(int unit)
    {
        _height = Shared.Size.Units(unit);
        return this;
    }

    public LayoutView Height(float fraction)
    {
        _height = Shared.Size.Fraction(fraction);
        return this;
    }

    public LayoutView Height(double fraction)
    {
        _height = Shared.Size.Fraction(Convert.ToSingle(fraction));
        return this;
    }

    public LayoutView Size(Size height)
    {
        _width = height;
        _height = height;
        return this;
    }

    public LayoutView Size(int unit)
    {
        _width = Shared.Size.Units(unit);
        _height = Shared.Size.Units(unit);
        return this;
    }

    public LayoutView Size(float fraction)
    {
        _width = Shared.Size.Fraction(fraction);
        _height = Shared.Size.Fraction(fraction);
        return this;
    }

    public LayoutView Size(double fraction)
    {
        _width = Shared.Size.Fraction(Convert.ToSingle(fraction));
        _height = Shared.Size.Fraction(Convert.ToSingle(fraction));
        return this;
    }

    public LayoutView Padding(int padding)
    {
        _padding = new Thickness(padding);
        return this;
    }

    public LayoutView Padding(int horizontal, int vertical)
    {
        _padding = new Thickness(horizontal, vertical);
        return this;
    }

    public LayoutView Padding(int left, int top, int right, int bottom)
    {
        _padding = new Thickness(left, top, right, bottom);
        return this;
    }

    public LayoutView P(int padding)
    {
        _padding = new Thickness(padding);
        return this;
    }

    public LayoutView P(int horizontal, int vertical)
    {
        _padding = new Thickness(horizontal, vertical);
        return this;
    }

    public LayoutView P(int left, int top, int right, int bottom)
    {
        _padding = new Thickness(left, top, right, bottom);
        return this;
    }

    public LayoutView Margin(int margin)
    {
        _margin = new Thickness(margin);
        return this;
    }

    public LayoutView Margin(int horizontal, int vertical)
    {
        _margin = new Thickness(horizontal, vertical);
        return this;
    }

    public LayoutView Margin(int left, int top, int right, int bottom)
    {
        _margin = new Thickness(left, top, right, bottom);
        return this;
    }

    public LayoutView Background(Colors color)
    {
        _background = color;
        return this;
    }

    public LayoutView Align(Align align)
    {
        _alignment = align;
        return this;
    }

    public LayoutView Center()
    {
        _alignment = Shared.Align.Center;
        return this;
    }

    public LayoutView Left()
    {
        _alignment = Shared.Align.Left;
        return this;
    }

    public LayoutView Right()
    {
        _alignment = Shared.Align.Right;
        return this;
    }

    public LayoutView Add(object content)
    {
        _elements.Add(new LayoutElement(content));
        return this;
    }

    public LayoutView Add(object[] content, string space = "auto")
    {
        _elements.AddRange(content.Select(e => new LayoutElement(e)));
        return this;
    }

    public LayoutView Add(IEnumerable<object> content, string space = "auto")
    {
        _elements.AddRange(content.Select(e => new LayoutElement(e)));
        return this;
    }

    public LayoutView Vertical(params object[] elements)
    {
        _layoutType = LayoutType.Stack;
        _orientation = Orientation.Vertical;
        Add(elements);
        return this;
    }

    public LayoutView Vertical(IEnumerable<object> elements)
    {
        return Vertical(elements.ToArray());
    }

    public LayoutView Horizontal(params object[] elements)
    {
        _layoutType = LayoutType.Stack;
        _orientation = Orientation.Horizontal;
        Add(elements);
        return this;
    }

    public LayoutView Horizontal(IEnumerable<object> elements)
    {
        return Horizontal(elements.ToArray());
    }

    public LayoutView Wrap(params object[] elements)
    {
        _layoutType = LayoutType.Wrap;
        Add(elements);
        return this;
    }

    public LayoutView Wrap(IEnumerable<object> elements)
    {
        return Wrap(elements.ToArray());
    }

    public LayoutView Scroll(Scroll scroll = Shared.Scroll.Auto)
    {
        _scroll = scroll;
        return this;
    }

    public LayoutView RemoveParentPadding()
    {
        _removeParentPadding = true;
        return this;
    }

    public override object? Build()
    {
        if (_layoutType == LayoutType.Wrap)
        {
            return new WrapLayout(_elements.Select(e => e.Content).ToArray(), _gap, _padding, _margin,
                    _background, _alignment, _removeParentPadding)
                .Width(_width)
                .Height(_height)
                .Visible(_visible);
        }

        return new StackLayout(_elements.Select(e => e.Content).ToArray(), _orientation, _gap, _padding, _margin, _background,
                _alignment, _removeParentPadding)
            .Width(_width)
            .Height(_height)
            .Visible(_visible);
    }

    public static LayoutView operator |(LayoutView view, object? child)
    {
        switch (child)
        {
            case null:
                return view;
            case object[] array:
                view.Add(array);
                return view;
            case IEnumerable<object> enumerable:
                view.Add(enumerable);
                return view;
            default:
                view.Add(child);
                return view;
        }
    }
}