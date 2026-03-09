using Ivy.Core.Hooks;

// Resharper disable once CheckNamespace
namespace Ivy;

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