using System.Linq.Expressions;
using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Shared;
using Ivy.Views.Builders;

namespace Ivy.Views.Kanban;

public class KanbanBuilder<TModel, TGroupKey> : ViewBase, IStateless
    where TGroupKey : notnull
{
    private readonly IEnumerable<TModel> _records;
    private readonly Func<TModel, TGroupKey> _groupBySelector;
    private readonly BuilderFactory<TModel> _builderFactory;
    private IBuilder<TModel>? _cardBuilder;
    private Func<TModel, object?>? _columnOrderBySelector;
    private bool _columnOrderDescending;
    private Func<TModel, object?>? _cardOrderBySelector;
    private bool _cardOrderDescending;
    private readonly Func<TModel, object?>? _cardIdSelector;
    private readonly Func<TModel, object?>? _cardTitleSelector;
    private readonly Func<TModel, object?>? _cardDescriptionSelector;
    private readonly Func<TModel, object?>? _orderSelector;
    private Func<TModel, object>? _customCardRenderer;
    private Func<Event<Ivy.Kanban, object?>, ValueTask>? _onDelete;
    private Func<Event<Ivy.Kanban, (object? CardId, TGroupKey ToColumn, int? TargetIndex)>, ValueTask>? _onMove;
    private Func<Event<KanbanCard, object?>, ValueTask>? _onClick;
    private object? _empty;
    private Size? _width = Size.Full();
    private Size? _height = Size.Full();
    private readonly Dictionary<TGroupKey, Size> _columnWidths = new();

    public KanbanBuilder(
        IEnumerable<TModel> records,
        Func<TModel, TGroupKey> groupBySelector,
        Func<TModel, object?>? cardIdSelector = null,
        Func<TModel, object?>? cardTitleSelector = null,
        Func<TModel, object?>? cardDescriptionSelector = null,
        Func<TModel, object?>? orderSelector = null)
    {
        _records = records;
        _groupBySelector = groupBySelector;
        _builderFactory = new BuilderFactory<TModel>();
        _cardIdSelector = cardIdSelector;
        _cardTitleSelector = cardTitleSelector;
        _cardDescriptionSelector = cardDescriptionSelector;
        _orderSelector = orderSelector;
    }

    public KanbanBuilder<TModel, TGroupKey> Builder(Func<IBuilderFactory<TModel>, IBuilder<TModel>> builder)
    {
        _cardBuilder = builder(_builderFactory);
        return this;
    }

    public KanbanBuilder<TModel, TGroupKey> CardBuilder(Func<TModel, object> cardRenderer)
    {
        _customCardRenderer = cardRenderer;
        return this;
    }

    public KanbanBuilder<TModel, TGroupKey> CardBuilder(Func<IBuilderFactory<TModel>, IBuilder<TModel>> builder)
    {
        _cardBuilder = builder(_builderFactory);
        return this;
    }

    public KanbanBuilder<TModel, TGroupKey> ColumnOrder<TOrderKey>(Expression<Func<TModel, TOrderKey>> orderBySelector, bool descending = false)
    {
        var compiled = orderBySelector.Compile();
        _columnOrderBySelector = item => compiled(item);
        _columnOrderDescending = descending;
        return this;
    }

    public KanbanBuilder<TModel, TGroupKey> CardOrder<TOrderKey>(Expression<Func<TModel, TOrderKey>> orderBySelector, bool descending = false)
    {
        var compiled = orderBySelector.Compile();
        _cardOrderBySelector = item => compiled(item);
        _cardOrderDescending = descending;
        return this;
    }


    public KanbanBuilder<TModel, TGroupKey> HandleDelete(Func<Event<Ivy.Kanban, object?>, ValueTask> onDelete)
    {
        _onDelete = onDelete;
        return this;
    }

    public KanbanBuilder<TModel, TGroupKey> HandleDelete(Action<Event<Ivy.Kanban, object?>> onDelete)
    {
        _onDelete = e => { onDelete(e); return ValueTask.CompletedTask; };
        return this;
    }

    public KanbanBuilder<TModel, TGroupKey> HandleDelete(Action<object?> onDelete)
    {
        _onDelete = e => { onDelete(e.Value); return ValueTask.CompletedTask; };
        return this;
    }

    public KanbanBuilder<TModel, TGroupKey> HandleCardMove(Func<Event<Ivy.Kanban, (object? CardId, TGroupKey ToColumn, int? TargetIndex)>, ValueTask> onMove)
    {
        _onMove = onMove;
        return this;
    }

    public KanbanBuilder<TModel, TGroupKey> HandleCardMove(Action<Event<Ivy.Kanban, (object? CardId, TGroupKey ToColumn, int? TargetIndex)>> onMove)
    {
        _onMove = e => { onMove(e); return ValueTask.CompletedTask; };
        return this;
    }

    public KanbanBuilder<TModel, TGroupKey> HandleCardMove(Action<(object? CardId, TGroupKey ToColumn, int? TargetIndex)> onMove)
    {
        _onMove = e => { onMove(e.Value); return ValueTask.CompletedTask; };
        return this;
    }

    public KanbanBuilder<TModel, TGroupKey> HandleClick(Func<Event<KanbanCard, object?>, ValueTask> onClick)
    {
        _onClick = onClick;
        return this;
    }

    public KanbanBuilder<TModel, TGroupKey> HandleClick(Action<Event<KanbanCard, object?>> onClick)
    {
        _onClick = e => { onClick(e); return ValueTask.CompletedTask; };
        return this;
    }

    public KanbanBuilder<TModel, TGroupKey> HandleClick(Action<object?> onClick)
    {
        _onClick = e => { onClick(e.Value); return ValueTask.CompletedTask; };
        return this;
    }

    public KanbanBuilder<TModel, TGroupKey> Empty(object content)
    {
        _empty = content;
        return this;
    }

    public KanbanBuilder<TModel, TGroupKey> Width(Size? width)
    {
        _width = width;
        return this;
    }

    public KanbanBuilder<TModel, TGroupKey> Width(int units)
    {
        _width = Size.Units(units);
        return this;
    }

    public KanbanBuilder<TModel, TGroupKey> Width(float fraction)
    {
        _width = Size.Fraction(fraction);
        return this;
    }

    public KanbanBuilder<TModel, TGroupKey> Width(string percent)
    {
        if (percent.EndsWith("%"))
        {
            if (float.TryParse(percent.Substring(0, percent.Length - 1), out var value))
                _width = Size.Fraction(value / 100);
        }
        return this;
    }

    public KanbanBuilder<TModel, TGroupKey> Width(Expression<Func<TModel, TGroupKey>> groupKeySelector, Size width)
    {
        var compiledSelector = groupKeySelector.Compile();
        var uniqueKeys = _records.Select(compiledSelector).Distinct().ToList();
        foreach (var key in uniqueKeys)
        {
            _columnWidths[key] = width;
        }
        return this;
    }

    public KanbanBuilder<TModel, TGroupKey> Width(TGroupKey groupKey, Size width)
    {
        _columnWidths[groupKey] = width;
        return this;
    }

    public KanbanBuilder<TModel, TGroupKey> Height(Size? height)
    {
        _height = height;
        return this;
    }

    public KanbanBuilder<TModel, TGroupKey> Height(int units)
    {
        _height = Size.Units(units);
        return this;
    }

    public KanbanBuilder<TModel, TGroupKey> Height(float fraction)
    {
        _height = Size.Fraction(fraction);
        return this;
    }

    public KanbanBuilder<TModel, TGroupKey> Height(string percent)
    {
        if (percent.EndsWith("%"))
        {
            if (float.TryParse(percent.Substring(0, percent.Length - 1), out var value))
                _height = Size.Fraction(value / 100);
        }
        return this;
    }

    public override object? Build()
    {
        if (!_records.Any())
        {
            return _empty ?? new Fragment();
        }

        var grouped = _records.GroupBy(_groupBySelector);

        IEnumerable<IGrouping<TGroupKey, TModel>> orderedGroups;
        if (_columnOrderBySelector != null)
        {
            orderedGroups = _columnOrderDescending
                ? grouped.OrderByDescending(g => _columnOrderBySelector(g.First()))
                : grouped.OrderBy(g => _columnOrderBySelector(g.First()));
        }
        else
        {
            orderedGroups = grouped;
        }

        var itemsWithGroupKey = orderedGroups.SelectMany(group =>
        {
            var groupKey = group.Key;
            IEnumerable<TModel> itemsInGroup;

            if (_cardOrderBySelector != null)
            {
                itemsInGroup = _cardOrderDescending
                    ? group.OrderByDescending(_cardOrderBySelector)
                    : group.OrderBy(_cardOrderBySelector);
            }
            else
            {
                itemsInGroup = group;
            }

            return itemsInGroup.Select(item => new { Item = item, GroupKey = groupKey });
        });

        var cards = itemsWithGroupKey.Select(itemWithKey =>
        {
            var item = itemWithKey.Item;
            var groupKey = itemWithKey.GroupKey;

            object content;

            if (_customCardRenderer != null)
            {
                content = _customCardRenderer(item);
            }
            else if (_cardBuilder != null)
            {
                content = _cardBuilder.Build(item, item) ?? "";
            }
            else if (_cardTitleSelector != null || _cardDescriptionSelector != null)
            {
                var cardWidget = new Card();
                if (_cardTitleSelector != null)
                    cardWidget = cardWidget.Title(_cardTitleSelector(item)?.ToString() ?? "");
                if (_cardDescriptionSelector != null)
                    cardWidget = cardWidget.Description(_cardDescriptionSelector(item)?.ToString() ?? "");
                content = cardWidget;
            }
            else
            {
                content = _builderFactory.Default().Build(item, item) ?? "";
            }

            var card = new KanbanCard(content);

            var cardId = _cardIdSelector?.Invoke(item);
            if (cardId != null)
                card = card with { CardId = cardId };

            var priority = _orderSelector?.Invoke(item);
            if (priority != null)
                card = card with { Priority = priority };

            card = card with { Column = groupKey };

            if (_onClick != null && cardId != null)
                card = card with { OnClick = _onClick };

            return card;
        }).ToArray();

        var columnWidthsDict = _columnWidths.Any()
            ? new Dictionary<object, Size>(_columnWidths.ToDictionary(kvp => (object)kvp.Key!, kvp => kvp.Value))
            : null;

        var kanban = new Ivy.Kanban(cards) with
        {
            ShowCounts = true,
            AllowAdd = false,
            AllowMove = _onMove != null,
            AllowDelete = _onDelete != null,
            Width = _width ?? Size.Full(),
            Height = _height ?? Size.Full(),
            ColumnWidths = columnWidthsDict
        };

        if (_onDelete != null)
        {
            kanban = kanban with { OnDelete = _onDelete };
        }

        if (_onMove != null)
        {
            kanban = kanban with
            {
                OnCardMove = e =>
                {
                    if (e.Value.ToColumn == null)
                        return ValueTask.CompletedTask;

                    if (e.Value.ToColumn is TGroupKey groupKey)
                    {
                        return _onMove(new Event<Ivy.Kanban, (object?, TGroupKey, int?)>(
                            e.EventName,
                            e.Sender,
                            (e.Value.CardId, groupKey, e.Value.TargetIndex)));
                    }

                    try
                    {
                        var convertedKey = (TGroupKey)Convert.ChangeType(e.Value.ToColumn, typeof(TGroupKey));
                        return _onMove(new Event<Ivy.Kanban, (object?, TGroupKey, int?)>(
                                e.EventName,
                                e.Sender,
                            (e.Value.CardId, convertedKey, e.Value.TargetIndex)));
                    }
                    catch
                    {
                        return ValueTask.CompletedTask;
                    }
                }
            };
        }

        return kanban;
    }
}
