using Ivy.Core;
using Ivy.Core.Hooks;

// ReSharper disable once CheckNamespace
namespace Ivy;

public class LayoutView : ViewBase, IStateless
{
    private readonly List<LayoutElement> _elements = new();
    private Orientation _orientation = Orientation.Vertical;
    private bool _wrap = false;

    private class LayoutElement(object content)
    {
        public object Content { get; set; } = content;
    }

    internal LayoutView()
    {
    }

    private int _rowGap = 4;
    private int _columnGap = 4;
    private Thickness? _padding = null;
    private Thickness? _margin = null;
    private Size? _width = null;
    private Size? _height = null;
    private Colors? _background = null;
    private Align? _alignment = null;
    private Scroll _scroll = Ivy.Scroll.None;
    private bool _removeParentPadding = false;
    private Colors? _borderColor = null;
    private Ivy.BorderRadius _borderRadius = Ivy.BorderRadius.None;
    private Ivy.BorderStyle _borderStyle = Ivy.BorderStyle.None;
    private Thickness _borderThickness = new(0);
    private string? _testId = null;
    private GridView? _activeGrid = null;

    public LayoutView Gap(bool gap)
    {
        _rowGap = gap ? 4 : 0;
        _columnGap = gap ? 4 : 0;
        return this;
    }

    public LayoutView Gap(int gap)
    {
        _rowGap = gap;
        _columnGap = gap;
        return this;
    }

