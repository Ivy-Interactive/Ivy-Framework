namespace Ivy.Core;

/// <summary>Attribute marking properties as serializable widget properties included in widget's JSON representation supporting regular and attached properties.</summary>
/// <param name="attached">Optional name of attached property. When specified, property becomes attached property set on child widgets and accessed by parent widget.</param>
[AttributeUsage(AttributeTargets.Property)]
public class PropAttribute(string? attached = null) : Attribute
{
    /// <summary>Name of attached property. When not null, indicates property is attached property set on child widgets.</summary>
    public string? AttachedName { get; set; } = attached;

    /// <summary>Whether this property is attached property used in parent-child widget relationships.</summary>
    public bool IsAttached => AttachedName != null;
}