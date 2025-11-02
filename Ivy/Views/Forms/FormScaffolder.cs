using System.Reflection;
using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;
using Ivy.Shared;
using Ivy.Widgets.Inputs;

namespace Ivy.Views.Forms;

/// <summary>Responsible for automatically scaffolding form fields from model types with support for DataAnnotations attributes.</summary>
public static class FormScaffolder
{
    /// <summary>Scaffolds form fields from a model type with automatic input type detection and DataAnnotations support.</summary>
    /// <typeparam name="TModel">The model type to scaffold fields from.</typeparam>
    /// <param name="modelType">The type of the model.</param>
    /// <param name="size">The size to apply to scaffolded inputs.</param>
    /// <returns>Dictionary of field names to form builder fields.</returns>
    public static Dictionary<string, FormBuilderField<TModel>> ScaffoldFields<TModel>(Type modelType, Sizes size = Sizes.Medium)
    {
        var fields = GetFieldsAndProperties(modelType);
        var scaffoldedFields = new Dictionary<string, FormBuilderField<TModel>>();

        foreach (var field in fields)
        {
            var displayInfo = field.PropertyInfo != null 
                ? FormHelpers.GetDisplayInfo(field.PropertyInfo)
                : FormHelpers.GetDisplayInfo(field.FieldInfo!);

            var label = displayInfo.Name ?? Utils.LabelFor(field.Name, field.Type);
            var order = displayInfo.Order ?? int.MaxValue;
            
            var scaffoldedField = new FormBuilderField<TModel>(
                field.Name, 
                label, 
                order, 
                ScaffoldInputFactory(field.Name, field.Type, size),
                field.FieldInfo, 
                field.PropertyInfo, 
                field.Required
            );

            // Apply display information
            if (!string.IsNullOrEmpty(displayInfo.Description))
            {
                scaffoldedField.Description = displayInfo.Description;
            }

            if (!string.IsNullOrEmpty(displayInfo.GroupName))
            {
                scaffoldedField.Group = displayInfo.GroupName;
            }

            scaffoldedFields[field.Name] = scaffoldedField;
        }

        return scaffoldedFields;
    }

    /// <summary>Creates an input factory function for a field based on its name and type.</summary>
    /// <param name="name">The field name.</param>
    /// <param name="type">The field type.</param>
    /// <param name="size">The size to apply to the input.</param>
    /// <returns>Input factory function or null if no suitable input type found.</returns>
    public static Func<IAnyState, IAnyInput>? ScaffoldInputFactory(string name, Type type, Sizes size)
    {
        Type nonNullableType = Nullable.GetUnderlyingType(type) ?? type;

        if (type == typeof(FileInput))
        {
            return (state) => state.ToFileInput().Size(size);
        }

        if (name.EndsWith("Id") && (type == typeof(Guid) || type == typeof(int) || type == typeof(string)))
        {
            return (state) => state.ToReadOnlyInput().Size(size);
        }

        if (name.EndsWith("Email") && nonNullableType == typeof(string))
        {
            return (state) => state.ToEmailInput().Size(size);
        }

        if ((name.EndsWith("Color") || name.EndsWith("Colour")) && nonNullableType == typeof(string))
        {
            return (state) => state.ToColorInput().Size(size);
        }

        if (nonNullableType == typeof(bool))
        {
            return (state) =>
            {
                var input = state.ToBoolInput();
                // Apply scaffold defaults
                input.ScaffoldDefaults(name, type);
                return input.Size(size);
            };
        }

        if (nonNullableType == typeof(string))
        {
            if (name.EndsWith("Password"))
            {
                return (state) => state.ToPasswordInput().Size(size);
            }

            return (state) => state.ToTextInput().Size(size);
        }

        if (nonNullableType.IsEnum)
        {
            return (state) => state.ToSelectInput().Size(size);
        }

        if (type.IsCollectionType() && type.GetCollectionTypeParameter() is { IsEnum: true })
        {
            return (state) => state.ToSelectInput().List().Size(size);
        }

        if (type.IsNumeric())
        {
            return (state) => state.ToNumberInput().ScaffoldDefaults(name, type).Size(size);
        }

        if (type.IsDate())
        {
            return (state) => state.ToDateTimeInput().Size(size);
        }

        return null;
    }

    /// <summary>Gets all fields and properties from a type with their metadata.</summary>
    private static List<FieldPropertyInfo> GetFieldsAndProperties(Type type)
    {
        var fieldsAndProperties = new List<FieldPropertyInfo>();

        // Add fields
        var fields = type.GetFields().Select(e => new FieldPropertyInfo
        {
            Name = e.Name,
            Type = e.FieldType,
            FieldInfo = e,
            PropertyInfo = null,
            Required = FormHelpers.IsRequired(e)
        });

        // Add properties
        var properties = type.GetProperties().Select(e => new FieldPropertyInfo
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

    /// <summary>Internal record for holding field/property information during scaffolding.</summary>
    private record FieldPropertyInfo
    {
        public required string Name { get; init; }
        public required Type Type { get; init; }
        public FieldInfo? FieldInfo { get; init; }
        public PropertyInfo? PropertyInfo { get; init; }
        public required bool Required { get; init; }
    }
}