using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Helpers;
using Ivy.Shared;
using Microsoft.Extensions.AI;

namespace Ivy.Views.DataTables;

public class DataTableBuilder<TModel> : ViewBase, IMemoized
{
    private readonly IQueryable<TModel> _queryable;
    private Size? _width;
    private Size? _height;
    private readonly Dictionary<string, InternalColumn> _columns;
    private readonly DataTableConfig _configuration = new();
    private Func<Event<DataTable, CellClickEventArgs>, ValueTask>? _onCellClick;
    private Func<Event<DataTable, CellClickEventArgs>, ValueTask>? _onCellActivated;
    private RowAction[]? _rowActions;
    private Func<Event<DataTable, RowActionClickEventArgs>, ValueTask>? _onRowAction;

    private class InternalColumn
    {
        public required DataTableColumn Column { get; init; }
        public bool Removed { get; set; }
    }

    public DataTableBuilder(IQueryable<TModel> queryable)
    {
        _queryable = queryable;
        _columns = new Dictionary<string, InternalColumn>();
        _Scaffold();
    }

    /// <summary>
    /// Determines the appropriate DataTypeHint based on the .NET type
    /// </summary>
    private static Ivy.ColType GetDataTypeHint(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        if (underlyingType == typeof(Icons))
            return Ivy.ColType.Icon;

        if (underlyingType == typeof(string) || underlyingType == typeof(char))
            return Ivy.ColType.Text;

        if (underlyingType == typeof(int) || underlyingType == typeof(long) ||
            underlyingType == typeof(short) || underlyingType == typeof(byte) ||
            underlyingType == typeof(uint) || underlyingType == typeof(ulong) ||
            underlyingType == typeof(ushort) || underlyingType == typeof(sbyte))
            return Ivy.ColType.Number;

        if (underlyingType == typeof(decimal) || underlyingType == typeof(double) ||
            underlyingType == typeof(float))
            return Ivy.ColType.Number;

        if (underlyingType == typeof(bool))
            return Ivy.ColType.Boolean;

        if (underlyingType == typeof(DateTime) || underlyingType == typeof(DateTimeOffset))
            return Ivy.ColType.DateTime;

        if (underlyingType == typeof(DateOnly))
            return Ivy.ColType.Date;

        if (underlyingType == typeof(TimeSpan) || underlyingType == typeof(TimeOnly))
            return Ivy.ColType.Text;

        if (underlyingType == typeof(Guid) || underlyingType.IsEnum)
            return Ivy.ColType.Text;

        // Handle string arrays as Labels type
        if (underlyingType.IsArray && underlyingType.GetElementType() == typeof(string))
            return Ivy.ColType.Labels;

        // Handle other arrays and collections as Text
        if (underlyingType.IsArray || typeof(System.Collections.IEnumerable).IsAssignableFrom(underlyingType))
            return Ivy.ColType.Text;

        return Ivy.ColType.Text;
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

        int order = fields.Count();
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

            var removed = field.Name.StartsWith("_") && field.Name.Length > 1 && char.IsLetter(field.Name[1]);

            _columns[field.Name] = new InternalColumn()
            {
                Column = new DataTableColumn()
                {
                    Name = field.Name,
                    Header = Utils.LabelFor(field.Name, field.Type) ?? field.Name,
                    ColType = GetDataTypeHint(field.Type),
                    Align = align,
                    Order = order++
                },
                Removed = removed
            };
        }
    }

    /// <summary>
    /// Sets the overall width of the DataTable. For column-specific widths, use Width(Expression, Size).
    /// </summary>
    /// <param name="width">The desired width for the table.</param>
    /// <returns>The builder for method chaining.</returns>
    public DataTableBuilder<TModel> Width(Size width)
    {
        _width = width;
        return this;
    }

    /// <summary>
    /// Sets the overall height of the DataTable.
    /// </summary>
    /// <param name="height">The desired height for the table.</param>
    /// <returns>The builder for method chaining.</returns>
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
        return this;
    }

    public DataTableBuilder<TModel> DataTypeHint(Expression<Func<TModel, object>> field, Ivy.ColType colType)
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

    public DataTableBuilder<TModel> RowActions(params RowAction[] actions)
    {
        _rowActions = actions;
        return this;
    }

    public DataTableBuilder<TModel> HandleRowAction(Action<TModel, MenuItem> handler)
    {
        //todo: Implement
        return this;
    }

    public DataTableBuilder<TModel> RowActions2(params MenuItem[] actions)
    {
        //todo: Implement and rename to RowActions
        return this;
    }

    public DataTableBuilder<TModel> HandleCellAction(Expression<Func<TModel, object>> field, Action<object> action)
    {
        //todo: Implement
        return this;
    }

    public DataTableBuilder<TModel> OnRowAction(Func<Event<DataTable, RowActionClickEventArgs>, ValueTask> handler)
    {
        //todo: remove
        _onRowAction = handler;
        return this;
    }

    public override object? Build()
    {
        var chatClient = this.UseService<IChatClient?>();

        var columns = _columns.Values.Where(e => !e.Removed).OrderBy(c => c.Column.Order).Select(e => e.Column).ToArray();
        var removedColumns = _columns.Values.Where(e => e.Removed).Select(c => c.Column.Name).ToArray();
        var queryable = _queryable.RemoveFields(removedColumns);

        // Default to full width if not explicitly set
        var width = _width ?? Size.Full();

        var configuration = _configuration;
        if (chatClient is not null)
        {
            configuration = _configuration with { AllowLlmFiltering = true };
        }

        // Automatically enable cell click events if handlers are provided
        if (_onCellClick != null || _onCellActivated != null)
        {
            configuration = configuration with { EnableCellClickEvents = true };
        }

        return new DataTableView(queryable, width, _height, columns, configuration, _onCellClick, _onCellActivated, _rowActions, _onRowAction);
    }

    public object[] GetMemoValues()
    {
        // Memoize based on configuration - if config hasn't changed, don't rebuild
        return [_width!, _height!, _configuration];
    }
}