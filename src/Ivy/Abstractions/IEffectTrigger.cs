
//ReSharper disable once CheckNamespace
namespace Ivy;

public interface IEffectTrigger : IEffectTriggerConvertible
{
    public EffectTriggerType Type { get; }

    public IAnyState? State { get; }
}

public enum EffectTriggerType
{
    AfterChange,
    AfterInit,
    AfterRender
}
