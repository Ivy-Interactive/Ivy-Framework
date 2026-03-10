using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Ivy.Core.Hooks;

// ReSharper disable once CheckNamespace
namespace Ivy;

internal static class FormScaffolder
{
    public static Dictionary<string, FormBuilderField<TModel>> ScaffoldFields<TModel>(Type modelType)
    {
        var fields = GetFieldsAndProperties(modelType);
        var scaffoldedFields = new Dictionary<string, FormBuilderField<TModel>>();

        foreach (var field in fields)
        {
            var displayInfo = field.GetDisplayInfo();

            var label = displayInfo.Name ?? Utils.LabelFor(field.Name, field.Type);

            if (ShouldTrimIdSuffix(displayInfo.Name, label))
            {
                label = label[..^3];
            }

            var order = displayInfo.Order ?? int.MaxValue;

            var factory = ScaffoldInputFactory(field);

            Func<IAnyState, IViewContext, IAnyInput>? wrappedFactory = factory != null
                ? (state, _) => factory(state)
                : null;

            var scaffoldedField = new FormBuilderField<TModel>(
                field.Name,
                label,
                order,
                wrappedFactory,
                field.FieldInfo,
                field.PropertyInfo,
                field.Required
            );

            scaffoldedField.Validators.AddRange(ScaffoldValidators(field));

            if (!string.IsNullOrEmpty(displayInfo.Description))
            {
                scaffoldedField.Description = displayInfo.Description;
            }

            if (!string.IsNullOrEmpty(displayInfo.GroupName))
            {
                scaffoldedField.Group = displayInfo.GroupName;
            }

            if (!string.IsNullOrEmpty(displayInfo.Prompt)) //We use prompt as a placeholder
            {
                scaffoldedField.Placeholder = displayInfo.Prompt;
            }

            scaffoldedFields[field.Name] = scaffoldedField;
        }

        return scaffoldedFields;
    }

    private static Func<IAnyState, IAnyInput>? ScaffoldInputFactory(FieldPropertyInfo field)
    {
        var type = field.Type;
        var name = field.Name;
        Type nonNullableType = field.NonNullableType;

        if (field.IsFileUpload())
        {
            // FileUpload fields are not auto-scaffolded. Use .Builder() to configure them manually.
            return null;
        }

        if (field.IsIdentity())
        {
            return (state) => state.ToReadOnlyInput();
        }

        if (field.IsColor())
        {
            return (state) =>
            {
                var input = state.ToColorInput();
                if (field.IsNullable && !field.Required) input.Nullable = true;
                return input;
            };
        }

        if (nonNullableType == typeof(bool))
        {
            return (state) =>
            {
                var input = state.ToBoolInput().ScaffoldDefaults(name, type);
                if (field.IsNullable && !field.Required) input.Nullable = true;
                return input;
            };
        }

        if (field.IsEmail())
        {
            return (state) =>
            {
                var input = ApplyMaxLength(state.ToEmailInput(), field);
                if (field.IsNullable && !field.Required) input.Nullable = true;
                return input;
            };
        }

        if (field.IsPhone())
        {
            return (state) =>
            {
                var input = ApplyMaxLength(state.ToTelInput(), field);
                if (field.IsNullable && !field.Required) input.Nullable = true;
                return input;
            };
        }

        if (field.IsUrl())
        {
            return (state) =>
            {
                var input = ApplyMaxLength(state.ToUrlInput(), field);
                if (field.IsNullable && !field.Required) input.Nullable = true;
                return input;
            };
        }

        if (field.IsPassword())
        {
            return (state) =>
            {
                var input = ApplyMaxLength(state.ToPasswordInput(), field);
                if (field.IsNullable && !field.Required) input.Nullable = true;
                return input;
            };
        }

        if (nonNullableType == typeof(string) && field.GetAllowedValues() is { } allowedValues)
        {
            return (state) =>
            {
                var options = allowedValues.Cast<string>().ToOptions().Cast<IAnyOption>().ToArray();
                var input = state.ToSelectInput(options);
                if (field.IsNullable && !field.Required) input.Nullable = true;
                return input;
            };
        }

        if (nonNullableType == typeof(string))
        {
            return (state) =>
            {
                var input = ApplyMaxLength(state.ToTextInput(), field);

                if (field.HasDataTypeAttribute(DataType.MultilineText))
                {
                    input = input.Variant(TextInputVariant.Textarea);
                }

                // If Required => don't show X button even for nullable types
                if (field.IsNullable && !field.Required) input = input.Nullable(true);
                return input;
            };
        }

        if (nonNullableType == typeof(Icons))
        {
            return (state) => state.ToIconInput();
        }

        if (nonNullableType.IsEnum)
        {
            return (state) =>
            {
                var input = state.ToSelectInput();
                if (field.IsNullable && !field.Required) input.Nullable = true;
                return input;
            };
        }

        if (type.IsCollectionType() && type.GetCollectionTypeParameter() is { IsEnum: true })
        {
            return (state) =>
            {
                var input = state.ToSelectInput().List();
                if (field.IsNullable && !field.Required) input.Nullable = true;
                return input;
            };
        }

        if (type.IsCollectionType() && type.GetCollectionTypeParameter() == typeof(string) && field.GetAllowedValues() is { } allowedCollectionValues)
        {
            return (state) =>
            {
                var options = allowedCollectionValues.Cast<string>().ToOptions().Cast<IAnyOption>().ToArray();
                var input = state.ToSelectInput(options).List();
                if (field.IsNullable && !field.Required) input.Nullable = true;
                return input;
            };
        }

        if (type.IsNumeric())
        {
            return (state) =>
            {
                var input = state.ToNumberInput();
                if (field.GetRangeInfo().Min is { } min)
                {
                    input = input.Min(min);
                }
                if (field.GetRangeInfo().Max is { } max)
                {
                    input = input.Max(max);
                }
                if (field.IsNullable && !field.Required) input.Nullable = true;
                return input.ScaffoldDefaults(name, type);
            };
        }

        if (type.IsDate())
        {
            return (state) =>
            {
                var input = state.ToDateTimeInput();

                if (field.HasDataTypeAttribute(DataType.Date))
                {
                    input = input.Variant(DateTimeInputVariant.Date);
                }
                else if (field.HasDataTypeAttribute(DataType.Time))
                {
                    input = input.Variant(DateTimeInputVariant.Time);
                }
                if (field.IsNullable && !field.Required) input.Nullable = true;
                return input;
            };
        }

        return null;
    }

