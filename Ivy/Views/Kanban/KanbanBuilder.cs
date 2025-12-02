using System.Linq.Expressions;
using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Shared;
using Ivy.Views.Builders;

namespace Ivy.Views.Kanban;

public class KanbanBuilder<TModel, TGroupKey>(
    IEnumerable<TModel> records,
    Func<TModel, TGroupKey> groupBySelector,
    Func<TModel, object?>? cardIdSelector = null,
    Func<TModel, object?>? cardOrderSelector = null)
    : ViewBase, IStateless
    where TGroupKey : notnull
{
    private readonly BuilderFactory<TModel> _builderFactory = new();
    private IBuilder<TModel>? _cardBuilder;
    private Func<TModel, object?>? _columnOrderBySelector;
    private bool _columnOrderDescending;
    private Func<TModel, object?>? _cardOrderBySelector;
    private bool _cardOrderDescending;
    private Func<TModel, object>? _customCardRenderer;
    private Func<Event<Ivy.Kanban, (object? CardId, TGroupKey ToColumn, int? TargetIndex)>, ValueTask>? _onMove;
    private object? _empty;
    private Size? _width = Size.Full();
    private Size? _height = Size.Full();
    private Size? _columnWidth;

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

    public KanbanBuilder<TModel, TGroupKey> HandleMove(Func<Event<Ivy.Kanban, (object? CardId, TGroupKey ToColumn, int? TargetIndex)>, ValueTask> onMove)
    {
        _onMove = onMove;
        return this;
    }

    public KanbanBuilder<TModel, TGroupKey> HandleMove(Action<Event<Ivy.Kanban, (object? CardId, TGroupKey ToColumn, int? TargetIndex)>> onMove)
    {
        _onMove = e => { onMove(e); return ValueTask.CompletedTask; };
        return this;
    }

    public KanbanBuilder<TModel, TGroupKey> HandleMove(Action<(object? CardId, TGroupKey ToColumn, int? TargetIndex)> onMove)
    {
        _onMove = e => { onMove(e.Value); return ValueTask.CompletedTask; };
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

    public KanbanBuilder<TModel, TGroupKey> Height(Size? height)
    {
        _height = height;
        return this;
    }

    public KanbanBuilder<TModel, TGroupKey> ColumnWidth(Size width)
    {
        _columnWidth = width;
        return this;
    }

    public override object? Build()
    {
        if (!records.Any())
        {
            return _empty ?? new Fragment();
        }

        var grouped = records.GroupBy(groupBySelector);

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
            else
            {
                throw new InvalidOperationException("CardBuilder must be specified. Use .CardBuilder() to provide a card renderer.");
            }

            var card = new KanbanCard(content);

            var cardId = cardIdSelector?.Invoke(item);
            if (cardId != null)
                card = card with { CardId = cardId };

            var priority = cardOrderSelector?.Invoke(item);
            if (priority != null)
                card = card with { Priority = priority };

            card = card with { Column = groupKey };

            return card;
        }).ToArray();

        var kanban = new Ivy.Kanban(cards) with
        {
            ShowCounts = true,
            Width = _width ?? Size.Full(),
            Height = _height ?? Size.Full(),
            ColumnWidth = _columnWidth
        };

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
