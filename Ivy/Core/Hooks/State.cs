using System.Reactive.Subjects;

namespace Ivy.Core.Hooks;

/// <summary>
/// Base interface for reactive state objects that can trigger effects and be observed.
/// </summary>
public interface IAnyState : IDisposable, IEffectTriggerConvertible
{
    /// <summary>
    /// Subscribes to state changes without receiving the new value.
    /// </summary>
    /// <param name="action">Action to execute when state changes.</param>
    /// <returns>Disposable subscription.</returns>
    public IDisposable SubscribeAny(Action action);

    /// <summary>
    /// Subscribes to state changes and receives the new value as object.
    /// </summary>
    /// <param name="action">Action to execute when state changes, receiving the new value.</param>
    /// <returns>Disposable subscription.</returns>
    public IDisposable SubscribeAny(Action<object?> action);

    /// <summary>
    /// Gets the type of the state value.
    /// </summary>
    /// <returns>The type of the state value.</returns>
    public Type GetStateType();
}

/// <summary>
/// Generic interface for reactive state objects with typed value access and modification.
/// </summary>
/// <typeparam name="T">The type of the state value.</typeparam>
public interface IState<T> : IObservable<T>, IAnyState
{
    public T Value { get; set; }

    /// <summary>
    /// Sets the state value and returns the new value.
    /// </summary>
    /// <param name="value">The new value to set.</param>
    /// <returns>The new state value.</returns>
    public T Set(T value);

    /// <summary>
    /// Updates the state value using a setter function and returns the new value.
    /// </summary>
    /// <param name="setter">Function that takes the current value and returns the new value.</param>
    /// <returns>The new state value.</returns>
    public T Set(Func<T, T> setter);

    /// <summary>
    /// Resets the state to its default value.
    /// </summary>
    /// <returns>The default value.</returns>
    public T Reset();
}

/// <summary>
/// Concrete implementation of reactive state using RxNET Subject for change notifications.
/// </summary>
/// <typeparam name="T">The type of the state value.</typeparam>
public class State<T> : IState<T>
{
    private T _value;
    private readonly Subject<T> _subject = new();
    private readonly object _lock = new();

    /// <summary>
    /// Creates a new state instance with the specified initial value.
    /// </summary>
    /// <param name="initialValue">The initial value for the state.</param>
    public State(T initialValue)
    {
        _value = initialValue;
    }

    /// <summary>
    /// Gets or sets the current state value, notifying observers when changed.
    /// </summary>
    public T Value
    {
        get
        {
            lock (_lock)
            {
                return _value;
            }
        }
        set
        {
            T? newValue = default;
            bool changed = false;
            lock (_lock)
            {
                if (!Equals(_value, value))
                {
                    _value = value;
                    newValue = _value;
                    changed = true;
                }
            }
            if (changed && !_subject.IsDisposed)
            {
                _subject.OnNext(newValue!);
            }
        }
    }

    /// <summary>
    /// Sets the state value and returns the new value.
    /// Thread-safe.
    /// </summary>
    /// <param name="value">The new value to set.</param>
    /// <returns>The new state value.</returns>
    public T Set(T value)
    {
        Value = value;
        return Value;
    }

    /// <summary>
    /// Updates the state value using a setter function and returns the new value.
    /// Thread-safe: the entire read-modify-write operation is atomic.
    /// </summary>
    /// <param name="setter">Function that takes the current value and returns the new value.</param>
    /// <returns>The new state value.</returns>
    public T Set(Func<T, T> setter)
    {
        T current;
        T updated;
        bool changed;
        lock (_lock)
        {
            current = _value;
            updated = setter(current);
            changed = !Equals(_value, updated);
            if (changed)
            {
                _value = updated;
            }
        }
        if (changed && !_subject.IsDisposed)
        {
            _subject.OnNext(updated);
        }
        return updated;
    }

    /// <summary>
    /// Resets the state to its default value.
    /// Thread-safe.
    /// </summary>
    /// <returns>The default value.</returns>
    public T Reset()
    {
        return Set(default(T)!);
    }

    /// <summary>
    /// Subscribes to state changes and immediately receives the current value.
    /// </summary>
    /// <param name="observer">Observer to receive state change notifications.</param>
    /// <returns>Disposable subscription.</returns>
    public IDisposable Subscribe(IObserver<T> observer)
    {
        lock (_lock)
        {
            observer.OnNext(_value);
            return _subject.Subscribe(observer);
        }
    }

    /// <summary>
    /// Disposes the state and its underlying subject.
    /// </summary>
    public void Dispose()
    {
        _subject.Dispose();
    }

    /// <summary>
    /// Subscribes to state changes without receiving the new value.
    /// </summary>
    /// <param name="action">Action to execute when state changes.</param>
    /// <returns>Disposable subscription.</returns>
    public IDisposable SubscribeAny(Action action)
    {
        return _subject.Subscribe(_ => action());
    }

    /// <summary>
    /// Subscribes to state changes and receives the new value as object.
    /// </summary>
    /// <param name="action">Action to execute when state changes, receiving the new value.</param>
    /// <returns>Disposable subscription.</returns>
    public IDisposable SubscribeAny(Action<object?> action)
    {
        return _subject.Subscribe(x => action(x));
    }

    /// <summary>
    /// Gets the type of the state value.
    /// </summary>
    /// <returns>The type of the state value.</returns>
    public Type GetStateType()
    {
        return typeof(T);
    }

    /// <summary>
    /// Returns the string representation of the current state value.
    /// </summary>
    /// <returns>String representation of the current value.</returns>
    public override string? ToString()
    {
        return _value?.ToString();
    }

    /// <summary>
    /// Creates an effect trigger that fires when this state changes.
    /// </summary>
    /// <returns>Effect trigger for state changes.</returns>
    public IEffectTrigger ToTrigger()
    {
        return EffectTrigger.AfterChange(this);
    }
}
