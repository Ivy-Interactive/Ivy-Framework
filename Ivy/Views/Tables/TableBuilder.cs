using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Shared;
using Ivy.Views.Builders;

namespace Ivy.Views.Tables;

/// <summary>
/// Fluent builder for creating tables from data collections with automatic column scaffolding.
/// </summary>
/// <typeparam name="TModel">The type of data objects in the table rows.</typeparam>
public class TableBuilder<TModel> : ViewBase, IStateless
{
    /// <summary>
    /// Internal column configuration with metadata and rendering options.
    /// </summary>
    private class TableBuilderColumn(
        string name,
        int order,
        IBuilder<TModel> builder,
        Align align,
        FieldInfo fieldInfo,
        PropertyInfo? propertyInfo,
        bool removed)
    {
        public string Name { get; set; } = name;
        private FieldInfo? FieldInfo { get; set; } = fieldInfo;
        private PropertyInfo? PropertyInfo { get; set; } = propertyInfo;
        public IBuilder<TModel> Builder { get; set; } = builder;
        public Type? Type => FieldInfo?.FieldType ?? PropertyInfo?.PropertyType;
        public int Order { get; set; } = order;
        public string Header { get; set; } = Utils.LabelFor(name, fieldInfo?.FieldType ?? propertyInfo?.PropertyType);
        public string? Description { get; set; }
        public bool Removed { get; set; } = removed;
        public bool IsMultiLine { get; set; }
        public Align Align { get; set; } = align;
        public Size? Width { get; set; }
        public Func<IEnumerable<TModel>, object>? FooterAggregate { get; set; }

        /// <summary>Gets the value of this column from a model object using reflection.</summary>
        public object? GetValue(TModel obj)
        {
            if (obj == null) return null;

            try
            {
                if (FieldInfo != null)
                {
                    return FieldInfo.GetValue(obj);
                }

                if (PropertyInfo != null)
                {
                    return PropertyInfo.GetValue(obj);
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
    }

    private Size? _width;
    private readonly IEnumerable<TModel> _records;
    private readonly Dictionary<string, TableBuilderColumn> _columns;
    private readonly BuilderFactory<TModel> _builderFactory;
    private bool _removeEmptyColumns = false;
    private bool _removeHeader;
    private object? _empty;

    /// <summary>
    /// Creates a table builder with automatic column scaffolding from the model type.
    /// </summary>
    /// <param name="records">The data records to display in the table.</param>
    public TableBuilder(IEnumerable<TModel> records)
    {
        _records = records;
        _builderFactory = new BuilderFactory<TModel>();
        _columns = new Dictionary<string, TableBuilderColumn>();
        _Scaffold();
    }

    /// <summary>
    /// Automatically discovers columns from model properties and fields with intelligent defaults.
    /// </summary>
    private void _Scaffold()
    {
        var type = typeof(TModel);

        var fields = type
            .GetFields()
            .Select(e => new { e.Name, Type = e.FieldType, FieldInfo = e, PropertyInfo = (PropertyInfo)null! })
            .Union(
                type
                    .GetProperties()
                    .Select(e => new { e.Name, Type = e.PropertyType, FieldInfo = (FieldInfo)null!, PropertyInfo = e })
            )
            .ToList();

        int order = fields.Count();
        foreach (var field in fields)
        {
            var cellAlignment = Shared.Align.Left;

            var cellBuilder = _builderFactory.Default();

            if (field.Type.IsNumeric())
            {
                cellAlignment = Shared.Align.Right;
            }

            else if (field.Type == typeof(bool))
            {
                cellAlignment = Shared.Align.Center;
            }

            else if (
                (field.Name.EndsWith("url", StringComparison.OrdinalIgnoreCase) || field.Name.EndsWith("link", StringComparison.OrdinalIgnoreCase))
                    && (field.Type == typeof(string) || field.Type == typeof(Uri)))
            {
                cellBuilder = _builderFactory.Link();
            }

            var removed = field.Name.StartsWith("_") && field.Name.Length > 1;

            _columns[field.Name] =
                new TableBuilderColumn(field.Name, order++, cellBuilder, cellAlignment, field.FieldInfo, field.PropertyInfo, removed);
        }
    }

    /// <summary>Sets the overall table width.</summary>
    /// <param name="width">The width to set for the entire table.</param>
    public TableBuilder<TModel> Width(Size width)
    {
        _width = width;
        return this;
    }

    /// <summary>Sets the width of a specific column.</summary>
    /// <param name="field">Expression identifying the column to set width for.</param>
    /// <param name="width">The width to set for the column.</param>
    public TableBuilder<TModel> Width(Expression<Func<TModel, object>> field, Size width)
    {
        var hint = GetField(field);
        hint.Width = width;
        return this;
    }

    /// <summary>Gets the column configuration for a field expression.</summary>
    /// <param name="field">Expression identifying the field to get configuration for.</param>
    private TableBuilderColumn GetField(Expression<Func<TModel, object>> field)
    {
        var name = Utils.GetNameFromMemberExpression(field.Body);
        return _columns[name];
    }

    /// <summary>Sets a custom builder for rendering a specific column's data.</summary>
    /// <param name="field">Expression identifying the column to configure.</param>
    /// <param name="builder">Factory function to create the column builder.</param>
    public TableBuilder<TModel> Builder(Expression<Func<TModel, object>> field, Func<IBuilderFactory<TModel>, IBuilder<TModel>> builder)
    {
        var column = GetField(field);
        column.Builder = builder(_builderFactory);
        return this;
    }

    /// <summary>Sets the builder for all columns of a specific type.</summary>
    /// <param name="builder">Factory function to create the builder for all matching columns.</param>
    public TableBuilder<TModel> Builder<TU>(Func<IBuilderFactory<TModel>, IBuilder<TModel>> builder)
    {
        foreach (var column in _columns.Values.Where(e => e.Type is TU))
        {
            column.Builder = builder(_builderFactory);
        }
        return this;
    }

    /// <summary>Sets the description for a column.</summary>
    /// <param name="field">Expression identifying the column to set description for.</param>
    /// <param name="description">The description text for the column.</param>
    public TableBuilder<TModel> Description(Expression<Func<TModel, object>> field, string description)
    {
        var column = GetField(field);
        column.Description = description;
        return this;
    }

    /// <summary>Sets a custom header text for a column.</summary>
    /// <param name="field">Expression identifying the column to set header for.</param>
    /// <param name="label">The header text to display.</param>
    public TableBuilder<TModel> Header(Expression<Func<TModel, object>> field, string label)
    {
        var hint = GetField(field);
        hint.Header = label;
        return this;
    }

    /// <summary>Sets the text alignment for a column.</summary>
    /// <param name="field">Expression identifying the column to set alignment for.</param>
    /// <param name="align">The text alignment to apply.</param>
    public TableBuilder<TModel> Align(Expression<Func<TModel, object>> field, Align align)
    {
        var hint = GetField(field);
        hint.Align = align;
        return this;
    }

    /// <summary>Sets the display order of columns.</summary>
    /// <param name="fields">Expressions identifying the columns in desired order.</param>
    public TableBuilder<TModel> Order(params Expression<Func<TModel, object>>[] fields)
    {
        int order = 0;
        foreach (var expr in fields)
        {
            var hint = GetField(expr);
            hint.Removed = false;
            hint.Order = order++;
        }
        return this;
    }

    /// <summary>Removes columns from the table.</summary>
    /// <param name="fields">Expressions identifying the columns to remove.</param>
    public TableBuilder<TModel> Remove(params IEnumerable<Expression<Func<TModel, object>>> fields)
    {
        foreach (var field in fields)
        {
            var hint = GetField(field);
            hint.Removed = true;
        }
        return this;
    }

    public TableBuilder<TModel> MultiLine(params Expression<Func<TModel, object>>[] fields)
    {
        foreach (var field in fields)
        {
            var hint = GetField(field);
            hint.IsMultiLine = true;
        }
        return this;
    }


    /// <summary>Adds a previously removed column back to the table.</summary>
    /// <param name="field">Expression identifying the column to add back.</param>
    public TableBuilder<TModel> Add(Expression<Func<TModel, object>> field)
    {
        var hint = GetField(field);
        hint.Removed = false;
        return this;
    }

    /// <summary>Removes all columns from the table.</summary>
    public TableBuilder<TModel> Clear()
    {
        foreach (var field in _columns.Values)
        {
            field.Removed = true;
        }
        return this;
    }

    /// <summary>Resets all columns to default settings.</summary>
    public TableBuilder<TModel> Reset()
    {
        foreach (var field in _columns.Values)
        {
            field.Removed = false;
            field.Align = Shared.Align.Left;
            field.Builder = _builderFactory.Text();
        }
        return this;
    }

    /// <summary>Adds a footer with custom aggregate calculation for a column.</summary>
    /// <param name="field">Expression identifying the column to add footer to.</param>
    /// <param name="summaryMethod">Function to calculate the footer value from all records.</param>
    public TableBuilder<TModel> Totals(Expression<Func<TModel, object>> field, Func<IEnumerable<TModel>, object> summaryMethod)
    {
        var hint = GetField(field);
        hint.FooterAggregate = summaryMethod;
        return this;
    }

    /// <summary>Adds a footer with automatic sum calculation for a column.</summary>
    /// <param name="field">Expression identifying the column to add sum footer to.</param>
    public TableBuilder<TModel> Totals(Expression<Func<TModel, object>> field)
    {
        var hint = GetField(field);
        object FooterAggregate(IEnumerable<TModel> rows)
        {
            return rows.Select(e => hint.GetValue(e)).Where(e => e != null).Aggregate((a, b) => (dynamic)a! + (dynamic)b!) ?? 0;
        }
        return Totals(field, FooterAggregate);
    }

    /// <summary>Removes the table header row.</summary>
    public TableBuilder<TModel> RemoveHeader()
    {
        _removeHeader = true;
        return this;
    }

    /// <summary>Automatically removes columns that contain only empty values.</summary>
    public TableBuilder<TModel> RemoveEmptyColumns()
    {
        _removeEmptyColumns = true;
        return this;
    }

    /// <summary>Sets content to display when the table has no data.</summary>
    /// <param name="content">The content to display for empty tables.</param>
    public TableBuilder<TModel> Empty(object content)
    {
        _empty = content;
        return this;
    }

    /// <summary>
    /// Builds the complete table with headers, data rows, and optional footers.
    /// </summary>
    public override object? Build()
    {
        if (!_records.Any()) return _empty!;

        bool[] isEmptyColumn = Enumerable.Repeat(true, _columns.Values.Count(e => !e.Removed)).ToArray();

        Table RenderTable(TableRow[] tableRows)
        {
            var table = new Table(tableRows).Width(_width);
            return table;
        }

        TableCell RenderCell(int index, TableBuilderColumn column, object? content, bool isHeader, bool isFooter)
        {
            var cell = new TableCell(content).IsHeader(isHeader).IsFooter(isFooter).Align(column.Align);

            if (column.IsMultiLine)
            {
                cell = cell.MultiLine(true);
            }

            if (isHeader)
            {
                cell = cell.Width(column.Width);
            }

            if (!isHeader && isEmptyColumn[index])
            {
                if (!Utils.IsEmptyContent(content))
                {
                    isEmptyColumn[index] = false;
                }
            }

            return cell;
        }

        TableCell RenderHeader(int index, TableBuilderColumn column, object content)
        {
            var cell = RenderCell(index, column, content, true, false);
            return cell;
        }

        TableCell RenderFooter(int index, TableBuilderColumn column, object content)
        {
            var cell = RenderCell(index, column, content, false, true);
            return cell;
        }

        var columns = _columns.Values.Where(e => !e.Removed)
            .OrderBy(e => e.Order).ToList();

        TableRow RenderRow(TModel e)

        {
            var row = new TableRow(
                columns.Select((f, i) => RenderCell(i, f, f.Builder.Build(f.GetValue(e), e), false, false)).ToArray()
            );

            return row;
        }

        var header = !_removeHeader
            ? new TableRow(columns.Select((e, i) =>
                RenderHeader(i, e, e.Header == "_" ? "" : e.Header)).ToArray())
            : null;

        var rows = _records.Select(RenderRow);

        var joinedRows = header != null ? new[] { header }.Concat(rows).ToArray() : rows.ToArray();

        if (columns.Any(e => e.FooterAggregate != null))
        {
            var footer = new TableRow(columns.Select((e, i) =>
                RenderFooter(i, e, e.FooterAggregate?.Invoke(_records)!)).ToArray());
            joinedRows = joinedRows.Concat([footer]).ToArray();
        }

        if (_removeEmptyColumns && isEmptyColumn.Any(e => e))
        {
            var indexes = isEmptyColumn.Select((e, i) => (e, i)).Where(x => x.e).Select(x => x.i).Reverse().ToArray();
            joinedRows = joinedRows.Select(e => e with { Children = e.Children.Where((_, i) => !indexes.Contains(i)).ToArray() }).ToArray();
        }

        return RenderTable(joinedRows);
    }
}

/// <summary>
/// Factory for creating table builders from generic collections.
/// </summary>
public static class TableBuilderFactory
{
    /// <summary>
    /// Creates a table view from any enumerable collection with automatic type detection.
    /// </summary>
    /// <param name="enumerable">The collection to create a table from.</param>
    public static ViewBase FromEnumerable(IEnumerable enumerable)
    {
        var enumerableType = enumerable.GetType()
            .GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

        if (enumerableType != null)
        {
            Type itemType = enumerableType.GetGenericArguments()[0];
            if (Utils.IsSimpleType(itemType))
            {
                var items = enumerable.Cast<object>().ToArray();
                var rows = items.Select(item => new TableRow(new TableCell(item))).ToArray();
                return new WrapperView(new Table(rows));
            }

            Type tableBuilderType = typeof(TableBuilder<>).MakeGenericType(itemType);
            object tableBuilderInstance = Activator.CreateInstance(tableBuilderType, [enumerable])!;
            return (ViewBase)tableBuilderInstance;
        }
        throw new NotImplementedException("Non-generic IEnumerable is not implemented.");
    }
}

