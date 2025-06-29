namespace Ivy.Core.Hooks;

public class EffectHook(int identity, Func<Task<IDisposable>> handler, IEffectTrigger[] triggers)
{
    public int Identity { get; } = identity;

    public Func<Task<IDisposable>> Handler { get; } = handler;

    public IEffectTrigger[] Triggers { get; } = triggers;

    public static EffectHook Create(int identity, Func<Task<IDisposable>> effect, IEffectTrigger[] triggers)
    {
        //if no triggers are provided we assume that the effect should be triggered after rendering the first time
        if (triggers.Length == 0)
        {
            triggers = [EffectTrigger.AfterInit()];
        }
        return new(identity, effect, triggers);
    }

}