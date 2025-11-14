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
    private Action<TModel, MenuItem>? _handleRowAction;
    private MenuItem[]? _menuItemRowActions;
    private readonly Dictionary<string, Action<object>> _cellActions = new();

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

    /// <summary>Sets the event handler for cell clicks (single-click).</summary>
    public DataTableBuilder<TModel> OnCellClick(Func<Event<DataTable, CellClickEventArgs>, ValueTask> handler)
    {
        _onCellClick = handler;
        return this;
    }

    /// <summary>Sets the event handler for cell activation (double-click).</summary>
    public DataTableBuilder<TModel> OnCellActivated(Func<Event<DataTable, CellClickEventArgs>, ValueTask> handler)
    {
        _onCellActivated = handler;
        return this;
    }

    /// <summary>Configures row action menu items that appear on hover using MenuItem structure.</summary>
    /// <param name="actions">Menu items to display as row actions.</param>
    public DataTableBuilder<TModel> RowActions(params MenuItem[] actions)
    {
        _menuItemRowActions = actions;
        return this;
    }

    /// <summary>Sets the event handler for row action menu item selections using MenuItem-based actions.</summary>
    /// <param name="handler">Handler that receives the row model and the selected menu item.</param>
    public DataTableBuilder<TModel> HandleRowAction(Action<TModel, MenuItem> handler)
    {
        _handleRowAction = handler;
        return this;
    }

    /// <summary>Attaches an action handler to a specific cell/column that will be invoked when the cell is interacted with.</summary>
    /// <param name="field">Expression identifying the field/column.</param>
    /// <param name="action">Action to execute when the cell is activated, receives the cell value.</param>
    public DataTableBuilder<TModel> HandleCellAction(Expression<Func<TModel, object>> field, Action<object> action)
    {
        var columnName = Utils.GetNameFromMemberExpression(field.Body);
        _cellActions[columnName] = action;
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
        if (_onCellClick != null || _onCellActivated != null || _cellActions.Count > 0)
        {
            configuration = configuration with { EnableCellClickEvents = true };
        }

        // Convert MenuItem[] row actions to RowAction[]
        RowAction[]? rowActions = null;
        if (_menuItemRowActions != null && _menuItemRowActions.Length > 0)
        {
            rowActions = _menuItemRowActions.Select(ConvertMenuItemToRowAction).ToArray();
        }

        // Helper method to recursively convert MenuItem tree to RowAction tree
        static RowAction ConvertMenuItemToRowAction(MenuItem menuItem)
        {
            return new RowAction
            {
                Id = menuItem.Tag?.ToString() ?? menuItem.Label ?? "",
                Icon = menuItem.Icon?.ToString() ?? "",
                EventName = menuItem.Tag?.ToString() ?? menuItem.Label ?? "",
                Tooltip = menuItem.Tooltip,
                Label = menuItem.Label,
                Children = menuItem.Children?.Where(c => c.Variant != MenuItemVariant.Separator)
                    .Select(ConvertMenuItemToRowAction).ToArray()
            };
        }

        // Create wrapper for HandleRowAction
        Func<Event<DataTable, RowActionClickEventArgs>, ValueTask>? onRowAction = null;
        if (_handleRowAction != null)
        {
            // Capture logger and queryable list during Build (can only access Context here)
            var logger = this.UseService<Microsoft.Extensions.Logging.ILogger<DataTableBuilder<TModel>>?>();

            // Materialize the queryable to a list so we can look up items by index
            // This is more reliable than deserializing corrupted frontend data
            var dataList = _queryable.ToList();

            onRowAction = async (Event<DataTable, RowActionClickEventArgs> e) =>
            {
                var args = e.Value;

                try
                {
                    // Try to get the Id from RowData to look up the actual model instance
                    if (args.RowData.TryGetValue("Id", out var idValue))
                    {
                        // Find the Id property on TModel
                        var idProperty = typeof(TModel).GetProperty("Id");
                        TModel? modelInstance = default;

                        if (idProperty != null)
                        {
                            // Handle JsonElement - need to extract the actual value first
                            object? rawId = idValue;
                            if (idValue is System.Text.Json.JsonElement jsonElement)
                            {
                                // Extract the value based on the property type
                                if (idProperty.PropertyType == typeof(int))
                                {
                                    rawId = jsonElement.GetInt32();
                                }
                                else if (idProperty.PropertyType == typeof(long))
                                {
                                    rawId = jsonElement.GetInt64();
                                }
                                else if (idProperty.PropertyType == typeof(string))
                                {
                                    rawId = jsonElement.GetString();
                                }
                                else if (idProperty.PropertyType == typeof(Guid))
                                {
                                    rawId = jsonElement.GetGuid();
                                }
                                else
                                {
                                    // Fallback: try to convert the string representation
                                    rawId = Convert.ChangeType(jsonElement.ToString(), idProperty.PropertyType);
                                }
                            }

                            // Convert the value to the correct type if needed
                            var convertedId = rawId != null && rawId.GetType() == idProperty.PropertyType
                                ? rawId
                                : Convert.ChangeType(rawId, idProperty.PropertyType);

                            // Find the item in the list with matching Id
                            modelInstance = dataList.FirstOrDefault(item =>
                            {
                                var itemId = idProperty.GetValue(item);
                                return itemId?.Equals(convertedId) == true;
                            });
                        }

                        if (modelInstance == null)
                        {
                            if (logger != null)
                            {
                                Microsoft.Extensions.Logging.LoggerExtensions.LogWarning(logger, "Could not find model instance with Id: {Id}", idValue);
                            }
                            return;
                        }

                        // Find the matching MenuItem
                        var matchingMenuItem = _menuItemRowActions?.FirstOrDefault(m =>
                            m.Tag?.ToString() == args.ActionId || m.Label == args.ActionId);

                        if (matchingMenuItem != null)
                        {
                            _handleRowAction(modelInstance, matchingMenuItem);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    // Log the error but don't crash the app
                    if (logger != null)
                    {
                        Microsoft.Extensions.Logging.LoggerExtensions.LogError(logger, ex, "Failed to handle row action for {ModelType}", typeof(TModel).Name);
                    }
                }

                await ValueTask.CompletedTask;
            };
        }

        // Wire up cell actions to OnCellActivated
        Func<Event<DataTable, CellClickEventArgs>, ValueTask>? onCellActivated = _onCellActivated;
        if (_cellActions.Count > 0)
        {
            var originalHandler = _onCellActivated;
            onCellActivated = async (Event<DataTable, CellClickEventArgs> e) =>
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

        return new DataTableView(queryable, width, _height, columns, configuration, _onCellClick, onCellActivated, rowActions, onRowAction);
    }

    public object[] GetMemoValues()
    {
        // Memoize based on configuration - if config hasn't changed, don't rebuild
        return [_width!, _height!, _configuration];
    }
}