namespace Ivy.Core.Hooks;

/// <summary>
/// Concrete implementation of effect triggers with factory methods for common trigger types.
/// </summary>
public class EffectTrigger : IEffectTrigger
{
    /// <summary>Gets the type of trigger that determines when the effect executes.</summary>
    public EffectTriggerType Type { get; }

    /// <summary>Gets the state object associated with this trigger, if any.</summary>
    public IAnyState? State { get; }

    /// <summary>
    /// Creates a new effect trigger with the specified type and optional state.
    /// </summary>
    /// <param name="type">The trigger type.</param>
    /// <param name="state">The associated state object, if any.</param>
    private EffectTrigger(EffectTriggerType type, IAnyState? state)
    {
        Type = type;
        State = state;
    }

    /// <summary>
    /// Creates a trigger that fires when the specified state changes.
    /// </summary>
    /// <param name="state">The state to monitor for changes.</param>
    /// <returns>An effect trigger for state changes.</returns>
    public static EffectTrigger AfterChange(IAnyState state) =>
        new(EffectTriggerType.AfterChange, state);

    /// <summary>
    /// Creates a trigger that fires once after component initialization.
    /// </summary>
    /// <returns>An effect trigger for initialization.</returns>
    public static EffectTrigger AfterInit() =>
        new(EffectTriggerType.AfterInit, null);

    /// <summary>
    /// Creates a trigger that fires after each render cycle.
    /// </summary>
    /// <returns>An effect trigger for render cycles.</returns>
    public static EffectTrigger AfterRender() =>
        new(EffectTriggerType.AfterRender, null);

    /// <summary>
    /// Returns this trigger instance (implements IEffectTriggerConvertible).
    /// </summary>
    /// <returns>This effect trigger.</returns>
    public IEffectTrigger ToTrigger()
    {
        return this;
    }
}

/// <summary>
/// Extension methods for converting observables to effect triggers.
/// </summary>
public static class EffectExtensions
{
    /// <summary>
    /// Adapter that wraps an IObservable as an IAnyState for effect triggering.
    /// </summary>
    private class ObservableState<T> : IAnyState
    {
        private readonly IObservable<T> _observable;

        /// <summary>
        /// Creates a new observable state wrapper.
        /// </summary>
        /// <param name="observable">The observable to wrap.</param>
        internal ObservableState(IObservable<T> observable)
        {
            _observable = observable;
        }

        /// <summary>Disposes the observable state (no-op for observables).</summary>
        public void Dispose()
        {
            // Nothing to dispose for observables
        }

        /// <summary>Creates an effect trigger (not implemented for observables).</summary>
        public IEffectTrigger ToTrigger()
        {
            throw new NotImplementedException();
        }

        /// <summary>Subscribes to observable changes without receiving values.</summary>
        public IDisposable SubscribeAny(Action action)
        {
            return _observable.Subscribe(_ => action());
        }

        /// <summary>Subscribes to observable changes and receives values as objects.</summary>
        public IDisposable SubscribeAny(Action<object?> action)
        {
            return _observable.Subscribe(x => action(x));
        }

        /// <summary>Gets the observable's value type.</summary>
        public Type GetStateType()
        {
            return typeof(T);
        }
    }

    /// <summary>
    /// Converts an observable to an effect trigger that fires on value changes.
    /// </summary>
    /// <typeparam name="T">The type of values emitted by the observable.</typeparam>
    /// <param name="observable">The observable to convert.</param>
    /// <returns>An effect trigger for observable changes.</returns>
    public static IEffectTrigger ToTrigger<T>(this IObservable<T> observable)
    {
        return EffectTrigger.AfterChange(new ObservableState<T>(observable));
    }
}

