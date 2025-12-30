using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using Ivy.Core;
using Ivy.Helpers;
using Ivy.Shared;
using Microsoft.Extensions.AI;

namespace Ivy.Views.DataTables;

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
    private readonly Dictionary<string, Action<object>> _cellActions = [];

    private readonly string? _idColumnName =
        idSelector != null ? Utils.GetNameFromMemberExpression(idSelector.Body) : null;

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
            var align = Shared.Align.Left;

            if (field.Type.IsNumeric())
            {
                align = Shared.Align.Right;
            }

            if (field.Type == typeof(bool))
            {
                align = Shared.Align.Center;
            }

            var removed = field.Name.StartsWith($"_") && field.Name.Length > 1 && char.IsLetter(field.Name[1]) ||
                          field.Name == "_hiddenKey";

            _columns[field.Name] = new InternalColumn()
            {
                Column = new DataTableColumn()
                {
                    Name = field.Name,
                    Header = Utils.LabelFor(field.Name, field.Type),
                    ColType = DataTableBuilderHelpers.GetDataTypeHint(field.Type),
                    Align = align,
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
        var name = Utils.GetNameFromMemberExpression(field.Body);
        return _columns[name];
    }

    public DataTableBuilder<TModel> Header(Expression<Func<TModel, object>> field, string label)
    {
        var column = GetColumn(field);
        column.Column.Header = label;
        return this;
    }

    public DataTableBuilder<TModel> Align(Expression<Func<TModel, object>> field, Align align)
    {
        var column = GetColumn(field);
        column.Column.Align = align;
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

    public DataTableBuilder<TModel> HandleRowAction(Func<Event<DataTable, RowActionClickEventArgs>, ValueTask> handler)
    {
        _onRowAction = handler;
        return this;
    }

    public DataTableBuilder<TModel> HandleCellAction(Expression<Func<TModel, object>> field, Action<object> action)
    {
        var columnName = Utils.GetNameFromMemberExpression(field.Body);
        _cellActions[columnName] = action;
        return this;
    }

    public override object? Build()
    {
        var chatClient = UseService<IChatClient?>();

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

        // Wire up cell actions to OnCellActivated
        var onCellActivated = _onCellActivated;
        if (_cellActions.Count > 0)
        {
            var originalHandler = _onCellActivated;
            onCellActivated = async e =>
            {
                var args = e.Value;
                if (_cellActions.TryGetValue(args.ColumnName, out var action))
                {
                    action(args.CellValue!);
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

        return new DataTableView(queryable1, width, _height, columns, configuration, _onCellClick, onCellActivated,
            _menuItemRowActions, _onRowAction, idSelectorForView);
    }

    public object[] GetMemoValues()
    {
        // Memoize based on configuration - if config hasn't changed, don't rebuild
        return [_width!, _height!, _configuration];
    }
}