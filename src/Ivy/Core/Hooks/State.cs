using System.Reactive.Subjects;
using System.Runtime.CompilerServices;

namespace Ivy.Core.Hooks;

public class State<T> : IState<T>, IRef<T>
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

    [OverloadResolutionPriority(1)]
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
            var subscription = _subject.Subscribe(observer);
            observer.OnNext(_value);
            return subscription;
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
        return EffectTrigger.OnStateChange(this);
    }
}
