using System.Reactive.Subjects;

namespace Ivy.Core.Hooks;

public interface IAnyState : IDisposable, IEffectTriggerConvertible
{
    public IDisposable SubscribeAny(Action action);

    public IDisposable SubscribeAny(Action<object?> action);

    public Type GetStateType();
}

public interface IState<T> : IObservable<T>, IAnyState
{
    public T Value { get; set; }

    public T Set(T value);

    public T Set(Func<T, T> setter);

    public T Reset();
}

public class State<T> : IState<T>
{
    private T _value;
    private readonly Subject<T> _subject = new();
    private readonly object _lock = new();

    public State(T initialValue)
    {
        _value = initialValue;
    }

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

    public T Set(T value)
    {
        Value = value;
        return Value;
    }

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

    public T Reset()
    {
        return Set(default(T)!);
    }

    public IDisposable Subscribe(IObserver<T> observer)
    {
        lock (_lock)
        {
            observer.OnNext(_value);
            return _subject.Subscribe(observer);
        }
    }

    public void Dispose()
    {
        _subject.Dispose();
    }

    public IDisposable SubscribeAny(Action action)
    {
        return _subject.Subscribe(_ => action());
    }

    public IDisposable SubscribeAny(Action<object?> action)
    {
        return _subject.Subscribe(x => action(x));
    }

    public Type GetStateType()
    {
        return typeof(T);
    }

    public override string? ToString()
    {
        return _value?.ToString();
    }

    public IEffectTrigger ToTrigger()
    {
        return EffectTrigger.AfterChange(this);
    }
}
