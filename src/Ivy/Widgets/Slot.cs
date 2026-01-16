using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A placeholder for dynamic content injection.
/// </summary>
public record Slot : WidgetBase<Slot>
{
    [Prop] public string? Name { get; set; }

    internal Slot() { }

    public Slot(params object[] children) : this(null, children)
    {
    }

    public Slot(string? name, params object?[] children) : base(children!)
    {
        Name = name;
    }
}