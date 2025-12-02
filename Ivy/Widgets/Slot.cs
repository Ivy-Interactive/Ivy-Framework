using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record Slot : WidgetBase<Slot>
{
    [Prop] public string? Name { get; set; }

    public Slot(params object[] children) : this(null, children)
    {
    }

    public Slot(string? name, params object?[] children) : base(children!)
    {
        Name = name;
    }
}