using System.Linq.Expressions;
using System.Reflection;
using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Helpers;
using Ivy.Shared;

namespace Ivy.DataTables;

public class GlideDataGridBuilder<TModel> : ViewBase, IStateless
{
    private readonly IQueryable<TModel> _queryable;
    private Size? _width;
    private Size? _height;
    private readonly Dictionary<string, InternalColumn> columns;
    private readonly DataTableConfiguration _configuration = new();

    private class InternalColumn
    {
        public required DataTableColumn Column { get; init; }
        public bool Removed { get; set; }
    }
    
    public GlideDataGridBuilder(IQueryable<TModel> queryable)
    {
        _queryable = queryable;
        columns = [];
        Scaffold();
    }

    private void Scaffold()
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

        int order = fields.Count;
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

            var removed = field.Name.StartsWith('_') && field.Name.Length > 1;

            columns[field.Name] = new InternalColumn()
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

    public GlideDataGridBuilder<TModel> Width(Size width)
    {
        _width = width;
        return this;
    }

    public GlideDataGridBuilder<TModel> Height(Size height)
    {
        _height = height;
        return this;
    }
    
    public GlideDataGridBuilder<TModel> Width(Expression<Func<TModel, object>> field, Size width)
    {
        var column = GetColumn(field);
        column.Column.Width = width;
        return this;
    }
    
    private InternalColumn GetColumn(Expression<Func<TModel, object>> field)
    {
        var name = Utils.GetNameFromMemberExpression(field.Body);
        return columns[name];
    }

    public GlideDataGridBuilder<TModel> Header(Expression<Func<TModel, object>> field, string label)
    {
        var column = GetColumn(field);
        column.Column.Header = label;
        return this;
    }

    public GlideDataGridBuilder<TModel> Align(Expression<Func<TModel, object>> field, Align align)
    {
        var column = GetColumn(field);
        column.Column.Align = align;
        return this;
    }
    
    public GlideDataGridBuilder<TModel> Sortable(Expression<Func<TModel, object>> field, bool sortable)
    {
        var column = GetColumn(field);
        column.Column.Sortable = sortable;
        return this;
    }

    public GlideDataGridBuilder<TModel> Order(params Expression<Func<TModel, object>>[] fields)
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

    public GlideDataGridBuilder<TModel> Hidden(params IEnumerable<Expression<Func<TModel, object>>> fields)
    {
        foreach (var field in fields)
        {
            var hint = GetColumn(field);
            hint.Column.Hidden = true;
        }
        return this;
    }
    
    public GlideDataGridBuilder<TModel> Config(Action<DataTableConfiguration> config)
    {
        config(_configuration);
        return this;
    }

    public override object? Build()
    {
        var columnsArray = columns.Values.Where(e => !e.Removed).OrderBy(c => c.Column.Order).Select(e => e.Column).ToArray();
        var removedColumns = columns.Values.Where(e => e.Removed).Select(c => c.Column.Name).ToArray();
        var queryable = _queryable.RemoveFields(removedColumns);
        return new GlideDataGridView(queryable, _width, _height, columnsArray, _configuration);
    }
}