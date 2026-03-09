// Resharper disable once CheckNamespace
namespace Ivy;

[AttributeUsage(AttributeTargets.Property)]
public class PropAttribute(string? attached = null) : Attribute
{
    public string? AttachedName { get; set; } = attached;

    public bool IsAttached => AttachedName != null;

    public bool AlwaysSerialize { get; set; } = false;
}