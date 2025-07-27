using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using Ivy.Builders;
using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Shared;

namespace Ivy.Views.Tables;

public class TableBuilder<TModel> : ViewBase, IStateless
{
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

        public string Header { get; set; } = Utils.SplitPascalCase(name) ?? name;

        public string? Description { get; set; }

        public bool Removed { get; set; } = removed;

        public Align Align { get; set; } = align;

        public Size? Width { get; set; }

        public Func<IEnumerable<TModel>, object>? FooterAggregate { get; set; }

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
    //private Func<TModel, bool> _highlightPredicate;
    private object? _empty;

    public TableBuilder(IEnumerable<TModel> records)
    {
        _records = records;
        _builderFactory = new BuilderFactory<TModel>();
        _columns = new Dictionary<string, TableBuilderColumn>();
        _Scaffold();
    }

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

            if (field.Type == typeof(bool))
            {
                cellAlignment = Shared.Align.Center;
            }

            //todo: 
            // if (field.Type == typeof(Controls.Icon) || field.Type == typeof(Controls.EmptyIcon))
            // {
            //     cellAlignment = CellAlignment.Center;
            // }

            //todo:
            //if (field.Type.IsDate())
            //{
            //    cellRenderer = _rendererFactory.Date();
            //}

            var removed = field.Name.StartsWith("_") && field.Name.Length > 1;

            _columns[field.Name] =
                new TableBuilderColumn(field.Name, order++, cellBuilder, cellAlignment, field.FieldInfo, field.PropertyInfo, removed);
        }
    }

    public TableBuilder<TModel> Width(Size width)
    {
        _width = width;
        return this;
    }

    public TableBuilder<TModel> Width(Expression<Func<TModel, object>> field, Size width)
    {
        var hint = GetField(field);
        hint.Width = width;
        return this;
    }

    private TableBuilderColumn GetField(Expression<Func<TModel, object>> field)
    {
        var name = Utils.GetNameFromMemberExpression(field.Body);
        return _columns[name];
    }

    public TableBuilder<TModel> Builder(Expression<Func<TModel, object>> field, Func<IBuilderFactory<TModel>, IBuilder<TModel>> builder)
    {
        var column = GetField(field);
        column.Builder = builder(_builderFactory);
        return this;
    }

    public TableBuilder<TModel> Builder<TU>(Func<IBuilderFactory<TModel>, IBuilder<TModel>> builder)
    {
        foreach (var column in _columns.Values.Where(e => e.Type is TU))
        {
            column.Builder = builder(_builderFactory);
        }
        return this;
    }

    public TableBuilder<TModel> Description(Expression<Func<TModel, object>> field, string description)
    {
        var column = GetField(field);
        column.Description = description;
        return this;
    }

    public TableBuilder<TModel> Header(Expression<Func<TModel, object>> field, string label)
    {
        var hint = GetField(field);
        hint.Header = label;
        return this;
    }

    public TableBuilder<TModel> Align(Expression<Func<TModel, object>> field, Align align)
    {
        var hint = GetField(field);
        hint.Align = align;
        return this;
    }

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

    public TableBuilder<TModel> Remove(params IEnumerable<Expression<Func<TModel, object>>> fields)
    {
        foreach (var field in fields)
        {
            var hint = GetField(field);
            hint.Removed = true;
        }
        return this;
    }

    public TableBuilder<TModel> Add(Expression<Func<TModel, object>> field)
    {
        var hint = GetField(field);
        hint.Removed = false;
        return this;
    }

    public TableBuilder<TModel> Clear()
    {
        foreach (var field in _columns.Values)
        {
            field.Removed = true;
        }
        return this;
    }

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

    // public TableBuilder<TModel> Row(Action<T, TableRow> rowAction)
    // {
    //     _rowAction = rowAction;
    //     return this;
    // }

    public TableBuilder<TModel> Totals(Expression<Func<TModel, object>> field, Func<IEnumerable<TModel>, object> summaryMethod)
    {
        var hint = GetField(field);
        hint.FooterAggregate = summaryMethod;
        return this;
    }

    public TableBuilder<TModel> Totals(Expression<Func<TModel, object>> field)
    {
        var hint = GetField(field);
        object FooterAggregate(IEnumerable<TModel> rows)
        {
            return rows.Select(e => hint.GetValue(e)).Where(e => e != null).Aggregate((a, b) => (dynamic)a! + (dynamic)b!) ?? 0;
        }
        return Totals(field, FooterAggregate);
    }

    public TableBuilder<TModel> RemoveEmptyColumns()
    {
        _removeEmptyColumns = true;
        return this;
    }

    // public TableBuilder<TModel> HighlightRow(Func<T, bool> predicate)
    // {
    //     _highlightRowPredicate = predicate;
    //     return this;
    // }

    public TableBuilder<TModel> Empty(object content)
    {
        _empty = content;
        return this;
    }

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
            //todo:
            //bool highlight = _highlightRowPredicate?.Invoke(e) ?? false;

            var row = new TableRow(
                columns.Select((f, i) => RenderCell(i, f, f.Builder.Build(f.GetValue(e), e), false, false)).ToArray()
            );

            //todo:
            //if (highlight) row.AddClass("highlight");
            //_rowAction?.Invoke(e, row);

            return row;
        }

        var header = new TableRow(columns.Select((e, i) =>
            RenderHeader(i, e, e.Header == "_" ? "" : e.Header)).ToArray());

        var rows = _records.Select(RenderRow);

        var joinedRows = new[] { header }.Concat(rows).ToArray();

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

public static class TableBuilderFactory
{
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

