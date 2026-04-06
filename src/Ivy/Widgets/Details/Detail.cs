// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A specific detail item within a Details view.
/// </summary>
public record Detail : WidgetBase<Detail>
{
    public Detail(string? label, object? value, bool multiline = false) : base(value != null ? [value] : [])
    {
        Label = label;
        Multiline = multiline;
    }

    internal Detail()
    {
    }

    [Prop] public string? Label { get; set; }

    [Prop] public bool Multiline { get; set; }

    public static Detail operator |(Detail widget, object child)
    {
        throw new NotSupportedException("Detail does not support children.");
    }
}