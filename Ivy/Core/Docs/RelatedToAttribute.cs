namespace Ivy.Core.Docs;

/// <summary>
/// Attribute used to associate extension methods with specific widget properties for documentation generation.
/// Links extension methods to properties when the method name doesn't directly match the property name.
/// </summary>
/// <param name="propertyName">The name of the property this extension method is related to.</param>
public class RelatedToAttribute(string propertyName) : Attribute
{
    /// <summary>Gets the name of the property this extension method is related to.</summary>
    public string PropertyName { get; } = propertyName;
}