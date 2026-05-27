using System.Reflection;
using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy.Views.Builders;

public class NavigationPropertyBuilder<TModel> : IBuilder<TModel>
{
    public object? Build(object? value, TModel record)
    {
        // Nested ToDetails() stores a DetailsBuilder in the model; render it as a view, not as text.
        if (value is IView)
            return value;

        return ResolveDisplayValue(value);
    }

    public static string? ResolveDisplayValue(object? value)
    {
        if (value == null) return null;

        if (value is IView)
            return null;

        var type = value.GetType();

        // Look for Name, Title, or DisplayName properties
        foreach (var propName in new[] { "Name", "Title", "DisplayName" })
        {
            var prop = type.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.PropertyType == typeof(string))
            {
                var result = prop.GetValue(value) as string;
                if (result != null) return result;
            }
        }

        // Check if the type overrides ToString()
        var toStringMethod = type.GetMethod("ToString", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, Type.EmptyTypes);
        if (toStringMethod != null)
        {
            return value.ToString();
        }

        // Look for an Id property (skip ViewBase — Id is the runtime widget id, not a display key)
        if (!typeof(ViewBase).IsAssignableFrom(type))
        {
            var idProp = type.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
            if (idProp != null)
            {
                var idValue = idProp.GetValue(value);
                if (idValue != null) return $"Entity #{idValue}";
            }
        }

        return value.ToString();
    }
}