    private static List<FieldPropertyInfo> GetFieldsAndProperties(Type type)
    {
        var fieldsAndProperties = new List<FieldPropertyInfo>();

        // Add fields
        var fields = type.GetFields()
            .Where(ShouldScaffold)
            .Select(e => new FieldPropertyInfo
            {
                Name = e.Name,
                Type = e.FieldType,
                FieldInfo = e,
                PropertyInfo = null,
                Required = FormHelpers.IsRequired(e)
            });

        // Add properties
        var properties = type.GetProperties()
            .Where(ShouldScaffold)
            .Select(e => new FieldPropertyInfo
            {
                Name = e.Name,
                Type = e.PropertyType,
                FieldInfo = null,
                PropertyInfo = e,
                Required = FormHelpers.IsRequired(e)
            });

        fieldsAndProperties.AddRange(fields);
        fieldsAndProperties.AddRange(properties);

        return fieldsAndProperties;
    }

    private static bool ShouldScaffold(MemberInfo member)
    {
        var scaffoldColumnAttr = member.GetCustomAttribute<ScaffoldColumnAttribute>();
        return scaffoldColumnAttr == null || scaffoldColumnAttr.Scaffold;
    }

    private static List<Func<object?, (bool, string)>> ScaffoldValidators(FieldPropertyInfo field)
    {
        var validators = new List<Func<object?, (bool, string)>>();

        if (field.Required)
        {
            validators.Add(e => (Utils.IsValidRequired(e), "Required field"));
        }

        if (field.PropertyInfo != null)
        {
            validators.AddRange(FormHelpers.GetValidators(field.PropertyInfo));
        }
        else if (field.FieldInfo != null)
        {
            validators.AddRange(FormHelpers.GetValidators(field.FieldInfo));
        }

        var nonNullableType = Nullable.GetUnderlyingType(field.Type) ?? field.Type;
        if (field.Name.EndsWith("Email") && nonNullableType == typeof(string))
        {
            validators.Add(Validators.CreateEmailValidator(field.Name));
        }

        return validators;
    }

    private static bool ShouldTrimIdSuffix(string? displayName, string label)
    {
        return displayName == null
            && label.EndsWith("Id")
            && label != "GovId"
            && label != "Id";
    }

