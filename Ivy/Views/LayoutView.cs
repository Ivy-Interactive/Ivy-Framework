using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Shared;

namespace Ivy.Views;

/// <summary>
/// Represents a flexible layout view that supports both stack and wrap layouts
/// with comprehensive configuration options for spacing, sizing, alignment,
/// and content arrangement.
/// </summary>
public class LayoutView : ViewBase, IStateless
{
    /// <summary>
    /// Defines the type of layout to use for arranging elements.
    /// </summary>
    private enum LayoutType
    {
        /// <summary>
        /// Elements are arranged in a linear stack (horizontal or vertical).
        /// </summary>
        Stack,
        /// <summary>
        /// Elements wrap to new lines when they reach the container's edge.
        /// </summary>
        Wrap
    }

    private readonly List<LayoutElement> _elements = new();
    private Orientation _orientation = Orientation.Vertical;
    private LayoutType _layoutType = LayoutType.Stack;

    /// <summary>
    /// Internal class that wraps content objects for layout management.
    /// </summary>
    private class LayoutElement(object content)
    {
        /// <summary>
        /// The content object to be displayed in the layout.
        /// </summary>
        public object Content { get; set; } = content;
    }

    /// <summary>
    /// Internal constructor that initializes a new LayoutView instance.
    /// </summary>
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

    /// <summary>
    /// Sets the gap between layout elements using a boolean value.
    /// </summary>
    /// <param name="gap">True to set gap to 4 pixels, false to set gap to 0.</param>
    /// <returns>The current LayoutView instance for method chaining.</returns>
    public LayoutView Gap(bool gap)
    {
        _gap = gap ? 4 : 0;
        return this;
    }

    /// <summary>
    /// Sets the gap between layout elements in pixels.
    /// </summary>
    /// <param name="gap">The gap size in pixels between layout elements.</param>
    /// <returns>The current LayoutView instance for method chaining.</returns>
    public LayoutView Gap(int gap)
    {
        _gap = gap;
        return this;
    }

    /// <summary>
    /// Sets the visibility of the layout view.
    /// </summary>
    /// <param name="visible">True to make the layout visible, false to hide it.</param>
    /// <returns>The current LayoutView instance for method chaining.</returns>
    public LayoutView Visible(bool visible)
    {
        _visible = visible;
        return this;
    }

    /// <summary>
    /// Sets the width of the layout view using a Size value.
    /// </summary>
    /// <param name="width">The Size value that determines the layout's width.</param>
    /// <returns>The current LayoutView instance for method chaining.</returns>
    public LayoutView Width(Size width)
    {
        _width = width;
        return this;
    }

    /// <summary>
    /// Makes the layout grow to fill available space in the direction of its orientation.
    /// </summary>
    /// <returns>The current LayoutView instance for method chaining.</returns>
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

    /// <summary>
    /// Makes the layout shrink to fit its content in the direction of its orientation.
    /// </summary>
    /// <returns>The current LayoutView instance for method chaining.</returns>
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

    /// <summary>
    /// Sets the width of the layout view in units.
    /// </summary>
    /// <param name="unit">The width in units.</param>
    /// <returns>The current LayoutView instance for method chaining.</returns>
    public LayoutView Width(int unit)
    {
        _width = Shared.Size.Units(unit);
        return this;
    }

    /// <summary>
    /// Sets the width of the layout view as a fraction of available space.
    /// </summary>
    /// <param name="fraction">The width as a fraction (0.0 to 1.0).</param>
    /// <returns>The current LayoutView instance for method chaining.</returns>
    public LayoutView Width(float fraction)
    {
        _width = Shared.Size.Fraction(fraction);
        return this;
    }

    /// <summary>
    /// Sets the width of the layout view as a fraction of available space.
    /// </summary>
    /// <param name="fraction">The width as a fraction (0.0 to 1.0).</param>
    /// <returns>The current LayoutView instance for method chaining.</returns>
    public LayoutView Width(double fraction)
    {
        _width = Shared.Size.Fraction(Convert.ToSingle(fraction));
        return this;
    }

    /// <summary>
    /// Sets the height of the layout view using a Size value.
    /// </summary>
    /// <param name="height">The Size value that determines the layout's height.</param>
    /// <returns>The current LayoutView instance for method chaining.</returns>
    public LayoutView Height(Size height)
    {
        _height = height;
        return this;
    }

