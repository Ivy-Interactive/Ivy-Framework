using System.Linq.Expressions;

// ReSharper disable once CheckNamespace
namespace Ivy;

public class CalendarBuilder<TModel>(
    IEnumerable<TModel> records,
    Expression<Func<TModel, DateTime>> startSelector,
    Expression<Func<TModel, DateTime>> endSelector,
    Expression<Func<TModel, object?>>? eventIdSelector = null)
    : ViewBase, IStateless
{
    private Func<TModel, object>? _eventRenderer;
    private Expression<Func<TModel, string?>>? _titleSelector;
    private Expression<Func<TModel, bool>>? _allDaySelector;
    private Expression<Func<TModel, string?>>? _colorSelector;
    private CalendarDisplayMode _defaultView = CalendarDisplayMode.Month;
    private DateTime? _date;
    private bool _enableDragDrop;
    private bool _showToolbar = true;
    private Size? _width = Size.Full();
    private Size? _height = Size.Full();
    private Func<Event<Calendar, (object? EventId, DateTime Start, DateTime End)>, ValueTask>? _onMove;
    private Func<Event<Calendar, object?>, ValueTask>? _onClick;
    private Func<Event<Calendar, (DateTime Start, DateTime End)>, ValueTask>? _onSelectSlot;

    public CalendarBuilder<TModel> EventBuilder(Func<TModel, object> eventRenderer)
    {
        _eventRenderer = eventRenderer;
        return this;
    }

    public CalendarBuilder<TModel> Title(Expression<Func<TModel, string?>> titleSelector)
    {
        _titleSelector = titleSelector;
        return this;
    }

    public CalendarBuilder<TModel> AllDay(Expression<Func<TModel, bool>> allDaySelector)
    {
        _allDaySelector = allDaySelector;
        return this;
    }

    public CalendarBuilder<TModel> Color(Expression<Func<TModel, string?>> colorSelector)
    {
        _colorSelector = colorSelector;
        return this;
    }

    public CalendarBuilder<TModel> DefaultView(CalendarDisplayMode view)
    {
        _defaultView = view;
        return this;
    }

    public CalendarBuilder<TModel> Date(DateTime date)
    {
        _date = date;
        return this;
    }

    public CalendarBuilder<TModel> EnableDragDrop(bool enable = true)
    {
        _enableDragDrop = enable;
        return this;
    }

    public CalendarBuilder<TModel> ShowToolbar(bool show = true)
    {
        _showToolbar = show;
        return this;
    }

    public CalendarBuilder<TModel> Width(Size? width)
    {
        _width = width;
        return this;
    }

    public CalendarBuilder<TModel> Height(Size? height)
    {
        _height = height;
        return this;
    }

    public CalendarBuilder<TModel> OnMove(Action<(object? EventId, DateTime Start, DateTime End)> onMove)
    {
        _onMove = e => { onMove(e.Value); return ValueTask.CompletedTask; };
        return this;
    }

    public CalendarBuilder<TModel> OnMove(EventHandler<Event<Calendar, (object? EventId, DateTime Start, DateTime End)>> onMove)
    {
        _onMove = onMove;
        return this;
    }

    public CalendarBuilder<TModel> OnClick(Action<object?> onClick)
    {
        _onClick = e => { onClick(e.Value); return ValueTask.CompletedTask; };
        return this;
    }

    public CalendarBuilder<TModel> OnSelectSlot(Action<(DateTime Start, DateTime End)> onSelectSlot)
    {
        _onSelectSlot = e => { onSelectSlot(e.Value); return ValueTask.CompletedTask; };
        return this;
    }

    public override object? Build()
    {
        var startFunc = startSelector.Compile();
        var endFunc = endSelector.Compile();
        var idFunc = eventIdSelector?.Compile();
        var titleFunc = _titleSelector?.Compile();
        var allDayFunc = _allDaySelector?.Compile();
        var colorFunc = _colorSelector?.Compile();

        var calendarEvents = records.Select(item =>
        {
            object? content = null;
            if (_eventRenderer != null)
            {
                content = _eventRenderer(item);
            }

            var evt = new CalendarEvent(content)
            {
                Start = startFunc(item),
                End = endFunc(item),
            };

            if (idFunc != null)
                evt = evt with { EventId = idFunc(item) };

            if (titleFunc != null)
                evt = evt with { Title = titleFunc(item) };

            if (allDayFunc != null)
                evt = evt with { AllDay = allDayFunc(item) };

            if (colorFunc != null)
                evt = evt with { Color = colorFunc(item) };

            return evt;
        }).ToArray();

        var calendar = new Calendar(calendarEvents)
        {
            DefaultView = _defaultView,
            Date = _date,
            EnableDragDrop = _enableDragDrop,
            ShowToolbar = _showToolbar,
            Width = _width ?? Size.Full(),
            Height = _height ?? Size.Full(),
        };

        if (_onMove != null)
        {
            calendar = calendar with
            {
                OnEventMove = new(e => _onMove(e))
            };
        }

        if (_onClick != null)
        {
            calendar = calendar with
            {
                OnEventClick = new(e => _onClick(e))
            };
        }

        if (_onSelectSlot != null)
        {
            calendar = calendar with
            {
                OnSelectSlot = new(e => _onSelectSlot(e))
            };
        }

        return calendar;
    }
}
