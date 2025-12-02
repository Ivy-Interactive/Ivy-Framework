using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record Detail : WidgetBase<Detail>
{
    public Detail(string? label, object? value, bool multiLine) : base(value != null ? [value] : [])
    {
        Label = label;
        MultiLine = multiLine;
    }

    [Prop] public string? Label { get; set; }

    [Prop] public bool MultiLine { get; set; }

    public static Detail operator |(Detail widget, object child)
    {
        throw new NotSupportedException("Detail does not support children.");
    }
}