    /// <summary>
    /// Sets the height of the layout view in units.
    /// </summary>
    /// <param name="unit">The height in units.</param>
    /// <returns>The current LayoutView instance for method chaining.</returns>
    public LayoutView Height(int unit)
    {
        _height = Shared.Size.Units(unit);
        return this;
    }

    /// <summary>
    /// Sets the height of the layout view as a fraction of available space.
    /// </summary>
    /// <param name="fraction">The height as a fraction (0.0 to 1.0).</param>
    /// <returns>The current LayoutView instance for method chaining.</returns>
    public LayoutView Height(float fraction)
    {
        _height = Shared.Size.Fraction(fraction);
        return this;
    }

    /// <summary>
    /// Sets the height of the layout view as a fraction of available space.
    /// </summary>
    /// <param name="fraction">The height as a fraction (0.0 to 1.0).</param>
    /// <returns>The current LayoutView instance for method chaining.</returns>
    public LayoutView Height(double fraction)
    {
        _height = Shared.Size.Fraction(Convert.ToSingle(fraction));
        return this;
    }

    /// <summary>
    /// Sets both width and height to the same Size value.
    /// </summary>
    /// <param name="height">The Size value for both width and height.</param>
    /// <returns>The current LayoutView instance for method chaining.</returns>
    public LayoutView Size(Size height)
    {
        _width = height;
        _height = height;
        return this;
    }

    /// <summary>
    /// Sets both width and height to the same unit value.
    /// </summary>
    /// <param name="unit">The unit value for both width and height.</param>
    /// <returns>The current LayoutView instance for method chaining.</returns>
    public LayoutView Size(int unit)
    {
        _width = Shared.Size.Units(unit);
        _height = Shared.Size.Units(unit);
        return this;
    }

    /// <summary>
    /// Sets both width and height to the same fraction value.
    /// </summary>
    /// <param name="fraction">The fraction value for both width and height.</param>
    /// <returns>The current LayoutView instance for method chaining.</returns>
    public LayoutView Size(float fraction)
    {
        _width = Shared.Size.Fraction(fraction);
        _height = Shared.Size.Fraction(fraction);
        return this;
    }

    /// <summary>
    /// Sets both width and height to the same fraction value.
    /// </summary>
    /// <param name="fraction">The fraction value for both width and height.</param>
    /// <returns>The current LayoutView instance for method chaining.</returns>
    public LayoutView Size(double fraction)
    {
        _width = Shared.Size.Fraction(Convert.ToSingle(fraction));
        _height = Shared.Size.Fraction(Convert.ToSingle(fraction));
        return this;
    }

    /// <summary>
    /// Sets uniform padding around the layout content.
    /// </summary>
    /// <param name="padding">The uniform padding size in pixels.</param>
    /// <returns>The current LayoutView instance for method chaining.</returns>
    public LayoutView Padding(int padding)
    {
        _padding = new Thickness(padding);
        return this;
    }

    /// <summary>
    /// Sets horizontal and vertical padding around the layout content.
    /// </summary>
    /// <param name="horizontal">The horizontal padding size in pixels.</param>
    /// <param name="vertical">The vertical padding size in pixels.</param>
    /// <returns>The current LayoutView instance for method chaining.</returns>
    public LayoutView Padding(int horizontal, int vertical)
    {
        _padding = new Thickness(horizontal, vertical);
        return this;
    }

    /// <summary>
    /// Sets specific padding values for each side of the layout content.
    /// </summary>
    /// <param name="left">The left padding size in pixels.</param>
    /// <param name="top">The top padding size in pixels.</param>
    /// <param name="right">The right padding size in pixels.</param>
    /// <param name="bottom">The bottom padding size in pixels.</param>
    /// <returns>The current LayoutView instance for method chaining.</returns>
    public LayoutView Padding(int left, int top, int right, int bottom)
    {
        _padding = new Thickness(left, top, right, bottom);
        return this;
    }

    /// <summary>
    /// Shortcut method for setting uniform padding around the layout content.
    /// </summary>
    /// <param name="padding">The uniform padding size in pixels.</param>
    /// <returns>The current LayoutView instance for method chaining.</returns>
    public LayoutView P(int padding)
    {
        _padding = new Thickness(padding);
        return this;
    }

