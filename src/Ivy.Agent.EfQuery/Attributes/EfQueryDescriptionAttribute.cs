namespace Ivy.Agent.EfQuery;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
public class EfQueryDescriptionAttribute(string description) : Attribute
{
    public string Description { get; } = description;
}
