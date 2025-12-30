namespace Ivy.Core.Hooks;

public interface IEffectTrigger : IEffectTriggerConvertible
{
    public EffectTriggerType Type { get; }

    public IAnyState? State { get; }
}

