namespace Ivy.Core;

/// <summary>Serializes properties to widget JSON state (regular or attached).</summary>
[AttributeUsage(AttributeTargets.Property)]
public class PropAttribute(string? attached = null) : Attribute
{
    public string? AttachedName { get; set; } = attached;

    /// <summary>True if this is an attached property.</summary>
    public bool IsAttached => AttachedName != null;
}