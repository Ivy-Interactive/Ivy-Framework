using System.Reflection;
using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;
using Ivy.Services;
using Ivy.Shared;
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
            var displayInfo = field.PropertyInfo != null
                ? FormHelpers.GetDisplayInfo(field.PropertyInfo)
                : FormHelpers.GetDisplayInfo(field.FieldInfo!);

            var label = displayInfo.Name ?? Utils.LabelFor(field.Name, field.Type);

            // Trim "Id" suffix from labels (except "GovId" and plain "Id")
            if (!field.Name.EndsWith("GovId") && field.Name != "Id" && field.Name.EndsWith("Id"))
            {
                label = label[..^3];
            }

            var order = displayInfo.Order ?? int.MaxValue;

            var factory = ScaffoldInputFactory(field.Name, field.Type);

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

    private static Func<IAnyState, IAnyInput>? ScaffoldInputFactory(string name, Type type)
    {
        Type nonNullableType = Nullable.GetUnderlyingType(type) ?? type;

        // FileUpload fields are not auto-scaffolded. Use .Builder() to configure them manually.
        if (IsFileUploadType(nonNullableType))
        {
            return null;
        }

        if (type.GetCollectionTypeParameter() is { } elementType && IsFileUploadType(elementType))
        {
            return null;
        }

        if (name.EndsWith("Id") && (type == typeof(Guid) || type == typeof(int) || type == typeof(string)))
        {
            return (state) => state.ToReadOnlyInput();
        }

        if (name.EndsWith("Email") && nonNullableType == typeof(string))
        {
            return (state) => state.ToEmailInput();
        }

        if ((name.EndsWith("Color") || name.EndsWith("Colour")) && nonNullableType == typeof(string))
        {
            return (state) => state.ToColorInput();
        }

        if (nonNullableType == typeof(bool))
        {
            return (state) => state.ToBoolInput().ScaffoldDefaults(name, type);
        }

        if (nonNullableType == typeof(string))
        {
            if (name.EndsWith("Password"))
            {
                return (state) => state.ToPasswordInput();
            }

            return (state) => state.ToTextInput();
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
            return (state) => state.ToNumberInput().ScaffoldDefaults(name, type);
        }

        if (type.IsDate())
        {
            return (state) => state.ToDateTimeInput();
        }

        return null;

        static bool IsFileUploadType(Type t)
        {
            if (t == typeof(FileUpload)) return true;
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(FileUpload<>)) return true;
            return typeof(IFileUpload).IsAssignableFrom(t);
        }
    }

    /// <summary>Gets all fields and properties from a type with their metadata.</summary>
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

    /// <summary>Collects all validators for a field including required, DataAnnotations, and convention-based validators.</summary>
    private static List<Func<object?, (bool, string)>> ScaffoldValidators(FieldPropertyInfo field)
    {
        var validators = new List<Func<object?, (bool, string)>>();

        // Add required field validator
        if (field.Required)
        {
            validators.Add(e => (Utils.IsValidRequired(e), "Required field"));
        }

        // Add validators from DataAnnotations attributes
        if (field.PropertyInfo != null)
        {
            validators.AddRange(FormHelpers.GetValidators(field.PropertyInfo));
        }
        else if (field.FieldInfo != null)
        {
            validators.AddRange(FormHelpers.GetValidators(field.FieldInfo));
        }

        // Add automatic email validation for fields ending with "Email"
        var nonNullableType = Nullable.GetUnderlyingType(field.Type) ?? field.Type;
        if (field.Name.EndsWith("Email") && nonNullableType == typeof(string))
        {
            validators.Add(Validators.CreateEmailValidator(field.Name));
        }

        return validators;
    }

    private record FieldPropertyInfo
    {
        public required string Name { get; init; }
        public required Type Type { get; init; }
        public FieldInfo? FieldInfo { get; init; }
        public PropertyInfo? PropertyInfo { get; init; }
        public required bool Required { get; init; }
    }
}