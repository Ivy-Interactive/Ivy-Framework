namespace Ivy.Core.Hooks;

public class ConvertedState<TFrom, TTo>(IState<TFrom> originalState, Func<TFrom, TTo> forward, Func<TTo, TFrom> backward) : IState<TTo>
{
    private class ForwardingObserver(IObserver<TTo> observer, Func<TFrom, TTo> forward) : IObserver<TFrom>
    {
        public void OnNext(TFrom value) => observer.OnNext(forward(value));
        public void OnError(Exception error) => observer.OnError(error);
        public void OnCompleted() => observer.OnCompleted();
    }

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
}