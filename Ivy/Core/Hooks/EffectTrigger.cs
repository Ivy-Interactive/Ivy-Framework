namespace Ivy.Core.Hooks;

public class EffectTrigger : IEffectTrigger
{
    public EffectTriggerType Type { get; }
    public IAnyState? State { get; }

    private EffectTrigger(EffectTriggerType type, IAnyState? state)
    {
        Type = type;
        State = state;
    }

    public static EffectTrigger AfterChange(IAnyState state) =>
        new(EffectTriggerType.AfterChange, state);

    public static EffectTrigger AfterInit() =>
        new(EffectTriggerType.AfterInit, null);

    public static EffectTrigger AfterRender() =>
        new(EffectTriggerType.AfterRender, null);

    public IEffectTrigger ToTrigger()
    {
        return this;
    }
}


public static class EffectExtensions
{
    private class ObservableState<T> : IAnyState
    {
        private readonly IObservable<T> _observable;

        internal ObservableState(IObservable<T> observable)
        {
            _observable = observable;
        }

        public void Dispose()
        {
            //nothing to dispose
        }

        public IEffectTrigger ToTrigger()
        {
            throw new NotImplementedException();
        }

        public IDisposable SubscribeAny(Action action)
        {
            return _observable.Subscribe(_ => action());
        }

        public IDisposable SubscribeAny(Action<object?> action)
        {
            return _observable.Subscribe(x => action(x));
        }

        public Type GetStateType()
        {
            return typeof(T);
        }
    }

    public static IEffectTrigger ToTrigger<T>(this IObservable<T> observable)
    {
        return EffectTrigger.AfterChange(new ObservableState<T>(observable));
    }

    //make a explicit conversion from IObservable to IEffectTriggerConvertible

}

