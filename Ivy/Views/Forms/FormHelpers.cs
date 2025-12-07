using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Ivy.Services;

namespace Ivy.Views.Forms;

public static class FormHelpers
{
    public static bool IsRequired(PropertyInfo propertyInfo)
    {
        if (propertyInfo.GetCustomAttribute<RequiredAttribute>() != null) return true;
        return IsNonNullableString(propertyInfo);
    }

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

    public static bool CheckForLoadingUploads(object? obj)
    {
        if (obj == null) return false;

        // Check single file upload
        if (obj is IFileUpload file)
            return file.Status == FileUploadStatus.Loading;

        // Check collection of uploads
        if (obj is IEnumerable<IFileUpload> files)
            return files.Any(f => f.Status == FileUploadStatus.Loading);

        // Recursively check all properties
        var type = obj.GetType();

        // Skip primitive types and strings
        if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal) || type == typeof(DateTime) || type == typeof(DateTimeOffset))
            return false;

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            // Skip indexed properties
            if (prop.GetIndexParameters().Length > 0)
                continue;

            try
            {
                var value = prop.GetValue(obj);
                if (CheckForLoadingUploads(value))
                    return true;
            }
            catch
            {
                // Skip properties that can't be read
            }
        }

        // Check fields as well
        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            try
            {
                var value = field.GetValue(obj);
                if (CheckForLoadingUploads(value))
                    return true;
            }
            catch
            {
                // Skip fields that can't be read
            }
        }

        return false;
    }

    public record DisplayInfo(
        string? Name = null,
        string? Description = null,
        string? GroupName = null,
        string? Prompt = null,
        int? Order = null
    );

    public static DisplayInfo GetDisplayInfo(PropertyInfo propertyInfo)
    {
        var displayAttr = propertyInfo.GetCustomAttribute<DisplayAttribute>();
        if (displayAttr == null) return new DisplayInfo();

        return new DisplayInfo(
            Name: displayAttr.Name,
            Description: displayAttr.Description,
            GroupName: displayAttr.GroupName,
            Prompt: displayAttr.Prompt,
            Order: displayAttr.GetOrder()
        );
    }

    public static DisplayInfo GetDisplayInfo(FieldInfo fieldInfo)
    {
        var displayAttr = fieldInfo.GetCustomAttribute<DisplayAttribute>();
        if (displayAttr == null) return new DisplayInfo();

        return new DisplayInfo(
            Name: displayAttr.Name,
            Description: displayAttr.Description,
            GroupName: displayAttr.GroupName,
            Prompt: displayAttr.Prompt,
            Order: displayAttr.GetOrder()
        );
    }

    public record RangeInfo(double? Min, double? Max);

    public static RangeInfo GetRangeInfo(PropertyInfo propertyInfo)
    {
        var rangeAttr = propertyInfo.GetCustomAttribute<RangeAttribute>();
        if (rangeAttr == null) return new(null, null);
        return new RangeInfo(Convert.ToDouble(rangeAttr.Minimum), Convert.ToDouble(rangeAttr.Maximum));
    }

    public static RangeInfo GetRangeInfo(FieldInfo fieldInfo)
    {
        var rangeAttr = fieldInfo.GetCustomAttribute<RangeAttribute>();
        if (rangeAttr == null) return new(null, null);
        return new RangeInfo(Convert.ToDouble(rangeAttr.Minimum), Convert.ToDouble(rangeAttr.Maximum));
    }

    public static int? GetMaxLength(PropertyInfo propertyInfo)
    {
        var maxLengthAttr = propertyInfo.GetCustomAttribute<MaxLengthAttribute>();
        if (maxLengthAttr is { Length: > 0 })
        {
            return maxLengthAttr.Length;
        }

        var stringLengthAttr = propertyInfo.GetCustomAttribute<StringLengthAttribute>();
        if (stringLengthAttr is { MaximumLength: > 0 })
        {
            return stringLengthAttr.MaximumLength;
        }

        var lengthAttr = propertyInfo.GetCustomAttribute<LengthAttribute>();
        if (lengthAttr is { MaximumLength: > 0 })
        {
            return lengthAttr.MaximumLength;
        }

        return null;
    }

    public static int? GetMaxLength(FieldInfo fieldInfo)
    {
        var maxLengthAttr = fieldInfo.GetCustomAttribute<MaxLengthAttribute>();
        if (maxLengthAttr is { Length: > 0 })
        {
            return maxLengthAttr.Length;
        }

        var stringLengthAttr = fieldInfo.GetCustomAttribute<StringLengthAttribute>();
        if (stringLengthAttr is { MaximumLength: > 0 })
        {
            return stringLengthAttr.MaximumLength;
        }

        var lengthAttr = fieldInfo.GetCustomAttribute<LengthAttribute>();
        if (lengthAttr is { MaximumLength: > 0 })
        {
            return lengthAttr.MaximumLength;
        }

        return null;
    }
}


