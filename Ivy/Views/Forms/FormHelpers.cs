using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Ivy.Views.Forms;

/// <summary>Utility methods for determining field requirements during form scaffolding. </summary>
public static class FormHelpers
{
    public static bool IsRequired(PropertyInfo propertyInfo)
    {
        if (propertyInfo.GetCustomAttribute<RequiredAttribute>() != null) return true;
        return IsNonNullableString(propertyInfo);
    }

    /// <summary> Gets all validators from DataAnnotations ValidationAttributes on a property. </summary> 
    public static List<Func<object?, (bool, string)>> GetValidators(PropertyInfo propertyInfo)
    {
        var validators = new List<Func<object?, (bool, string)>>();
        var attributes = propertyInfo.GetCustomAttributes<ValidationAttribute>();

        foreach (var attr in attributes)
        {
            var capturedAttr = attr; // Capture for closure
            validators.Add(value =>
            {
                try
                {
                    var validationContext = new ValidationContext(new { })
                    {
                        MemberName = propertyInfo.Name,
                        DisplayName = propertyInfo.Name
                    };
                    var result = capturedAttr.GetValidationResult(value, validationContext);
                    return result == ValidationResult.Success
                        ? (true, "")
                        : (false, result?.ErrorMessage ?? "Validation failed");
                }
                catch
                {
                    // If validation throws an exception, consider it invalid
                    return (false, "Validation failed");
                }
            });
        }

        return validators;
    }

    /// <summary> Gets all validators from DataAnnotations ValidationAttributes on a field. </summary>
    public static List<Func<object?, (bool, string)>> GetValidators(FieldInfo fieldInfo)
    {
        var validators = new List<Func<object?, (bool, string)>>();
        var attributes = fieldInfo.GetCustomAttributes<ValidationAttribute>();

        foreach (var attr in attributes)
        {
            var capturedAttr = attr; // Capture for closure
            validators.Add(value =>
            {
                try
                {

                    var validationContext = new ValidationContext(new { })
                    {
                        MemberName = fieldInfo.Name,
                        DisplayName = fieldInfo.Name
                    };
                    var result = capturedAttr.GetValidationResult(value, validationContext);
                    return result == ValidationResult.Success
                        ? (true, "")
                        : (false, result?.ErrorMessage ?? "Validation failed");
                }
                catch
                {
                    // If validation throws an exception, consider it invalid
                    return (false, "Validation failed");
                }
            });
        }

        return validators;
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


