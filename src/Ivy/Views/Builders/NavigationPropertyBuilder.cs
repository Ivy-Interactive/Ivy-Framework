using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Ivy.Views.Builders;

public class NavigationPropertyBuilder<TModel> : IBuilder<TModel>
{
    public object? Build(object? value, TModel record)
    {
        return ResolveDisplayValue(value);
    }

    public static string? ResolveDisplayValue(object? value)
    {
        if (value == null) return null;

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

        // Look for an Id property
        var idProp = type.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
        if (idProp != null)
        {
            var idValue = idProp.GetValue(value);
            if (idValue != null) return $"Entity #{idValue}";
        }

        return value.ToString();
    }
}
