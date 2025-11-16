using Ivy.Core;
using Ivy.Shared;
using Ivy.Widgets.Inputs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Ivy;

public record Field : WidgetBase<Field>
{
    public Field(IAnyInput input, string? label = null, string? description = null, bool required = false, string? help = null) : base([input])
    {
        var labelProp = input.GetType().GetProperty("Label");
        if (labelProp != null && labelProp.PropertyType == typeof(string))
        {
            var inputLabel = (string?)labelProp.GetValue(input);
            labelProp.SetValue(input, inputLabel ?? label);
            label = null;
        }

        var descriptionProp = input.GetType().GetProperty("Description");
        if (descriptionProp != null && descriptionProp.PropertyType == typeof(string))
        {
            var inputDescription = (string?)descriptionProp.GetValue(input);
            descriptionProp.SetValue(input, inputDescription ?? description);
            description = null;
        }
        Label = label;
        Description = description;
        Required = required;
        Help = help;
    }

    [Prop] public string? Label { get; set; }

    [Prop] public string? Description { get; set; }

    [Prop] public bool Required { get; set; }

    [Prop] public string? Help { get; set; }

    [Prop] public Sizes Size { get; set; } = Sizes.Medium;

    /// <exception cref="NotSupportedException">Field widgets wrap single input control.</exception>
    public static Field operator |(Field widget, object child)
    {
        throw new NotSupportedException("Field does not support children.");
    }
}

public static class FieldExtensions
{

    public static Field Label(this Field field, string label) => field with { Label = label };

    public static Field Description(this Field field, string description) => field with { Description = description };

    public static Field Help(this Field field, string help) => field with { Help = help };

    public static Field Required(this Field field) => field with { Required = true };

    public static Field Size(this Field field, Sizes size) => field with { Size = size };

    public static Field WithField(this IAnyInput input) => new Field(input);

}

