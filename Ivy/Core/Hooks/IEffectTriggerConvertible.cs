namespace Ivy.Core.Hooks;

/// <summary>
/// Interface for objects that can be converted to effect triggers for use with UseEffect.
/// </summary>
public interface IEffectTriggerConvertible
{
    /// <summary>
    /// Converts this object to an effect trigger.
    /// </summary>
    /// <returns>An effect trigger that can be used with UseEffect.</returns>
    public IEffectTrigger ToTrigger();
}