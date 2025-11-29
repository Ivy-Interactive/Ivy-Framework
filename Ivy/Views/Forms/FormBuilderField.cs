using System.Reflection;
using Ivy.Core.Hooks;
using Ivy.Widgets.Inputs;

namespace Ivy.Views.Forms;

public class FormBuilderField<TModel>(
    string name,
    string label,
    int order,
    Func<IAnyState, IViewContext, IAnyInput>? inputFactory,
    FieldInfo? fieldInfo,
    PropertyInfo? propertyInfo,
    bool required)
{
    public Func<TModel, bool> Visible { get; set; } = _ => true;

    public string Name { get; set; } = name;

    private FieldInfo? FieldInfo { get; set; } = fieldInfo;

    private PropertyInfo? PropertyInfo { get; set; } = propertyInfo;

    public Type Type => (FieldInfo?.FieldType ?? PropertyInfo?.PropertyType)!;

    public bool Disabled { get; set; } = true;

    public int Order { get; set; } = order;

    public int Column { get; set; } = 0;

    public Guid RowKey { get; set; } = Guid.NewGuid();

    public string? Group { get; set; }

    public string Label { get; set; } = label;

    public string? Description { get; set; }

    public string? Help { get; set; }

    public string? Placeholder { get; set; }

    public Func<IAnyState, IViewContext, IAnyInput>? InputFactory { get; set; } = inputFactory;

    public bool Removed { get; set; }

    public bool Required { get; set; } = required;

    public List<Func<object?, (bool, string)>> Validators { get; set; } = new();
}