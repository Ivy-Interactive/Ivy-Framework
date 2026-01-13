namespace Ivy.Core.Hooks;

public class EffectHook(int identity, Func<Task<IAsyncDisposable?>> handler, IEffectTrigger[] triggers)
{
    public int Identity { get; } = identity;

    public Func<Task<IAsyncDisposable?>> Handler { get; } = handler;

    public IEffectTrigger[] Triggers { get; } = triggers;

    public static EffectHook Create(int identity, Func<Task<IAsyncDisposable?>> effect, IEffectTrigger[] triggers)
    {
        // If no triggers are provided, assume the effect should be triggered after initialization
        if (triggers.Length == 0)
        {
            triggers = [EffectTrigger.OnMount()];
        }
        return new(identity, effect, triggers);
    }
}