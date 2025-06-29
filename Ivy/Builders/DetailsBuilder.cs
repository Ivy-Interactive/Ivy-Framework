using System.Linq.Expressions;
using System.Reflection;
using Ivy.Core;
using Ivy.Core.Hooks;

namespace Ivy.Builders;

public class DetailsBuilder<TModel> : ViewBase, IStateless
{
    private class Item(string label, IBuilder<TModel> builder, FieldInfo fieldInfo,
        PropertyInfo? propertyInfo)
    {
        public string Label { get; set; } = label;

        public bool IsRemoved { get; set; }

        public bool IsMultiLine { get; set; }

        public IBuilder<TModel> Builder { get; set; } = builder;

        public Type? Type => FieldInfo?.FieldType ?? PropertyInfo?.PropertyType;

        private FieldInfo? FieldInfo { get; set; } = fieldInfo;

        private PropertyInfo? PropertyInfo { get; set; } = propertyInfo;

        public object? GetValue(TModel obj)
        {
            if (obj == null) return null;

            if (FieldInfo != null)
            {
                return FieldInfo.GetValue(obj);
            }

            if (PropertyInfo != null)
            {
                return PropertyInfo.GetValue(obj);
            }

            throw new InvalidOperationException("Both FieldInfo and PropertyInfo are null.");
        }
    }

    private readonly IBuilderFactory<TModel> _builderFactory = new BuilderFactory<TModel>();
    private bool _removeEmpty;
    private readonly Dictionary<string, Item> _items;
    private readonly TModel _model;

    public DetailsBuilder(TModel model)
    {
        _model = model;
        _items = new Dictionary<string, Item>();
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

        foreach (var field in fields)
        {
            _items[field.Name] =
                new Item(
                    Utils.SplitPascalCase(field.Name) ?? "",
                    new DefaultBuilder<TModel>(),
                    field.FieldInfo,
                    field.PropertyInfo
                );
        }

    }

    public DetailsBuilder<TModel> RemoveEmpty()
    {
        _removeEmpty = true;
        return this;
    }

    public DetailsBuilder<TModel> Remove(params Expression<Func<TModel, object>>[] fields)
    {
        foreach (var expr in fields)
        {
            var prop = GetField(expr);
            prop.IsRemoved = true;
        }
        return this;
    }

    public DetailsBuilder<TModel> MultiLine(params Expression<Func<TModel, object>>[] fields)
    {
        foreach (var expr in fields)
        {
            var prop = GetField(expr);
            prop.IsMultiLine = true;
        }
        return this;
    }

    public DetailsBuilder<TModel> Builder(Expression<Func<TModel, object>> field, Func<IBuilderFactory<TModel>, IBuilder<TModel>> builder)
    {
        var column = GetField(field);
        column.Builder = builder(_builderFactory);
        return this;
    }

    public DetailsBuilder<TModel> Builder<TU>(Func<IBuilderFactory<TModel>, IBuilder<TModel>> builder)
    {
        foreach (var column in _items.Values.Where(e => e.Type is TU))
        {
            column.Builder = builder(_builderFactory);
        }
        return this;
    }

    private Item GetField<TU>(Expression<Func<TModel, TU>> field)
    {
        var name = Utils.GetNameFromMemberExpression(field.Body);
        return _items[name];
    }

    public override object? Build()
    {
        var items = _items.Values.Where(e => !e.IsRemoved).ToArray();

        if (_removeEmpty)
        {
            items = items.Where(e => !Utils.IsEmptyContent(e.GetValue(_model))).ToArray();
        }

        return new Details(items.Select(BuildDetail).ToArray());

        Detail BuildDetail(Item item)
        {
            return new Detail(item.Label, item.Builder.Build(item.GetValue(_model), _model), item.IsMultiLine);
        }
    }
}

public static class DetailsBuilderExtensions
{
    public static DetailsBuilder<TModel> ToDetails<TModel>(this TModel model)
    {
        return new DetailsBuilder<TModel>(model);
    }

    public static DetailsBuilder<TModel> ToDetails<TModel>(this IState<TModel> model)
    {
        return new DetailsBuilder<TModel>(model.Value);
    }
}