    public LayoutView Gap(int rowGap, int columnGap)
    {
        _rowGap = rowGap;
        _columnGap = columnGap;
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
            _height = Ivy.Size.Grow();
        }
        else
        {
            _width = Ivy.Size.Grow();
        }
        return this;
    }

    public LayoutView Shrink()
    {
        if (_orientation == Orientation.Vertical)
        {
            _height = Ivy.Size.Shrink();
        }
        else
        {
            _width = Ivy.Size.Shrink();
        }
        return this;
    }

    public LayoutView Width(int unit)
    {
        _width = Ivy.Size.Units(unit);
        return this;
    }

    public LayoutView Width(float fraction)
    {
        _width = Ivy.Size.Fraction(fraction);
        return this;
    }

    public LayoutView Width(double fraction)
    {
        _width = Ivy.Size.Fraction(Convert.ToSingle(fraction));
        return this;
    }

    public LayoutView Height(Size height)
    {
        _height = height;
        return this;
    }

    public LayoutView Height(int unit)
    {
        _height = Ivy.Size.Units(unit);
        return this;
    }

    public LayoutView Height(float fraction)
    {
        _height = Ivy.Size.Fraction(fraction);
        return this;
    }

    public LayoutView Height(double fraction)
    {
        _height = Ivy.Size.Fraction(Convert.ToSingle(fraction));
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
        _width = Ivy.Size.Units(unit);
        _height = Ivy.Size.Units(unit);
        return this;
    }

    public LayoutView Size(float fraction)
    {
        _width = Ivy.Size.Fraction(fraction);
        _height = Ivy.Size.Fraction(fraction);
        return this;
    }

    public LayoutView Size(double fraction)
    {
        _width = Ivy.Size.Fraction(Convert.ToSingle(fraction));
        _height = Ivy.Size.Fraction(Convert.ToSingle(fraction));
        return this;
    }

    public LayoutView Full()
    {
        _width = Ivy.Size.Full();
        _height = Ivy.Size.Full();
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

    public LayoutView TopMargin(int margin)
    {
        _margin = new Thickness(_margin?.Left ?? 0, margin, _margin?.Right ?? 0, _margin?.Bottom ?? 0);
        return this;
    }

    public LayoutView BottomMargin(int margin)
    {
        _margin = new Thickness(_margin?.Left ?? 0, _margin?.Top ?? 0, _margin?.Right ?? 0, margin);
        return this;
    }

    public LayoutView LeftMargin(int margin)
    {
        _margin = new Thickness(margin, _margin?.Top ?? 0, _margin?.Right ?? 0, _margin?.Bottom ?? 0);
        return this;
    }

    public LayoutView RightMargin(int margin)
    {
        _margin = new Thickness(_margin?.Left ?? 0, _margin?.Top ?? 0, margin, _margin?.Bottom ?? 0);
        return this;
    }

    public LayoutView Background(Colors color)
    {
        _background = color;
        return this;
    }

    public LayoutView Border(Colors color, int thickness = 1)
    {
        _borderColor = color;
        _borderThickness = new(thickness);
        _borderStyle = Ivy.BorderStyle.Solid;
        _borderRadius = Ivy.BorderRadius.Rounded;
        return this;
    }

    public LayoutView Border(Colors color, Thickness thickness)
    {
        _borderColor = color;
        _borderThickness = thickness;
        _borderStyle = Ivy.BorderStyle.Solid;
        _borderRadius = Ivy.BorderRadius.Rounded;
        return this;
    }

    public LayoutView BorderColor(Colors color)
    {
        _borderColor = color;
        return this;
    }

    public LayoutView BorderRadius(Ivy.BorderRadius radius)
    {
        _borderRadius = radius;
        return this;
    }

    public LayoutView BorderStyle(Ivy.BorderStyle style)
    {
        _borderStyle = style;
        return this;
    }

    public LayoutView BorderThickness(int thickness)
    {
        _borderThickness = new(thickness);
        return this;
    }

    public LayoutView BorderThickness(Thickness thickness)
    {
        _borderThickness = thickness;
        return this;
    }

    public LayoutView AlignContent(Align align)
    {
        _alignment = align;
        return this;
    }

    public LayoutView Center()
    {
        _alignment = Ivy.Align.Center;
        return this;
    }

    public LayoutView Left()
    {
        _alignment = Ivy.Align.Left;
        return this;
    }

    public LayoutView Right()
    {
        _alignment = Ivy.Align.Right;
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
        _wrap = false;
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
        _wrap = false;
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
        _wrap = true;
        _orientation = Orientation.Horizontal;
        Add(elements);
        return this;
    }

    public LayoutView Wrap(IEnumerable<object> elements)
    {
        return Wrap(elements.ToArray());
    }

    public LayoutView Wrap(Orientation orientation, params object[] elements)
    {
        _wrap = true;
        _orientation = orientation;
        Add(elements);
        return this;
    }

    public LayoutView Wrap(Orientation orientation, IEnumerable<object> elements)
    {
        return Wrap(orientation, elements.ToArray());
    }

    public LayoutView Scroll(Scroll scroll = Ivy.Scroll.Auto)
    {
        _scroll = scroll;
        return this;
    }

    public LayoutView RemoveParentPadding()
    {
        _removeParentPadding = true;
        return this;
    }

    public LayoutView TestId(string testId)
    {
        _testId = testId;
        return this;
    }

    public override object? Build()
    {
        var layout = new StackLayout(_elements.Select(e => e.Content).ToArray(), _orientation, _rowGap, _padding, _margin, _background,
                _alignment, _removeParentPadding, _wrap)
        {
            ColumnGap = _columnGap,
            Scroll = _scroll,
            BorderColor = _borderColor,
            BorderRadius = _borderRadius,
            BorderStyle = _borderStyle,
            BorderThickness = _borderThickness
        }
            .Width(_width)
            .Height(_height);

        if (_testId != null) layout.TestId = _testId;

        return layout;
    }

    public static LayoutView operator |(LayoutView view, GridView child)
    {
        view.Add(child);
        view._activeGrid = child;
        return view;
    }

    public static LayoutView operator |(LayoutView view, object? child)
    {
        switch (child)
        {
            case null:
                view._activeGrid = null;
                return view;
            case object[] array:
                view._activeGrid = null;
                view.Add(array);
                return view;
            case IEnumerable<object> enumerable:
                view._activeGrid = null;
                view.Add(enumerable);
                return view;
            default:
                if (view._activeGrid != null && child is not ViewBase)
                {
                    view._activeGrid.Add(child);
                    return view;
                }
                view._activeGrid = null;
                view.Add(child);
                return view;
        }
    }
}
