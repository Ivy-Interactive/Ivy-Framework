using System.Reflection;
using Ivy.Core.Hooks;
using Ivy.Services;
using Ivy.Widgets.Inputs;

namespace Ivy.Views.Forms;

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

            // Trim "Id" suffix from labels (except "GovId" and plain "Id")
            if (!field.Name.EndsWith("GovId") && field.Name != "Id" && field.Name.EndsWith("Id"))
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

        Type nonNullableType = Nullable.GetUnderlyingType(type) ?? type;

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
            return (state) => state.ToColorInput();
        }

        if (nonNullableType == typeof(bool))
        {
            return (state) => state.ToBoolInput().ScaffoldDefaults(name, type);
        }

        if (field.IsEmail())
        {
            return (state) => ApplyMaxLength(state.ToEmailInput(), field);
        }

        if (field.IsPhone())
        {
            return (state) => ApplyMaxLength(state.ToTelInput(), field);
        }

        if (field.IsUrl())
        {
            return (state) => ApplyMaxLength(state.ToUrlInput(), field);
        }

        if (field.IsPassword())
        {
            return (state) => ApplyMaxLength(state.ToPasswordInput(), field);
        }

        if (nonNullableType == typeof(string))
        {
            return (state) => ApplyMaxLength(state.ToTextInput(), field);
        }

        if (nonNullableType.IsEnum)
        {
            return (state) => state.ToSelectInput();
        }

        if (type.IsCollectionType() && type.GetCollectionTypeParameter() is { IsEnum: true })
        {
            return (state) => state.ToSelectInput().List();
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
                return input.ScaffoldDefaults(name, type);
            };
        }

        if (type.IsDate())
        {
            return (state) => state.ToDateTimeInput();
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
        var scaffoldColumnAttr = member.GetCustomAttribute<System.ComponentModel.DataAnnotations.ScaffoldColumnAttribute>();
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

        public bool IsEmail() =>
            NonNullableType == typeof(string) &&
            (HasAttribute<System.ComponentModel.DataAnnotations.EmailAddressAttribute>() ||
             Name.EndsWith("email", StringComparison.OrdinalIgnoreCase));

        public bool IsPhone() =>
            NonNullableType == typeof(string) &&
            (HasAttribute<System.ComponentModel.DataAnnotations.PhoneAttribute>() ||
             Name.EndsWith("phone", StringComparison.OrdinalIgnoreCase));

        public bool IsUrl() =>
            NonNullableType == typeof(string) &&
            (HasAttribute<System.ComponentModel.DataAnnotations.UrlAttribute>() ||
             Name.EndsWith("url", StringComparison.OrdinalIgnoreCase));

        public bool IsCreditCard() =>
            NonNullableType == typeof(string) &&
            (HasAttribute<System.ComponentModel.DataAnnotations.UrlAttribute>() ||
             Name.EndsWith("url", StringComparison.OrdinalIgnoreCase));

        public bool IsFileUpload() =>
            IsFileUploadType(NonNullableType) ||
            Type.GetCollectionTypeParameter() is { } elementType && IsFileUploadType(elementType);

        public bool IsIdentity() =>
            (Name.EndsWith("Id") && (NonNullableType == typeof(Guid) || NonNullableType == typeof(int) || NonNullableType == typeof(string)));

        public bool IsColor() =>
            (Name.EndsWith("Color") || Name.EndsWith("Colour")) && NonNullableType == typeof(string);

        public bool IsPassword() =>
            NonNullableType == typeof(string) && Name.EndsWith("Password");

        private Type NonNullableType => Nullable.GetUnderlyingType(Type) ?? Type;

        private bool HasAttribute<T>() where T : Attribute
        {
            return PropertyInfo != null
                ? PropertyInfo.GetCustomAttribute<T>() != null
                : FieldInfo!.GetCustomAttribute<T>() != null;
        }

        private bool IsFileUploadType(Type t)
        {
            if (t == typeof(FileUpload)) return true;
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(FileUpload<>)) return true;
            return typeof(IFileUpload).IsAssignableFrom(t);
        }
    }

}