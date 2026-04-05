using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using Ivy.Core.Helpers;
using Microsoft.Extensions.AI;

// ReSharper disable once CheckNamespace
namespace Ivy;

public class DataTableBuilder<TModel>(
    IQueryable<TModel> queryable,
    Expression<Func<TModel, object?>>? idSelector = null)
    : ViewBase, IMemoized
{
    private Size? _width;
    private Size? _height;
    private readonly Dictionary<string, InternalColumn> _columns = [];
    private readonly DataTableConfig _configuration = new();
    private Func<Event<DataTable, CellClickEventArgs>, ValueTask>? _onCellClick;
    private Func<Event<DataTable, CellClickEventArgs>, ValueTask>? _onCellActivated;
    private MenuItem[]? _menuItemRowActions;
    private Func<Event<DataTable, RowActionClickEventArgs>, ValueTask>? _onRowAction;
    private readonly Dictionary<string, EventHandler<object>> _cellActions = [];
    private RefreshToken? _refreshToken;
    private FuncViewBuilder? _emptyViewFactory;
    private FuncViewBuilder? _headerLeftFactory;
    private FuncViewBuilder? _headerRightFactory;
    private Dictionary<string, object>? _footerValuesByColumn;

    private readonly string? _idColumnName =
        idSelector != null ? TypeHelper.GetNameFromMemberExpression(idSelector.Body) : null;

    private readonly Func<TModel, object?>? _idSelectorFunc = idSelector?.Compile();

    private class InternalColumn
    {
        public required DataTableColumn Column { get; init; }
        public bool Removed { get; set; }
    }

    public DataTableBuilder(IQueryable<TModel> queryable) : this(queryable, null)
    {
        _Scaffold();
    }

    internal void Initialize()
    {
        if (_columns.Count == 0)
        {
            _Scaffold();
        }
    }

    private void _Scaffold()
    {
        var type = typeof(TModel);

        var fields = type
            .GetFields()
            .Where(f => f.GetCustomAttribute<ScaffoldColumnAttribute>()?.Scaffold != false)
            .Select(e => new { e.Name, Type = e.FieldType, FieldInfo = e, PropertyInfo = (PropertyInfo)null! })
            .Union(
                type
                    .GetProperties()
                    .Where(p => p.GetCustomAttribute<ScaffoldColumnAttribute>()?.Scaffold != false)
                    .Select(e => new { e.Name, Type = e.PropertyType, FieldInfo = (FieldInfo)null!, PropertyInfo = e })
            )
            .ToList();

        var order = fields.Count;
        foreach (var field in fields)
        {
            var align = Ivy.Align.Left;

            if (field.Type.IsNumeric())
            {
                align = Ivy.Align.Right;
            }

            if (field.Type == typeof(bool))
            {
                align = Ivy.Align.Center;
            }

            var isNavigationCollection = field.Type.IsCollectionType() &&
                field.Type.GetCollectionTypeParameter() is { IsClass: true } elementType &&
                elementType != typeof(string);

            var removed = field.Name.StartsWith("_") && field.Name.Length > 1 && char.IsLetter(field.Name[1]) ||
                          field.Name == "_hiddenKey" ||
                          isNavigationCollection;

            _columns[field.Name] = new InternalColumn()
            {
                Column = new DataTableColumn()
                {
                    Name = field.Name,
                    Header = StringHelper.LabelFor(field.Name, field.Type),
                    ColType = DataTableBuilderHelpers.GetDataTypeHint(field.Type),
                    AlignContent = align,
                    Order = order++
                },
                Removed = removed
            };
        }
    }

    public DataTableBuilder<TModel> Width(Size width)
    {
        _width = width;
        return this;
    }

    public DataTableBuilder<TModel> Height(Size height)
    {
        _height = height;
        return this;
    }

    public DataTableBuilder<TModel> Width(Expression<Func<TModel, object>> field, Size width)
    {
        var column = GetColumn(field);
        column.Column.Width = width;
        return this;
    }

    private InternalColumn GetColumn(Expression<Func<TModel, object>> field)
    {
        var name = TypeHelper.GetNameFromMemberExpression(field.Body);
        var column = _columns[name];

        if (TypeHelper.IsComplexExpression(field.Body))
        {
            var compiled = field.Compile();
            column.Column.ValueAccessor = obj => compiled((TModel)obj);
        }

        return column;
    }

    private InternalColumn GetColumn<TValue>(Expression<Func<TModel, TValue>> field)
    {
        var name = TypeHelper.GetNameFromMemberExpression(field.Body);
        var column = _columns[name];

        if (TypeHelper.IsComplexExpression(field.Body))
        {
            var compiled = field.Compile();
            column.Column.ValueAccessor = obj => compiled((TModel)obj);
        }

        return column;
    }

    public DataTableBuilder<TModel> Header(Expression<Func<TModel, object>> field, string label)
    {
        var column = GetColumn(field);
        column.Column.Header = label;
        return this;
    }

    public DataTableBuilder<TModel> AlignContent(Expression<Func<TModel, object>> field, Align align)
    {
        var column = GetColumn(field);
        column.Column.AlignContent = align;
        return this;
    }

    public DataTableBuilder<TModel> Sortable(Expression<Func<TModel, object>> field, bool sortable)
    {
        var column = GetColumn(field);
        column.Column.Sortable = sortable;
        return this;
    }

    public DataTableBuilder<TModel> Filterable(Expression<Func<TModel, object>> field, bool filterable)
    {
        var column = GetColumn(field);
        column.Column.Filterable = filterable;
        return this;
    }

    public DataTableBuilder<TModel> Icon(Expression<Func<TModel, object>> field, string icon)
    {
        var column = GetColumn(field);
        column.Column.Icon = icon;
        return this;
    }

    public DataTableBuilder<TModel> Help(Expression<Func<TModel, object>> field, string help)
    {
        var column = GetColumn(field);
        column.Column.Help = help;
        return this;
    }

    public DataTableBuilder<TModel> Footer<TValue>(
        Expression<Func<TModel, TValue>> field,
        string label,
        Func<IEnumerable<TValue>, object> aggregateFunc)
    {
        var column = GetColumn(field);
        var selector = field.Compile();
        var values = GetOrCreateFooterValueList(field, selector);
        var result = aggregateFunc(values);
        var footerText = $"{label}: {FormatFooterValue(column.Column, result)}";
        column.Column.Footer ??= [];
        column.Column.Footer.Add(footerText);
        return this;
    }

    public DataTableBuilder<TModel> Footer<TValue>(
        Expression<Func<TModel, TValue>> field,
        IEnumerable<(string Label, Func<IEnumerable<TValue>, object> AggregateFunc)> aggregates)
    {
        var column = GetColumn(field);
        var selector = field.Compile();
        var values = GetOrCreateFooterValueList(field, selector);
        var footerValues = aggregates
            .Select(agg => $"{agg.Label}: {FormatFooterValue(column.Column, agg.AggregateFunc(values))}")
            .ToList();
        column.Column.Footer ??= [];
        column.Column.Footer.AddRange(footerValues);
        return this;
    }

    private static string FormatFooterValue(DataTableColumn column, object value)
    {
        if (value is IFormattable formattable && column.FormatStyle.HasValue)
        {
            var style = column.FormatStyle.Value;
            var precision = column.Precision ?? 2;

            if (style == NumberFormatStyle.Currency || style == NumberFormatStyle.Accounting)
            {
                var currency = column.Currency ?? "USD";
                var culture = GetCultureForCurrency(currency);
                return formattable.ToString($"C{precision}", culture);
            }

            var invariant = CultureInfo.InvariantCulture;
            return style switch
            {
                NumberFormatStyle.Percent => formattable.ToString($"P{precision}", invariant),
                NumberFormatStyle.Decimal => formattable.ToString($"N{precision}", invariant),
                _ => value.ToString() ?? ""
            };
        }

        return value.ToString() ?? "";
    }

    private static CultureInfo GetCultureForCurrency(string isoCurrencyCode)
    {
        try
        {
            foreach (var ci in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
            {
                var region = new RegionInfo(ci.Name);
                if (string.Equals(region.ISOCurrencySymbol, isoCurrencyCode, StringComparison.OrdinalIgnoreCase))
                    return ci;
            }
        }
        catch
        {
            // Fall through to default
        }

        // Default to en-US for unknown currencies
        var fallback = new CultureInfo("en-US");
        fallback.NumberFormat.CurrencySymbol = isoCurrencyCode;
        return fallback;
    }

    private List<TValue> GetOrCreateFooterValueList<TValue>(
        Expression<Func<TModel, TValue>> field,
        Func<TModel, TValue> compiledSelector)
    {
        var columnName = TypeHelper.GetNameFromMemberExpression(field.Body);
        if (_footerValuesByColumn?.TryGetValue(columnName, out var cached) == true && cached is List<TValue> list)
            return list;

        var values = queryable.Select(compiledSelector).ToList();
        _footerValuesByColumn ??= new Dictionary<string, object>(StringComparer.Ordinal);
        _footerValuesByColumn[columnName] = values;
        return values;
    }

    public DataTableBuilder<TModel> Format(Expression<Func<TModel, object>> field, NumberFormatStyle formatStyle, int? precision = null, string? currency = null)
    {
        var column = GetColumn(field);
        column.Column.FormatStyle = formatStyle;
        if (precision.HasValue) column.Column.Precision = precision;
        if (currency != null) column.Column.Currency = currency;
        if ((formatStyle == NumberFormatStyle.Currency || formatStyle == NumberFormatStyle.Accounting) && string.IsNullOrEmpty(column.Column.Currency))
        {
            column.Column.Currency = "USD";
        }
        return this;
    }

    public DataTableBuilder<TModel> Group(Expression<Func<TModel, object>> field, string group)
    {
        var column = GetColumn(field);
        column.Column.Group = group;
        return this;
    }

    public DataTableBuilder<TModel> SortDirection(Expression<Func<TModel, object>> field, SortDirection direction)
    {
        var column = GetColumn(field);
        column.Column.SortDirection = direction;
        return this;
    }

    public DataTableBuilder<TModel> Renderer(Expression<Func<TModel, object>> field, IDataTableColumnRenderer renderer)
    {
        var column = GetColumn(field);
        column.Column.Renderer = renderer;
        column.Column.ColType = renderer.ColType;
        if (renderer is NumberDisplayRenderer numRenderer)
        {
            column.Column.FormatStyle = numRenderer.FormatStyle;
            column.Column.Precision = numRenderer.Precision;
            column.Column.Currency = numRenderer.Currency;
        }

        if (renderer is LabelsDisplayRenderer labelsRenderer)
        {
            column.Column.Color = labelsRenderer.Color;
            column.Column.CustomColor = labelsRenderer.CustomColor;
            column.Column.BadgeColorMapping = labelsRenderer.BadgeColorMapping;
        }

        return this;
    }

    public DataTableBuilder<TModel> Badges(Expression<Func<TModel, object>> field, Colors? color = null)
    {
        var column = GetColumn(field);
        column.Column.ColType = ColType.Labels;
        column.Column.Color = color;
        return this;
    }

    public DataTableBuilder<TModel> Badges(Expression<Func<TModel, object>> field, string customColor)
    {
        var column = GetColumn(field);
        column.Column.ColType = ColType.Labels;
        column.Column.CustomColor = customColor;
        return this;
    }

    public DataTableBuilder<TModel> Badges(Expression<Func<TModel, object>> field, Dictionary<string, Colors> mapping)
    {
        var column = GetColumn(field);
        column.Column.ColType = ColType.Labels;
        column.Column.BadgeColorMapping = mapping.ToDictionary(k => k.Key, v => v.Value.ToString());
        return this;
    }

    public DataTableBuilder<TModel> DataTypeHint(Expression<Func<TModel, object>> field, ColType colType)
    {
        var column = GetColumn(field);
        column.Column.ColType = colType;
        return this;
    }

    public DataTableBuilder<TModel> Order(params Expression<Func<TModel, object>>[] fields)
    {
        int order = 0;
        foreach (var expr in fields)
        {
            var hint = GetColumn(expr);
            hint.Removed = false;
            hint.Column.Order = order++;
        }

        return this;
    }

    public DataTableBuilder<TModel> Remove(params IEnumerable<Expression<Func<TModel, object>>> fields)
    {
        foreach (var field in fields)
        {
            var name = TypeHelper.GetNameFromMemberExpression(field.Body);
            if (!_columns.TryGetValue(name, out var hint)) continue;
            hint.Removed = true;
        }
        return this;
    }

    public DataTableBuilder<TModel> Hidden(params IEnumerable<Expression<Func<TModel, object>>> fields)
    {
        foreach (var field in fields)
        {
            var hint = GetColumn(field);
            hint.Column.Hidden = true;
        }

        return this;
    }

    public DataTableBuilder<TModel> Config(Action<DataTableConfig> config)
    {
        config(_configuration);
        return this;
    }

    public DataTableBuilder<TModel> BatchSize(int batchSize)
    {
        _configuration.BatchSize = batchSize;
        return this;
    }

    public DataTableBuilder<TModel> LoadAllRows(bool loadAll = true)
    {
        _configuration.LoadAllRows = loadAll;
        return this;
    }

    public DataTableBuilder<TModel> OnCellClick(Func<Event<DataTable, CellClickEventArgs>, ValueTask> handler)
    {
        _onCellClick = handler;
        return this;
    }

    public DataTableBuilder<TModel> OnCellActivated(Func<Event<DataTable, CellClickEventArgs>, ValueTask> handler)
    {
        _onCellActivated = handler;
        return this;
    }

    public DataTableBuilder<TModel> RowActions(params MenuItem[] actions)
    {
        _menuItemRowActions = actions;
        return this;
    }

    public DataTableBuilder<TModel> OnRowAction(Func<Event<DataTable, RowActionClickEventArgs>, ValueTask> handler)
    {
        _onRowAction = handler;
        return this;
    }


    public DataTableBuilder<TModel> OnCellAction(Expression<Func<TModel, object>> field, EventHandler<object> action)
    {
        var columnName = TypeHelper.GetNameFromMemberExpression(field.Body);
        _cellActions[columnName] = action;
        return this;
    }

    public DataTableBuilder<TModel> RefreshToken(RefreshToken token)
    {
        _refreshToken = token;
        return this;
    }

    public DataTableBuilder<TModel> Empty(FuncViewBuilder emptyViewFactory)
    {
        _emptyViewFactory = emptyViewFactory;
        return this;
    }

    public DataTableBuilder<TModel> HeaderLeft(FuncViewBuilder factory)
    {
        _headerLeftFactory = factory;
        return this;
    }

    public DataTableBuilder<TModel> HeaderRight(FuncViewBuilder factory)
    {
        _headerRightFactory = factory;
        return this;
    }

    public override object? Build()
    {
        Context.TryUseService<IChatClient>(out var chatClient);

        var columns = _columns.Values.Where(e => !e.Removed).OrderBy(c => c.Column.Order).Select(e => e.Column)
            .ToArray();
        var removedColumns = _columns.Values.Where(e => e.Removed).Select(c => c.Column.Name).ToArray();
        var queryable1 = queryable.RemoveFields(removedColumns);

        // Default to full width if not explicitly set
        var width = _width ?? Size.Full();

        var configuration = _configuration;
        if (chatClient is not null)
        {
            configuration = _configuration with { AllowLlmFiltering = true };
        }

        // Automatically enable cell click events if handlers are provided
        if (_onCellClick != null || _onCellActivated != null || _cellActions.Count > 0)
        {
            configuration = configuration with { EnableCellClickEvents = true };
        }

        // Set ID column name if idSelector was provided
        if (_idColumnName != null)
        {
            configuration = configuration with { IdColumnName = _idColumnName };
        }

        // Wire up cell actions to OnCellClick
        var onCellClick = _onCellClick;
        if (_cellActions.Count > 0)
        {
            var originalHandler = _onCellClick;
            onCellClick = async e =>
            {
                var args = e.Value;
                if (_cellActions.TryGetValue(args.ColumnName, out var action))
                {
                    await action.Invoke(args.CellValue!);
                }

                // Call original handler if it exists
                if (originalHandler != null)
                {
                    await originalHandler(e);
                }
            };
        }

        // Convert idSelector function to work with object instead of TModel
        Func<object, object?>? idSelectorForView = null;
        if (_idSelectorFunc != null)
        {
            idSelectorForView = obj => _idSelectorFunc((TModel)obj);
        }

        return new DataTableView(
            queryable1,
            width,
            _height,
            columns,
            configuration,
            onCellClick: onCellClick,
            onCellActivated: _onCellActivated,
            rowActions: _menuItemRowActions,
            onRowAction: _onRowAction,
            idSelector: idSelectorForView,
            refreshToken: _refreshToken,
            emptyViewFactory: _emptyViewFactory,
            headerLeftFactory: _headerLeftFactory,
            headerRightFactory: _headerRightFactory);
    }

    public object[] GetMemoValues()
    {
        // Memoize based on configuration - if config hasn't changed, don't rebuild
        return [_width!, _height!, _configuration, _refreshToken?.Token!];
    }
}
