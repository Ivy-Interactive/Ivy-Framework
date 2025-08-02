using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using Ivy.Builders;
using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Helpers;
using Ivy.Shared;
using Microsoft.AspNetCore.Hosting;

namespace Ivy.DataTables;

public class DataTableBuilder<TModel> : ViewBase, IStateless
{
    private readonly IQueryable<TModel> _queryable;
    private Size? _width;
    private Size? _height;
    private readonly Dictionary<string, InternalColumn> _columns;
    private readonly DataTableConfiguration _configuration = new();

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
            var align = Shared.Align.Left;

            if (field.Type.IsNumeric())
            {
                align = Shared.Align.Right;
            }

            if (field.Type == typeof(bool))
            {
                align = Shared.Align.Center;
            }

            var removed = field.Name.StartsWith("_") && field.Name.Length > 1;

            _columns[field.Name] = new InternalColumn()
            {
                Column = new DataTableColumn()
                {
                    Name = field.Name,
                    Header = Utils.SplitPascalCase(field.Name) ?? field.Name,
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
    
    public DataTableBuilder<TModel> Config(Action<DataTableConfiguration> config)
    {
        config(_configuration);
        return this;
    }

    public override object? Build()
    {
        var columns = _columns.Values.Where(e => !e.Removed).OrderBy(c => c.Column.Order).Select(e => e.Column).ToArray();
        var removedColumns = _columns.Values.Where(e => e.Removed).Select(c => c.Column.Name).ToArray();
        var queryable = _queryable.RemoveFields(removedColumns);
        return new DataTableView(queryable, _width, _height, columns, _configuration);
    }
}