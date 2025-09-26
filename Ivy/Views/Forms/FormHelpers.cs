using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Ivy.Views.Forms;

/// <summary>
/// Utility methods for determining field requirements during form scaffolding.
/// </summary>
public static class FormHelpers
{
    /// <summary>
    /// Determines if a property should be required based on [Required] attribute or non-nullable string type.
    /// </summary>
    /// <param name="propertyInfo">The property to analyze.</param>
    /// <returns>True if the property should be required.</returns>
    public static bool IsRequired(PropertyInfo propertyInfo)
    {
        if (propertyInfo.GetCustomAttribute<RequiredAttribute>() != null) return true;
        return IsNonNullableString(propertyInfo);
    }

    /// <summary>
    /// Checks if a property is a non-nullable string using nullability context.
    /// </summary>
    private static bool IsNonNullableString(PropertyInfo propertyInfo)
    {
        if (propertyInfo.PropertyType != typeof(string)) return false;
        var nullabilityContext = new NullabilityInfoContext();
        var nullabilityInfo = nullabilityContext.Create(propertyInfo);
        return nullabilityInfo.ReadState != NullabilityState.Nullable;
    }

    /// <summary>
    /// Determines if a field should be required based on [Required] attribute or non-nullable string type.
    /// </summary>
    /// <param name="fieldInfo">The field to analyze.</param>
    /// <returns>True if the field should be required.</returns>
    public static bool IsRequired(FieldInfo fieldInfo)
    {
        if (fieldInfo.GetCustomAttribute<RequiredAttribute>() != null) return true;
        return IsNonNullableString(fieldInfo);
    }

    /// <summary>
    /// Checks if a field is a non-nullable string using nullability context.
    /// </summary>
    private static bool IsNonNullableString(FieldInfo fieldInfo)
    {
        if (fieldInfo.FieldType != typeof(string)) return false;
        var nullabilityContext = new NullabilityInfoContext();
        var nullabilityInfo = nullabilityContext.Create(fieldInfo);
        return nullabilityInfo.ReadState != NullabilityState.Nullable;
    }
}