    private static TextInputBase ApplyMaxLength(TextInputBase input, FieldPropertyInfo field)
    {
        if (field.GetMaxLength() is { } maxLength)
        {
            input = input.MaxLength(maxLength);
        }
        return input;
    }

    private record FieldPropertyInfo
    {
        public required string Name { get; init; }
        public required Type Type { get; init; }
        public FieldInfo? FieldInfo { get; init; }
        public PropertyInfo? PropertyInfo { get; init; }
        public required bool Required { get; init; }

        public FormHelpers.DisplayInfo GetDisplayInfo() => PropertyInfo != null ? FormHelpers.GetDisplayInfo(PropertyInfo) : FormHelpers.GetDisplayInfo(FieldInfo!);
        public FormHelpers.RangeInfo GetRangeInfo() => PropertyInfo != null ? FormHelpers.GetRangeInfo(PropertyInfo) : FormHelpers.GetRangeInfo(FieldInfo!);
        public int? GetMaxLength() => PropertyInfo != null ? FormHelpers.GetMaxLength(PropertyInfo) : FormHelpers.GetMaxLength(FieldInfo!);
        public object[]? GetAllowedValues() => PropertyInfo != null ? FormHelpers.GetAllowedValues(PropertyInfo) : FormHelpers.GetAllowedValues(FieldInfo!);

        public bool IsEmail() =>
            NonNullableType == typeof(string) &&
            (HasAttribute<EmailAddressAttribute>() ||
             HasDataTypeAttribute(DataType.EmailAddress) ||
             Name.EndsWith("email", StringComparison.OrdinalIgnoreCase));

        public bool IsPhone() =>
            NonNullableType == typeof(string) &&
            (HasAttribute<PhoneAttribute>() ||
             HasDataTypeAttribute(DataType.PhoneNumber) ||
             Name.EndsWith("phone", StringComparison.OrdinalIgnoreCase));

        public bool IsUrl() =>
            NonNullableType == typeof(string) &&
            (HasAttribute<UrlAttribute>() ||
             HasDataTypeAttribute(DataType.Url) ||
             Name.EndsWith("url", StringComparison.OrdinalIgnoreCase));

        public bool IsFileUpload() =>
            IsFileUploadType(NonNullableType) ||
            Type.GetCollectionTypeParameter() is { } elementType && IsFileUploadType(elementType);

        public bool IsIdentity() =>
            (Name.EndsWith("Id") && (NonNullableType == typeof(Guid) || NonNullableType == typeof(int) || NonNullableType == typeof(string)));

        public bool IsColor() =>
            (Name.EndsWith("Color") || Name.EndsWith("Colour")) && NonNullableType == typeof(string);

        public bool IsPassword() =>
            NonNullableType == typeof(string) && (
                Name.EndsWith("Password") ||
                HasDataTypeAttribute(DataType.Password)
            );

        public Type NonNullableType => Nullable.GetUnderlyingType(Type) ?? Type;

        public bool IsNullable
        {
            get
            {
                if (Nullable.GetUnderlyingType(Type) != null)
                    return true;

                if (Type.IsClass)
                {
                    var nullabilityContext = new NullabilityInfoContext();
                    if (PropertyInfo != null)
                    {
                        var nullabilityInfo = nullabilityContext.Create(PropertyInfo);
                        return nullabilityInfo.ReadState == NullabilityState.Nullable;
                    }
                    if (FieldInfo != null)
                    {
                        var nullabilityInfo = nullabilityContext.Create(FieldInfo);
                        return nullabilityInfo.ReadState == NullabilityState.Nullable;
                    }
                }

                return false;
            }
        }

        private bool HasAttribute<T>() where T : Attribute
        {
            return PropertyInfo != null
                ? PropertyInfo.GetCustomAttribute<T>() != null
                : FieldInfo!.GetCustomAttribute<T>() != null;
        }

        public bool HasDataTypeAttribute(DataType dataType)
        {
            var dataTypeAttr = PropertyInfo != null
                ? PropertyInfo.GetCustomAttribute<DataTypeAttribute>()
                : FieldInfo!.GetCustomAttribute<DataTypeAttribute>();

            return dataTypeAttr != null && dataTypeAttr.DataType == dataType;
        }

        private bool IsFileUploadType(Type t)
        {
            if (t == typeof(FileUpload)) return true;
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(FileUpload<>)) return true;
            return typeof(IFileUpload).IsAssignableFrom(t);
        }
    }

}
