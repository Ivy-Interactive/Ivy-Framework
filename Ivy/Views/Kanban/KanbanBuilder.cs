using System.Linq.Expressions;
using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Shared;
using Ivy.Views.Builders;

namespace Ivy.Views.Kanban;

/// <summary>
/// Fluent builder for creating kanban boards from data collections with automatic grouping and card rendering.
/// </summary>
/// <typeparam name="TModel">The type of data objects to display in kanban cards.</typeparam>
/// <typeparam name="TGroupKey">The type of the grouping key used to organize items into columns.</typeparam>
public class KanbanBuilder<TModel, TGroupKey> : ViewBase, IStateless
    where TGroupKey : notnull
{
    private readonly IEnumerable<TModel> _records;
    private readonly Func<TModel, TGroupKey> _groupBySelector;
    private readonly BuilderFactory<TModel> _builderFactory;
    private IBuilder<TModel> _cardBuilder;
    private Func<TGroupKey, string>? _columnTitleFormatter;
    private Func<TModel, object?>? _columnOrderBySelector;
    private bool _columnOrderDescending;
    private Func<TModel, object?>? _cardOrderBySelector;
    private bool _cardOrderDescending;
    private Func<Event<KanbanColumn, TGroupKey>, ValueTask>? _onAdd;
    private readonly Func<TModel, object?>? _cardIdSelector;
    private readonly Func<TModel, object?>? _cardTitleSelector;
    private readonly Func<TModel, object?>? _cardDescriptionSelector;
    private readonly Func<TModel, object?>? _orderSelector;
    private Func<TModel, object>? _customCardRenderer;
    private Func<Event<Ivy.Kanban, object?>, ValueTask>? _onDelete;
    private Func<Event<Ivy.Kanban, (object? CardId, TGroupKey FromColumn, TGroupKey ToColumn, int? TargetIndex)>, ValueTask>? _onMove;
    private Func<Event<KanbanCard, object?>, ValueTask>? _onClick;
    private object? _empty;
    private Size? _width = Size.Fit();
    private Size? _height = Size.Full();
    private readonly Dictionary<TGroupKey, Size> _columnWidths = new();

    /// <summary>
    /// Creates a kanban builder with automatic column grouping based on the selector.
    /// </summary>
    /// <param name="records">The data records to display in the kanban board.</param>
    /// <param name="groupBySelector">Function that determines which column each item belongs to.</param>
    /// <param name="cardIdSelector">Optional function to select the card ID field.</param>
    /// <param name="cardTitleSelector">Optional function to select the card title field.</param>
    /// <param name="cardDescriptionSelector">Optional function to select the card description field.</param>
    /// <param name="orderSelector">Optional function to select the field used for ordering cards within columns.</param>
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

    /// <summary>Sets a custom formatter for column titles.</summary>
    /// <param name="formatter">Function to format the group key into a column title.</param>
    public KanbanBuilder<TModel, TGroupKey> ColumnTitle(Func<TGroupKey, string> formatter)
    {
        _columnTitleFormatter = formatter;
        return this;
    }

    /// <summary>Sets the order of columns in the kanban board by sorting based on a field expression.</summary>
    /// <param name="orderBySelector">Expression that selects the field to sort columns by.</param>
    /// <param name="descending">Whether to sort in descending order. Default is false (ascending).</param>
    public KanbanBuilder<TModel, TGroupKey> ColumnOrder<TOrderKey>(Expression<Func<TModel, TOrderKey>> orderBySelector, bool descending = false)
    {
        _columnOrderBySelector = orderBySelector.Compile() as Func<TModel, object?>;
        _columnOrderDescending = descending;
        return this;
    }

    /// <summary>Sets the order of cards within each column by sorting based on a field expression.</summary>
    /// <param name="orderBySelector">Expression that selects the field to sort cards by.</param>
    /// <param name="descending">Whether to sort in descending order. Default is false (ascending).</param>
    public KanbanBuilder<TModel, TGroupKey> CardOrder<TOrderKey>(Expression<Func<TModel, TOrderKey>> orderBySelector, bool descending = false)
    {
        _cardOrderBySelector = orderBySelector.Compile() as Func<TModel, object?>;
        _cardOrderDescending = descending;
        return this;
    }

    /// <summary>Sets the event handler called when a card is added to a column.</summary>
    /// <param name="onAdd">Event handler that receives the column key (TGroupKey) when a card is added.</param>
    public KanbanBuilder<TModel, TGroupKey> HandleAdd(Func<Event<KanbanColumn, TGroupKey>, ValueTask> onAdd)
    {
        _onAdd = onAdd;
        return this;
    }

    /// <summary>Sets the event handler called when a card is added to a column.</summary>
    /// <param name="onAdd">Event handler that receives the column key (TGroupKey) when a card is added.</param>
    public KanbanBuilder<TModel, TGroupKey> HandleAdd(Action<Event<KanbanColumn, TGroupKey>> onAdd)
    {
        _onAdd = e => { onAdd(e); return ValueTask.CompletedTask; };
        return this;
    }

    /// <summary>Sets a simple event handler called when a card is added to a column.</summary>
    /// <param name="onAdd">Simple action that receives the column key (TGroupKey) when a card is added.</param>
    public KanbanBuilder<TModel, TGroupKey> HandleAdd(Action<TGroupKey> onAdd)
    {
        _onAdd = e => { onAdd(e.Value); return ValueTask.CompletedTask; };
        return this;
    }

    /// <summary>Sets the event handler called when a card is deleted.</summary>
    /// <param name="onDelete">Event handler that receives the card ID when a card is deleted.</param>
    public KanbanBuilder<TModel, TGroupKey> HandleDelete(Func<Event<Ivy.Kanban, object?>, ValueTask> onDelete)
    {
        _onDelete = onDelete;
        return this;
    }

    /// <summary>Sets the event handler called when a card is deleted.</summary>
    /// <param name="onDelete">Event handler that receives the card ID when a card is deleted.</param>
    public KanbanBuilder<TModel, TGroupKey> HandleDelete(Action<Event<Ivy.Kanban, object?>> onDelete)
    {
        _onDelete = e => { onDelete(e); return ValueTask.CompletedTask; };
        return this;
    }

    /// <summary>Sets a simple event handler called when a card is deleted.</summary>
    /// <param name="onDelete">Simple action that receives the card ID when a card is deleted.</param>
    public KanbanBuilder<TModel, TGroupKey> HandleDelete(Action<object?> onDelete)
    {
        _onDelete = e => { onDelete(e.Value); return ValueTask.CompletedTask; };
        return this;
    }

    /// <summary>Sets the event handler called when a card is moved between columns or reordered within a column.</summary>
    /// <param name="onMove">Event handler that receives the card ID, from column key, to column key, and target index when a card is moved.</param>
    public KanbanBuilder<TModel, TGroupKey> HandleMove(Func<Event<Ivy.Kanban, (object? CardId, TGroupKey FromColumn, TGroupKey ToColumn, int? TargetIndex)>, ValueTask> onMove)
    {
        _onMove = onMove;
        return this;
    }

    /// <summary>Sets the event handler called when a card is moved between columns or reordered within a column.</summary>
    /// <param name="onMove">Event handler that receives the card ID, from column key, to column key, and target index when a card is moved.</param>
    public KanbanBuilder<TModel, TGroupKey> HandleMove(Action<Event<Ivy.Kanban, (object? CardId, TGroupKey FromColumn, TGroupKey ToColumn, int? TargetIndex)>> onMove)
    {
        _onMove = e => { onMove(e); return ValueTask.CompletedTask; };
        return this;
    }

    /// <summary>Sets a simple event handler called when a card is moved between columns or reordered within a column.</summary>
    /// <param name="onMove">Simple action that receives a tuple with (CardId, FromColumn, ToColumn, TargetIndex) when a card is moved.</param>
    public KanbanBuilder<TModel, TGroupKey> HandleMove(Action<(object? CardId, TGroupKey FromColumn, TGroupKey ToColumn, int? TargetIndex)> onMove)
    {
        _onMove = e => { onMove(e.Value); return ValueTask.CompletedTask; };
        return this;
    }

    /// <summary>Sets the event handler called when a card is clicked.</summary>
    /// <param name="onClick">Event handler that receives the card ID when a card is clicked.</param>
    public KanbanBuilder<TModel, TGroupKey> HandleClick(Func<Event<KanbanCard, object?>, ValueTask> onClick)
    {
        _onClick = onClick;
        return this;
    }

    /// <summary>Sets the event handler called when a card is clicked.</summary>
    /// <param name="onClick">Event handler that receives the card ID when a card is clicked.</param>
    public KanbanBuilder<TModel, TGroupKey> HandleClick(Action<Event<KanbanCard, object?>> onClick)
    {
        _onClick = e => { onClick(e); return ValueTask.CompletedTask; };
        return this;
    }

    /// <summary>Sets a simple event handler called when a card is clicked.</summary>
    /// <param name="onClick">Simple action that receives the card ID when a card is clicked.</param>
    public KanbanBuilder<TModel, TGroupKey> HandleClick(Action<object?> onClick)
    {
        _onClick = e => { onClick(e.Value); return ValueTask.CompletedTask; };
        return this;
    }

    /// <summary>Sets content to display when the kanban board has no data.</summary>
    /// <param name="content">The content to display for empty kanban boards.</param>
    public KanbanBuilder<TModel, TGroupKey> Empty(object content)
    {
        _empty = content;
        return this;
    }

    /// <summary>Sets the width of the kanban board.</summary>
    /// <param name="width">The width of the kanban board.</param>
    public KanbanBuilder<TModel, TGroupKey> Width(Size? width)
    {
        _width = width;
        return this;
    }

    /// <summary>Sets the width of the kanban board in units.</summary>
    /// <param name="units">The width of the kanban board in units.</param>
    public KanbanBuilder<TModel, TGroupKey> Width(int units)
    {
        _width = Size.Units(units);
        return this;
    }

    /// <summary>Sets the width of the kanban board as a fraction.</summary>
    /// <param name="fraction">The width of the kanban board as a fraction.</param>
    public KanbanBuilder<TModel, TGroupKey> Width(float fraction)
    {
        _width = Size.Fraction(fraction);
        return this;
    }

    /// <summary>Sets the width of the kanban board as a percentage.</summary>
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

    /// <summary>Sets the height of the kanban board.</summary>
    /// <param name="height">The height of the kanban board.</param>
    public KanbanBuilder<TModel, TGroupKey> Height(Size? height)
    {
        _height = height;
        return this;
    }

    /// <summary>Sets the height of the kanban board in units.</summary>
    /// <param name="units">The height of the kanban board in units.</param>
    public KanbanBuilder<TModel, TGroupKey> Height(int units)
    {
        _height = Size.Units(units);
        return this;
    }

    /// <summary>Sets the height of the kanban board as a fraction.</summary>
    /// <param name="fraction">The height of the kanban board as a fraction.</param>
    public KanbanBuilder<TModel, TGroupKey> Height(float fraction)
    {
        _height = Size.Fraction(fraction);
        return this;
    }

    /// <summary>Sets the height of the kanban board as a percentage.</summary>
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

    /// <summary>Sets the width of columns based on their group key value.</summary>
    /// <param name="groupKeySelector">Expression that selects the group key field (same as used in ToKanban grouping).</param>
    /// <param name="width">The width to set for columns matching the group key.</param>
    /// <remarks>
    /// This method sets the width for all columns that have the same group key value as the first record.
    /// For more precise control, use Width(TGroupKey, Size) with specific group key values.
    /// </remarks>
    public KanbanBuilder<TModel, TGroupKey> Width(Expression<Func<TModel, TGroupKey>> groupKeySelector, Size width)
    {
        // Evaluate the selector on all unique group keys to set widths for all matching columns
        var compiledSelector = groupKeySelector.Compile();
        var uniqueKeys = _records.Select(compiledSelector).Distinct();
        foreach (var key in uniqueKeys)
        {
            _columnWidths[key] = width;
        }
        return this;
    }

    /// <summary>Sets the width of a specific column identified by the group key.</summary>
    /// <param name="groupKey">The group key that identifies the column.</param>
    /// <param name="width">The width to set for the column.</param>
    public KanbanBuilder<TModel, TGroupKey> Width(TGroupKey groupKey, Size width)
    {
        _columnWidths[groupKey] = width;
        return this;
    }

    /// <summary>
    /// Builds the complete kanban board with columns and cards.
    /// </summary>
    public override object? Build()
    {
        if (!_records.Any()) return _empty!;

        var grouped = _records.GroupBy(_groupBySelector);

        // Apply column ordering if specified
        IEnumerable<IGrouping<TGroupKey, TModel>> orderedGroups;
        if (_columnOrderBySelector != null)
        {
            var groupsWithKeys = grouped
                .Select(g => new { Group = g, SortKey = g.FirstOrDefault() })
                .Where(x => x.SortKey != null);

            orderedGroups = _columnOrderDescending
                ? groupsWithKeys.OrderByDescending(x => _columnOrderBySelector(x.SortKey!)).Select(x => x.Group)
                : groupsWithKeys.OrderBy(x => _columnOrderBySelector(x.SortKey!)).Select(x => x.Group);
        }
        else
        {
            // Default to alphabetical ordering by group key
            orderedGroups = grouped.OrderBy(g => g.Key?.ToString());
        }

        var columns = orderedGroups
            .Where(group => group != null)
            .Select(group =>
            {
                // Use natural order of items (no automatic sorting by priority)
                // Users can drag cards around without affecting priority values
                IEnumerable<TModel> orderedItems = group!;

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

                var title = _columnTitleFormatter != null
                    ? _columnTitleFormatter(group!.Key)
                    : group!.Key?.ToString() ?? "";

                var column = new KanbanColumn(cards)
                    .Title(title)
                    .ColumnKey(group!.Key);

                // Apply column width if specified
                if (group!.Key != null && _columnWidths.TryGetValue(group.Key, out var columnWidth))
                {
                    column = column.Width(columnWidth);
                }

                // Attach OnAdd handler if specified
                if (_onAdd != null)
                {
                    var columnKey = group!.Key!;
                    column = column with
                    {
                        OnAdd = e => _onAdd(new Event<KanbanColumn, TGroupKey>(e.EventName, e.Sender, columnKey))
                    };
                }

                return column;
            }).ToArray();

        var kanban = new Ivy.Kanban(columns) with
        {
            ShowCounts = true,
            AllowAdd = _onAdd != null,
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

        // Attach OnMove handler if specified
        if (_onMove != null)
        {
            kanban = kanban with
            {
                OnMove = e => _onMove(new Event<Ivy.Kanban, (object?, TGroupKey, TGroupKey, int?)>(
                                e.EventName,
                                e.Sender,
                                (e.Value.CardId, (TGroupKey)e.Value.FromColumn!, (TGroupKey)e.Value.ToColumn!, e.Value.TargetIndex)))
            };
        }

        return kanban;
    }
}
