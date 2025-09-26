namespace Ivy.Core;

/// <summary>
/// Custom attribute that marks properties as event sources. This attribute
/// can be applied to properties to indicate they are event handlers or
/// event-related functionality within the Ivy framework.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class EventAttribute : Attribute
{

}