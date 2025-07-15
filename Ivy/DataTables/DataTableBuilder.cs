using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using Ivy.Builders;
using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Shared;
using Microsoft.AspNetCore.Hosting;

namespace Ivy.DataTables;

public class DataTableBuilder<TModel> : ViewBase, IStateless
{
    private readonly IQueryable<TModel> _queryable;
    private Size? _width;
    private readonly Dictionary<string, DataTableColumn> _columns;

    public DataTableBuilder(IQueryable<TModel> queryable)
    {
        _queryable = queryable;
        _columns = new Dictionary<string, DataTableColumn>();
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

            _columns[field.Name] =
                new DataTableColumn()
                {
                    Name = field.Name,
                    Header = Utils.SplitPascalCase(field.Name) ?? field.Name,
                    Align = align,
                    Hidden = removed, //todo: this should be Removed instead
                    Order = order++
                };
        }
    }

    public DataTableBuilder<TModel> Width(Size width)
    {
        _width = width;
        return this;
    }

    public DataTableBuilder<TModel> Width(Expression<Func<TModel, object>> field, Size width)
    {
        var column = GetColumn(field);
        column.Width = width;
        return this;
    }

    private DataTableColumn GetColumn(Expression<Func<TModel, object>> field)
    {
        var name = Utils.GetNameFromMemberExpression(field.Body);
        return _columns[name];
    }

    public DataTableBuilder<TModel> Header(Expression<Func<TModel, object>> field, string label)
    {
        var column = GetColumn(field);
        column.Header = label;
        return this;
    }

    public DataTableBuilder<TModel> Align(Expression<Func<TModel, object>> field, Align align)
    {
        var column = GetColumn(field);
        column.Align = align;
        return this;
    }

    public DataTableBuilder<TModel> Order(params Expression<Func<TModel, object>>[] fields)
    {
        int order = 0;
        foreach (var expr in fields)
        {
            var hint = GetColumn(expr);
            //hint.Removed = false; //todo
            hint.Order = order++;
        }
        return this;
    }

    public DataTableBuilder<TModel> Hidden(params IEnumerable<Expression<Func<TModel, object>>> fields)
    {
        foreach (var field in fields)
        {
            var hint = GetColumn(field);
            hint.Hidden = true;
        }
        return this;
    }

    public override object? Build()
    {
        return new DataTableView<TModel>(_queryable, _width, _columns.Values.OrderBy(c => c.Order).ToArray());
    }
}