    /// <summary>
    /// Shortcut method for setting horizontal and vertical padding around the layout content.
    /// </summary>
    /// <param name="horizontal">The horizontal padding size in pixels.</param>
    /// <param name="vertical">The vertical padding size in pixels.</param>
    /// <returns>The current LayoutView instance for method chaining.</returns>
    public LayoutView P(int horizontal, int vertical)
    {
        _padding = new Thickness(horizontal, vertical);
        return this;
    }

    /// <summary>
    /// Shortcut method for setting specific padding values for each side of the layout content.
    /// </summary>
    /// <param name="left">The left padding size in pixels.</param>
    /// <param name="top">The top padding size in pixels.</param>
    /// <param name="right">The right padding size in pixels.</param>
    /// <param name="bottom">The bottom padding size in pixels.</param>
    /// <returns>The current LayoutView instance for method chaining.</returns>
    public LayoutView P(int left, int top, int right, int bottom)
    {
        _padding = new Thickness(left, top, right, bottom);
        return this;
    }

    /// <summary>
    /// Sets uniform margin around the layout view.
    /// </summary>
    /// <param name="margin">The uniform margin size in pixels.</param>
    /// <returns>The current LayoutView instance for method chaining.</returns>
    public LayoutView Margin(int margin)
    {
        _margin = new Thickness(margin);
        return this;
    }

    /// <summary>
    /// Sets horizontal and vertical margin around the layout view.
    /// </summary>
    /// <param name="horizontal">The horizontal margin size in pixels.</param>
    /// <param name="vertical">The vertical margin size in pixels.</param>
    /// <returns>The current LayoutView instance for method chaining.</returns>
    public LayoutView Margin(int horizontal, int vertical)
    {
        _margin = new Thickness(horizontal, vertical);
        return this;
    }

    /// <summary>
    /// Sets specific margin values for each side of the layout view.
    /// </summary>
    /// <param name="left">The left margin size in pixels.</param>
    /// <param name="top">The top margin size in pixels.</param>
    /// <param name="right">The right margin size in pixels.</param>
    /// <param name="bottom">The bottom margin size in pixels.</param>
    /// <returns>The current LayoutView instance for method chaining.</returns>
    public LayoutView Margin(int left, int top, int right, int bottom)
    {
        _margin = new Thickness(left, top, right, bottom);
        return this;
    }

    /// <summary>
    /// Sets the background color of the layout view.
    /// </summary>
    /// <param name="color">The Colors value for the background.</param>
    /// <returns>The current LayoutView instance for method chaining.</returns>
    public LayoutView Background(Colors color)
    {
        _background = color;
        return this;
    }

    /// <summary>
    /// Sets the alignment of content within the layout view.
    /// </summary>
    /// <param name="align">The Align value for content alignment.</param>
    /// <returns>The current LayoutView instance for method chaining.</returns>
    public LayoutView Align(Align align)
    {
        _alignment = align;
        return this;
    }

    /// <summary>
    /// Centers the content within the layout view.
    /// </summary>
    /// <returns>The current LayoutView instance for method chaining.</returns>
    public LayoutView Center()
    {
        _alignment = Shared.Align.Center;
        return this;
    }

    /// <summary>
    /// Aligns the content to the left within the layout view.
    /// </summary>
    /// <returns>The current LayoutView instance for method chaining.</returns>
    public LayoutView Left()
    {
        _alignment = Shared.Align.Left;
        return this;
    }

    /// <summary>
    /// Aligns the content to the right within the layout view.
    /// </summary>
    /// <returns>The current LayoutView instance for method chaining.</returns>
    public LayoutView Right()
    {
        _alignment = Shared.Align.Right;
        return this;
    }

    /// <summary>
    /// Adds a single content element to the layout.
    /// </summary>
    /// <param name="content">The object to add to the layout.</param>
    /// <returns>The current LayoutView instance for method chaining.</returns>
    public LayoutView Add(object content)
    {
        _elements.Add(new LayoutElement(content));
        return this;
    }

