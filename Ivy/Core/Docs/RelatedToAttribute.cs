namespace Ivy.Core.Docs;

/// <summary>Attribute used to associate extension methods with specific widget properties for documentation generation. Links extension methods to properties when the method name doesn't directly match the property name.</summary>
public class RelatedToAttribute(string propertyName) : Attribute
{
    public string PropertyName { get; } = propertyName;
}