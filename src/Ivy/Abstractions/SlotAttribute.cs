// Resharper disable once CheckNamespace
namespace Ivy;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class SlotAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}
