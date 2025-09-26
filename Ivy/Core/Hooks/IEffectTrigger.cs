namespace Ivy.Core.Hooks;

/// <summary>
/// Represents a trigger condition that determines when an effect should execute.
/// </summary>
public interface IEffectTrigger : IEffectTriggerConvertible
{
    /// <summary>Gets the type of trigger that determines when the effect executes.</summary>
    public EffectTriggerType Type { get; }

    /// <summary>Gets the state object associated with this trigger, if any.</summary>
    public IAnyState? State { get; }
}

