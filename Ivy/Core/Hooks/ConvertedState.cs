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
        public void OnNext(TFrom value) => observer.OnNext(forward(value));
        public void OnError(Exception error) => observer.OnError(error);
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

    public void Dispose() => originalState.Dispose();

    public IEffectTrigger ToTrigger() => originalState.ToTrigger();

    public IDisposable SubscribeAny(Action action) => originalState.SubscribeAny(action);

    public IDisposable SubscribeAny(Action<object?> action) => originalState.SubscribeAny(action);

    public Type GetStateType() => typeof(TTo);

    public TTo Value
    {
        get => forward(originalState.Value);
        set => originalState.Value = backward(value);
    }

    /// <summary>
    /// Sets the state value and returns the new value.
    /// Thread-safe: delegates to the original state's Set method.
    /// </summary>
    /// <param name="value">The new value to set.</param>
    /// <returns>The new state value.</returns>
    public TTo Set(TTo value)
    {
        originalState.Set(backward(value));
        return value;
    }

    /// <summary>
    /// Updates the state value using a setter function and returns the new value.
    /// Thread-safe: the entire read-modify-write operation is atomic.
    /// </summary>
    /// <param name="setter">Function that takes the current value and returns the new value.</param>
    /// <returns>The new state value.</returns>
    public TTo Set(Func<TTo, TTo> setter)
    {
        var newValue = originalState.Set(from => backward(setter(forward(from))));
        return forward(newValue);
    }

    /// <summary>
    /// Resets the state to its default value.
    /// Thread-safe: delegates to the original state's Default method.
    /// </summary>
    /// <returns>The default value.</returns>
    public TTo Reset()
    {
        return Set(default(TTo)!);
    }
}