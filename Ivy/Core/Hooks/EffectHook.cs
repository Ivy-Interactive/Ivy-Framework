namespace Ivy.Core.Hooks;

public class EffectHook(int identity, Func<Task<IDisposable>> handler, IEffectTrigger[] triggers)
{
    public int Identity { get; } = identity;

    public Func<Task<IDisposable>> Handler { get; } = handler;

    public IEffectTrigger[] Triggers { get; } = triggers;

    public static EffectHook Create(int identity, Func<Task<IDisposable>> effect, IEffectTrigger[] triggers)
    {
        // If no triggers are provided, assume the effect should be triggered after initialization
        if (triggers.Length == 0)
        {
            triggers = [EffectTrigger.AfterInit()];
        }
        return new(identity, effect, triggers);
    }
}