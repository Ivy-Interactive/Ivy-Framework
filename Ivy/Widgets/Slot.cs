using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Slot widget providing named content areas within parent widgets for flexible content organization and layout management.</summary>
public record Slot : WidgetBase<Slot>
{
    /// <summary>Name identifier for this slot enabling parent widgets to reference specific content areas. Default is null (unnamed slot).</summary>
    [Prop] public string? Name { get; set; }

    /// <summary>Initializes Slot with specified children and no name.</summary>
    /// <param name="children">Child elements to display within slot.</param>
    public Slot(params object[] children) : this(null, children)
    {
    }

    /// <summary>Initializes Slot with specified name and children.</summary>
    /// <param name="name">Name identifier for this slot used by parent widgets to reference content area.</param>
    /// <param name="children">Child elements to display within slot.</param>
    public Slot(string? name, params object?[] children) : base(children!)
    {
        Name = name;
    }
}