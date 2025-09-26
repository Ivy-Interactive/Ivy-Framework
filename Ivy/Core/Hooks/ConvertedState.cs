namespace Ivy.Core.Hooks;

/// <summary>
/// Adapter that converts between different state types using forward and backward transformation functions.
/// Allows working with state in a different type while maintaining reactivity with the original state.
/// </summary>
/// <typeparam name="TFrom">The original state type.</typeparam>
/// <typeparam name="TTo">The converted state type.</typeparam>
/// <param name="originalState">The original state to convert from.</param>
/// <param name="forward">Function to convert from original type to target type.</param>
/// <param name="backward">Function to convert from target type back to original type.</param>
public class ConvertedState<TFrom, TTo>(IState<TFrom> originalState, Func<TFrom, TTo> forward, Func<TTo, TFrom> backward) : IState<TTo>
{
    /// <summary>
    /// Observer adapter that forwards notifications from the original state type to the converted type.
    /// </summary>
    private class ForwardingObserver(IObserver<TTo> observer, Func<TFrom, TTo> forward) : IObserver<TFrom>
    {
        /// <summary>Converts and forwards the next value to the target observer.</summary>
        public void OnNext(TFrom value) => observer.OnNext(forward(value));
        /// <summary>Forwards error notifications to the target observer.</summary>
        public void OnError(Exception error) => observer.OnError(error);
        /// <summary>Forwards completion notifications to the target observer.</summary>
        public void OnCompleted() => observer.OnCompleted();
    }

    /// <summary>
    /// Subscribes to converted state changes using a forwarding observer.
    /// </summary>
    /// <param name="observer">Observer to receive converted state notifications.</param>
    /// <returns>Disposable subscription.</returns>
    public IDisposable Subscribe(IObserver<TTo> observer)
    {
        return originalState.Subscribe(new ForwardingObserver(observer, forward));
    }

    /// <summary>Disposes the original state.</summary>
    public void Dispose() => originalState.Dispose();

    /// <summary>Creates an effect trigger from the original state.</summary>
    public IEffectTrigger ToTrigger() => originalState.ToTrigger();

    /// <summary>Subscribes to any changes in the original state.</summary>
    public IDisposable SubscribeAny(Action action) => originalState.SubscribeAny(action);

    /// <summary>Subscribes to any changes in the original state with value.</summary>
    public IDisposable SubscribeAny(Action<object?> action) => originalState.SubscribeAny(action);

    /// <summary>Gets the converted state type.</summary>
    public Type GetStateType() => typeof(TTo);

    /// <summary>
    /// Gets or sets the converted state value, applying transformations as needed.
    /// </summary>
    public TTo Value
    {
        get => forward(originalState.Value);
        set => originalState.Value = backward(value);
    }
}