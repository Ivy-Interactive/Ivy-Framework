using Ivy.Core;
using Ivy.Widgets.Inputs;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record FormField : WidgetBase<FormField>
{
    internal FormField(IAnyInput input, string? label = null, string? description = null, bool required = false) : base([input])
    {
        var labelProp = input.GetType().GetProperty("Label");
        if (labelProp != null && labelProp.PropertyType == typeof(string))
        {
            //Input handles label on its own
            var inputLabel = (string?)labelProp.GetValue(input);
            labelProp.SetValue(input, inputLabel ?? label);
            label = null;
        }

        var descriptionProp = input.GetType().GetProperty("Description");
        if (descriptionProp != null && descriptionProp.PropertyType == typeof(string))
        {
            //Input handles description on its own
            var inputDescription = (string?)descriptionProp.GetValue(input);
            descriptionProp.SetValue(input, inputDescription ?? description);
            description = null;
        }
        Label = label;
        Description = description;
        Required = required;
    }

    [Prop] public string? Label { get; set; }

    [Prop] public string? Description { get; set; }

    [Prop] public bool Required { get; set; }

    public static FormField operator |(FormField widget, object child)
    {
        throw new NotSupportedException("FormField does not support children.");
    }
}