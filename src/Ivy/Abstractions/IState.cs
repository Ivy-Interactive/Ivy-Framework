using System.Runtime.CompilerServices;

// Resharper disable once CheckNamespace
namespace Ivy;

public interface IAnyState : IDisposable, IEffectTriggerConvertible
{
    public IDisposable SubscribeAny(Action action);

    public IDisposable SubscribeAny(Action<object?> action);

    public Type GetStateType();

    public object? GetValueAsObject();
}

public interface IState<T> : IObservable<T>, IAnyState
{
    public T Value { get; set; }

    [OverloadResolutionPriority(1)]
    public T Set(T value);

    public T Set(Func<T, T> setter);

    public T Reset();
}

public interface IRef<T> : IState<T>;