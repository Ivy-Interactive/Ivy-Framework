namespace Ivy.Core;

[AttributeUsage(AttributeTargets.Property)]
public class PropAttribute(string? attached = null) : Attribute
{
    public string? AttachedName { get; set; } = attached;

    public bool IsAttached => AttachedName != null;
}