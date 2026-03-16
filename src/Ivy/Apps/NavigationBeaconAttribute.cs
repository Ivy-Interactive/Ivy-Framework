namespace Ivy;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class NavigationBeaconAttribute(Type entityType, string factoryMethodName) : Attribute
{
    public Type EntityType { get; } = entityType;
    public string FactoryMethodName { get; } = factoryMethodName;
}
