using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Ivy.Forms;

public static class FormHelpers
{
    public static bool IsRequired(PropertyInfo propertyInfo)
    {
        if (propertyInfo.GetCustomAttribute<RequiredAttribute>() != null) return true;
        return IsNonNullableString(propertyInfo);
    }

    private static bool IsNonNullableString(PropertyInfo propertyInfo)
    {
        if (propertyInfo.PropertyType != typeof(string)) return false;
        var nullabilityContext = new NullabilityInfoContext();
        var nullabilityInfo = nullabilityContext.Create(propertyInfo);
        return nullabilityInfo.ReadState != NullabilityState.Nullable;
    }

    public static bool IsRequired(FieldInfo fieldInfo)
    {
        if (fieldInfo.GetCustomAttribute<RequiredAttribute>() != null) return true;
        return IsNonNullableString(fieldInfo);
    }

    private static bool IsNonNullableString(FieldInfo fieldInfo)
    {
        if (fieldInfo.FieldType != typeof(string)) return false;
        var nullabilityContext = new NullabilityInfoContext();
        var nullabilityInfo = nullabilityContext.Create(fieldInfo);
        return nullabilityInfo.ReadState != NullabilityState.Nullable;
    }
}


