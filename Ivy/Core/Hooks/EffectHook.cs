namespace Ivy.Core.Hooks;

/// <summary>
/// Represents an effect with its handler function, triggers, and unique identity for the hooks system.
/// </summary>
/// <param name="identity">Unique identifier for this effect within its view context.</param>
/// <param name="handler">Async function that executes the effect and returns a cleanup disposable.</param>
/// <param name="triggers">Array of triggers that determine when this effect should execute.</param>
public class EffectHook(int identity, Func<Task<IDisposable>> handler, IEffectTrigger[] triggers)
{
    /// <summary>Gets the unique identifier for this effect hook.</summary>
    public int Identity { get; } = identity;

    /// <summary>Gets the async handler function that executes the effect.</summary>
    public Func<Task<IDisposable>> Handler { get; } = handler;

    /// <summary>Gets the array of triggers that determine when this effect executes.</summary>
    public IEffectTrigger[] Triggers { get; } = triggers;

    /// <summary>
    /// Creates a new effect hook with default triggers if none are provided.
    /// </summary>
    /// <param name="identity">Unique identifier for this effect.</param>
    /// <param name="effect">The effect handler function.</param>
    /// <param name="triggers">Triggers that determine when the effect executes.</param>
    /// <returns>A new effect hook instance.</returns>
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