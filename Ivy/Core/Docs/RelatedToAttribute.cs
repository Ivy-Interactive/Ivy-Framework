namespace Ivy.Core.Docs;

public class RelatedToAttribute(string propertyName) : Attribute
{
    public string PropertyName { get; } = propertyName;
}