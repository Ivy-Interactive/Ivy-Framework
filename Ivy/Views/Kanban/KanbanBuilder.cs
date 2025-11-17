using System.Linq.Expressions;
using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Shared;
using Ivy.Views.Builders;

namespace Ivy.Views.Kanban;

/// <summary>Fluent builder for creating kanban boards from data collections with automatic grouping and card rendering.</summary>
/// <typeparam name="TModel">The type of data objects to display in kanban cards.</typeparam>
/// <typeparam name="TGroupKey">The type of the grouping key used to organize items into columns.</typeparam>
public class KanbanBuilder<TModel, TGroupKey> : ViewBase, IStateless
    where TGroupKey : notnull
{
    private readonly IEnumerable<TModel> _records;
    private readonly Func<TModel, TGroupKey> _groupBySelector;
    private readonly BuilderFactory<TModel> _builderFactory;
    private IBuilder<TModel> _cardBuilder;
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
    private Size? _width = Size.Fit();
    private Size? _height = Size.Full();

    /// <summary>
    /// Creates a kanban builder for displaying data records as kanban cards.
    /// </summary>
    /// <param name="records">The data records to display in the kanban board.</param>
    /// <param name="groupBySelector">Function that determines grouping (kept for compatibility, not currently used).</param>
    /// <param name="cardIdSelector">Optional function to select the card ID field.</param>
    /// <param name="cardTitleSelector">Optional function to select the card title field.</param>
    /// <param name="cardDescriptionSelector">Optional function to select the card description field.</param>
    /// <param name="orderSelector">Optional function to select the field used for ordering cards.</param>
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
        _cardBuilder = _builderFactory.Default();
        _cardIdSelector = cardIdSelector;
        _cardTitleSelector = cardTitleSelector;
        _cardDescriptionSelector = cardDescriptionSelector;
        _orderSelector = orderSelector;
    }

    /// <summary>Sets a custom builder for rendering card content.</summary>
    /// <param name="builder">Factory function to create the card builder.</param>
    public KanbanBuilder<TModel, TGroupKey> Builder(Func<IBuilderFactory<TModel>, IBuilder<TModel>> builder)
    {
        _cardBuilder = builder(_builderFactory);
        return this;
    }

    /// <summary>Sets a custom card renderer function that receives the model item and returns a widget.</summary>
    /// <param name="cardRenderer">Function that takes a model item and returns a widget to display as the card.</param>
    public KanbanBuilder<TModel, TGroupKey> CardBuilder(Func<TModel, object> cardRenderer)
    {
        _customCardRenderer = cardRenderer;
        return this;
    }

    /// <param name="orderBySelector">Expression that selects the field to sort cards by.</param>
    /// <param name="descending">Whether to sort in descending order. Default is false (ascending).</param>
    public KanbanBuilder<TModel, TGroupKey> CardOrder<TOrderKey>(Expression<Func<TModel, TOrderKey>> orderBySelector, bool descending = false)
    {
        _cardOrderBySelector = orderBySelector.Compile() as Func<TModel, object?>;
        _cardOrderDescending = descending;
        return this;
    }


    /// <param name="onDelete">Event handler that receives the card ID when a card is deleted.</param>
    public KanbanBuilder<TModel, TGroupKey> HandleDelete(Func<Event<Ivy.Kanban, object?>, ValueTask> onDelete)
    {
        _onDelete = onDelete;
        return this;
    }

    /// <param name="onDelete">Event handler that receives the card ID when a card is deleted.</param>
    public KanbanBuilder<TModel, TGroupKey> HandleDelete(Action<Event<Ivy.Kanban, object?>> onDelete)
    {
        _onDelete = e => { onDelete(e); return ValueTask.CompletedTask; };
        return this;
    }

    /// <param name="onDelete">Simple action that receives the card ID when a card is deleted.</param>
    public KanbanBuilder<TModel, TGroupKey> HandleDelete(Action<object?> onDelete)
    {
        _onDelete = e => { onDelete(e.Value); return ValueTask.CompletedTask; };
        return this;
    }

    /// <param name="onMove">Event handler that receives the card ID, to column key, and target index when a card is moved.</param>
    public KanbanBuilder<TModel, TGroupKey> HandleMove(Func<Event<Ivy.Kanban, (object? CardId, TGroupKey ToColumn, int? TargetIndex)>, ValueTask> onMove)
    {
        _onMove = onMove;
        return this;
    }

    /// <param name="onMove">Event handler that receives the card ID, to column key, and target index when a card is moved.</param>
    public KanbanBuilder<TModel, TGroupKey> HandleMove(Action<Event<Ivy.Kanban, (object? CardId, TGroupKey ToColumn, int? TargetIndex)>> onMove)
    {
        _onMove = e => { onMove(e); return ValueTask.CompletedTask; };
        return this;
    }

    /// <param name="onMove">Simple action that receives a tuple with (CardId, ToColumn, TargetIndex) when a card is moved.</param>
    public KanbanBuilder<TModel, TGroupKey> HandleMove(Action<(object? CardId, TGroupKey ToColumn, int? TargetIndex)> onMove)
    {
        _onMove = e => { onMove(e.Value); return ValueTask.CompletedTask; };
        return this;
    }

    /// <param name="onClick">Event handler that receives the card ID when a card is clicked.</param>
    public KanbanBuilder<TModel, TGroupKey> HandleClick(Func<Event<KanbanCard, object?>, ValueTask> onClick)
    {
        _onClick = onClick;
        return this;
    }

    /// <param name="onClick">Event handler that receives the card ID when a card is clicked.</param>
    public KanbanBuilder<TModel, TGroupKey> HandleClick(Action<Event<KanbanCard, object?>> onClick)
    {
        _onClick = e => { onClick(e); return ValueTask.CompletedTask; };
        return this;
    }

    /// <param name="onClick">Simple action that receives the card ID when a card is clicked.</param>
    public KanbanBuilder<TModel, TGroupKey> HandleClick(Action<object?> onClick)
    {
        _onClick = e => { onClick(e.Value); return ValueTask.CompletedTask; };
        return this;
    }

    /// <param name="content">The content to display for empty kanban boards.</param>
    public KanbanBuilder<TModel, TGroupKey> Empty(object content)
    {
        _empty = content;
        return this;
    }

    /// <param name="width">The width of the kanban board.</param>
    public KanbanBuilder<TModel, TGroupKey> Width(Size? width)
    {
        _width = width;
        return this;
    }

    /// <param name="units">The width of the kanban board in units.</param>
    public KanbanBuilder<TModel, TGroupKey> Width(int units)
    {
        _width = Size.Units(units);
        return this;
    }

    /// <param name="fraction">The width of the kanban board as a fraction.</param>
    public KanbanBuilder<TModel, TGroupKey> Width(float fraction)
    {
        _width = Size.Fraction(fraction);
        return this;
    }

    /// <param name="percent">The width of the kanban board as a percentage string (e.g., "50%").</param>
    public KanbanBuilder<TModel, TGroupKey> Width(string percent)
    {
        if (percent.EndsWith("%"))
        {
            if (float.TryParse(percent.Substring(0, percent.Length - 1), out var value))
                _width = Size.Fraction(value / 100);
        }
        return this;
    }

    /// <param name="height">The height of the kanban board.</param>
    public KanbanBuilder<TModel, TGroupKey> Height(Size? height)
    {
        _height = height;
        return this;
    }

    /// <param name="units">The height of the kanban board in units.</param>
    public KanbanBuilder<TModel, TGroupKey> Height(int units)
    {
        _height = Size.Units(units);
        return this;
    }

    /// <param name="fraction">The height of the kanban board as a fraction.</param>
    public KanbanBuilder<TModel, TGroupKey> Height(float fraction)
    {
        _height = Size.Fraction(fraction);
        return this;
    }

    /// <param name="percent">The height of the kanban board as a percentage string (e.g., "50%").</param>
    public KanbanBuilder<TModel, TGroupKey> Height(string percent)
    {
        if (percent.EndsWith("%"))
        {
            if (float.TryParse(percent.Substring(0, percent.Length - 1), out var value))
                _height = Size.Fraction(value / 100);
        }
        return this;
    }

    /// <summary>
    /// Builds the complete kanban board with cards.
    /// </summary>
    public override object? Build()
    {
        if (!_records.Any()) return _empty!;

        // Apply card ordering if specified
        IEnumerable<TModel> orderedItems;
        if (_cardOrderBySelector != null)
        {
            orderedItems = _cardOrderDescending
                ? _records.OrderByDescending(_cardOrderBySelector)
                : _records.OrderBy(_cardOrderBySelector);
        }
        else
        {
            orderedItems = _records;
        }

        var cards = orderedItems.Select(item =>
        {
            object content;

            // Use custom card renderer if provided
            if (_customCardRenderer != null)
            {
                content = _customCardRenderer(item);
            }
            // Use default Card widget with Title and Description if selectors are provided
            else if (_cardTitleSelector != null || _cardDescriptionSelector != null)
            {
                var cardWidget = new Card();
                if (_cardTitleSelector != null)
                    cardWidget = cardWidget.Title(_cardTitleSelector(item)?.ToString() ?? "");
                if (_cardDescriptionSelector != null)
                    cardWidget = cardWidget.Description(_cardDescriptionSelector(item)?.ToString() ?? "");
                content = cardWidget;
            }
            // Fallback to default builder
            else
            {
                content = _cardBuilder.Build(item, item) ?? "";
            }

            var card = new KanbanCard(content);

            // Set card ID if selector is provided
            var cardId = _cardIdSelector?.Invoke(item);
            if (cardId != null)
                card = card with { CardId = cardId };

            // Set priority if order selector is provided
            var priority = _orderSelector?.Invoke(item);
            if (priority != null)
                card = card with { Priority = priority };

            // Attach OnClick handler if specified
            if (_onClick != null && cardId != null)
                card = card with { OnClick = _onClick };

            return card;
        }).ToArray();

        var kanban = new Ivy.Kanban(cards) with
        {
            ShowCounts = true,
            AllowAdd = false,
            AllowMove = _onMove != null,
            AllowDelete = _onDelete != null,
            Width = _width ?? Size.Full(),
            Height = _height ?? Size.Full()
        };

        // Attach OnDelete handler if specified
        if (_onDelete != null)
        {
            kanban = kanban with { OnDelete = _onDelete };
        }

        // Attach OnCardMove handler if specified
        if (_onMove != null)
        {
            kanban = kanban with
            {
                OnCardMove = e => _onMove(new Event<Ivy.Kanban, (object?, TGroupKey, int?)>(
                                e.EventName,
                                e.Sender,
                                (e.Value.CardId, (TGroupKey)e.Value.ToColumn!, e.Value.TargetIndex)))
            };
        }

        return kanban;
    }
}
