namespace Ivy.Core;

/// <summary>Serializes properties to widget JSON state (regular or attached).</summary>
/// <param name="attached">Attached property name. When set, property is attached to children.</param>
[AttributeUsage(AttributeTargets.Property)]
public class PropAttribute(string? attached = null) : Attribute
{
    /// <summary>Attached property name.</summary>
    public string? AttachedName { get; set; } = attached;

    /// <summary>True if this is an attached property.</summary>
    public bool IsAttached => AttachedName != null;
}