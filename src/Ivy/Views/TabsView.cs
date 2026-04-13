// ReSharper disable once CheckNamespace
using Ivy.Core;
namespace Ivy;

public class TabView : ViewBase
{
    private readonly List<Tab> _tabs = new();
    private Size _width = Ivy.Size.Full();
    private Size _height = Ivy.Size.Full();
    private TabsVariant _variant = TabsVariant.Content;
    private bool _removeParentPadding = false;
    private Thickness? _padding = new Thickness(4);
    private string? _testId = null;
    private Action<int>? _onSelect;
    private Action<int>? _onClose;
    private Action<int>? _onCloseOthers;
    private Action<int>? _onRefresh;
    private Action<int[]>? _onReorder;
    private string? _addButtonText;
    private Action? _onAddButtonClick;
    private int? _externalSelectedIndex;
    private bool _useExternalSelectedIndex;

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
        _width = Ivy.Size.Units(unit);
        return this;
    }

    public TabView Width(float fraction)
    {
        _width = Ivy.Size.Fraction(fraction);
        return this;
    }

    public TabView Width(double fraction)
    {
        _width = Ivy.Size.Fraction(Convert.ToSingle(fraction));
        return this;
    }

    public TabView Height(Size height)
    {
        _height = height;
        return this;
    }

    public TabView Height(int unit)
    {
        _height = Ivy.Size.Units(unit);
        return this;
    }

    public TabView Height(float fraction)
    {
        _height = Ivy.Size.Fraction(fraction);
        return this;
    }

    public TabView Height(double fraction)
    {
        _height = Ivy.Size.Fraction(Convert.ToSingle(fraction));
        return this;
    }

    public TabView Size(Size size)
    {
        _width = _height = size;
        return this;
    }

    public TabView Size(int unit)
    {
        _width = Ivy.Size.Units(unit);
        _height = Ivy.Size.Units(unit);
        return this;
    }

    public TabView Size(float fraction)
    {
        _width = Ivy.Size.Fraction(fraction);
        _height = Ivy.Size.Fraction(fraction);
        return this;
    }

    public TabView Size(double fraction)
    {
        _width = Ivy.Size.Fraction(Convert.ToSingle(fraction));
        _height = Ivy.Size.Fraction(Convert.ToSingle(fraction));
        return this;
    }

    public TabView Grow()
    {
        _width = Ivy.Size.Grow();
        _height = Ivy.Size.Grow();
        return this;
    }

    public TabView Shrink()
    {
        _width = Ivy.Size.Shrink();
        _height = Ivy.Size.Shrink();
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

    public TabView TestId(string testId)
    {
        _testId = testId;
        return this;
    }

    public TabView OnSelect(Action<int>? onSelect)
    {
        _onSelect = onSelect;
        return this;
    }

    public TabView OnClose(Action<int>? onClose)
    {
        _onClose = onClose;
        return this;
    }

    public TabView OnCloseOthers(Action<int>? onCloseOthers)
    {
        _onCloseOthers = onCloseOthers;
        return this;
    }

    public TabView OnRefresh(Action<int>? onRefresh)
    {
        _onRefresh = onRefresh;
        return this;
    }

    public TabView OnReorder(Action<int[]>? onReorder)
    {
        _onReorder = onReorder;
        return this;
    }

    public TabView AddButton(string text, Action? onAddButtonClick = null)
    {
        _addButtonText = text;
        _onAddButtonClick = onAddButtonClick;
        return this;
    }

    public TabView SelectedIndex(int? selectedIndex)
    {
        _externalSelectedIndex = selectedIndex;
        _useExternalSelectedIndex = true;
        return this;
    }

    public override object? Build()
    {
        var internalIndex = UseState(0);

        int? currentIndex = _useExternalSelectedIndex ? _externalSelectedIndex : internalIndex.Value;

        void HandleSelect(Event<TabsLayout, int> @event)
        {
            if (!_useExternalSelectedIndex)
                internalIndex.Set(@event.Value);
            _onSelect?.Invoke(@event.Value);
        }

        void HandleClose(Event<TabsLayout, int> @event) => _onClose?.Invoke(@event.Value);
        void HandleCloseOthers(Event<TabsLayout, int> @event) => _onCloseOthers?.Invoke(@event.Value);
        void HandleRefresh(Event<TabsLayout, int> @event) => _onRefresh?.Invoke(@event.Value);
        void HandleReorder(Event<TabsLayout, int[]> @event) => _onReorder?.Invoke(@event.Value);
        void HandleAddButtonClick(Event<TabsLayout, int> @event) => _onAddButtonClick?.Invoke();

        var layout = new TabsLayout(
            HandleSelect,
            _onClose != null ? HandleClose : null,
            _onRefresh != null ? HandleRefresh : null,
            _onReorder != null ? HandleReorder : null,
            currentIndex,
            _tabs.ToArray()
        ).Variant(_variant).Width(_width).Height(_height).RemoveParentPadding(_removeParentPadding).Padding(_padding);

        if (_addButtonText != null)
            layout = layout.AddButton(_addButtonText, _onAddButtonClick != null ? HandleAddButtonClick : null);

        if (_onCloseOthers != null)
            layout = layout with { OnCloseOthers = ((Action<Event<TabsLayout, int>>)HandleCloseOthers).ToEventHandler() };

        if (_testId != null) layout.TestId = _testId;
        return layout;
    }
}
