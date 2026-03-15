// Resharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Specifies the allowed child type for a widget when using the | operator.
/// The analyzer uses this attribute to report IVYCHILD003 at compile time
/// instead of throwing NotSupportedException at runtime.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class ChildTypeAttribute(Type type) : Attribute
{
    public Type Type { get; } = type;
}