    /// <summary>
    /// Adds an array of content elements to the layout.
    /// </summary>
    /// <param name="content">The array of objects to add to the layout.</param>
    /// <param name="space">The spacing strategy (defaults to "auto").</param>
    /// <returns>The current LayoutView instance for method chaining.</returns>
    public LayoutView Add(object[] content, string space = "auto")
    {
        _elements.AddRange(content.Select(e => new LayoutElement(e)));
        return this;
    }

    /// <summary>
    /// Adds a collection of content elements to the layout.
    /// </summary>
    /// <param name="content">The collection of objects to add to the layout.</param>
    /// <param name="space">The spacing strategy (defaults to "auto").</param>
    /// <returns>The current LayoutView instance for method chaining.</returns>
    public LayoutView Add(IEnumerable<object> content, string space = "auto")
    {
        _elements.AddRange(content.Select(e => new LayoutElement(e)));
        return this;
    }

    /// <summary>
    /// Arranges elements vertically in a stack layout.
    /// </summary>
    /// <param name="elements">The elements to arrange vertically.</param>
    /// <returns>The current LayoutView instance for method chaining.</returns>
    public LayoutView Vertical(params object[] elements)
    {
        _layoutType = LayoutType.Stack;
        _orientation = Orientation.Vertical;
        Add(elements);
        return this;
    }

    /// <summary>
    /// Arranges elements vertically in a stack layout.
    /// </summary>
    /// <param name="elements">The collection of elements to arrange vertically.</param>
    /// <returns>The current LayoutView instance for method chaining.</returns>
    public LayoutView Vertical(IEnumerable<object> elements)
    {
        return Vertical(elements.ToArray());
    }

    /// <summary>
    /// Arranges elements horizontally in a stack layout.
    /// </summary>
    /// <param name="elements">The elements to arrange horizontally.</param>
    /// <returns>The current LayoutView instance for method chaining.</returns>
    public LayoutView Horizontal(params object[] elements)
    {
        _layoutType = LayoutType.Stack;
        _orientation = Orientation.Horizontal;
        Add(elements);
        return this;
    }

    /// <summary>
    /// Arranges elements horizontally in a stack layout.
    /// </summary>
    /// <param name="elements">The collection of elements to arrange horizontally.</param>
    /// <returns>The current LayoutView instance for method chaining.</returns>
    public LayoutView Horizontal(IEnumerable<object> elements)
    {
        return Horizontal(elements.ToArray());
    }

    /// <summary>
    /// Arranges elements in a wrap layout that automatically wraps to new lines.
    /// </summary>
    /// <param name="elements">The elements to arrange in a wrap layout.</param>
    /// <returns>The current LayoutView instance for method chaining.</returns>
    public LayoutView Wrap(params object[] elements)
    {
        _layoutType = LayoutType.Wrap;
        Add(elements);
        return this;
    }

    /// <summary>
    /// Arranges elements in a wrap layout that automatically wraps to new lines.
    /// </summary>
    /// <param name="elements">The collection of elements to arrange in a wrap layout.</param>
    /// <returns>The current LayoutView instance for method chaining.</returns>
    public LayoutView Wrap(IEnumerable<object> elements)
    {
        return Wrap(elements.ToArray());
    }

    /// <summary>
    /// Sets the scroll behavior for the layout view.
    /// </summary>
    /// <param name="scroll">The Scroll value for scroll behavior (defaults to Auto).</param>
    /// <returns>The current LayoutView instance for method chaining.</returns>
    public LayoutView Scroll(Scroll scroll = Shared.Scroll.Auto)
    {
        _scroll = scroll;
        return this;
    }

    /// <summary>
    /// Removes parent padding from the layout view.
    /// </summary>
    /// <returns>The current LayoutView instance for method chaining.</returns>
    public LayoutView RemoveParentPadding()
    {
        _removeParentPadding = true;
        return this;
    }

    /// <summary>
    /// Builds the final layout widget based on the current configuration.
    /// </summary>
    /// <returns>A WrapLayout or StackLayout widget configured with the current settings.</returns>
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

    /// <summary>
    /// Operator overload that allows adding content to the layout using the pipe operator.
    /// </summary>
    /// <param name="view">The LayoutView to add content to.</param>
    /// <param name="child">The content to add, which can be null, an array, enumerable, or single object.</param>
    /// <returns>The updated LayoutView instance with the new content added.</returns>
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