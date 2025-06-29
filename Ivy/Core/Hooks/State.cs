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

    public T Set(T value)
    {
        Value = value;
        return Value;
    }

    public T Set(Func<T, T> setter)
    {
        Value = setter(Value);
        return Value;
    }
}

public class State<T> : IState<T>
{
    private T _value;
    private readonly Subject<T> _subject = new();

    public State(T initialValue)
    {
        _value = initialValue;
    }

    public T Value
    {
        get => _value;
        set
        {
            if (Equals(_value, value)) return;
            _value = value;
            if (!_subject.IsDisposed) _subject.OnNext(_value);
        }
    }

    public IDisposable Subscribe(IObserver<T> observer)
    {
        observer.OnNext(_value);
        return _subject.Subscribe(observer);
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