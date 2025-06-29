namespace Ivy.Core.Hooks;

public interface IViewContext : IDisposable
{
    void TrackDisposable(IDisposable disposable);

    public void TrackDisposable(IEnumerable<IDisposable> disposables);

    void Reset();

    IState<T> UseState<T>(T? initialValue = default, bool buildOnChange = true);

    IState<T> UseState<T>(Func<T> buildInitialValue, bool buildOnChange = true);

    void UseEffect(Func<Task> handler, params IEffectTriggerConvertible[] triggers);

    void UseEffect(Func<Task<IDisposable>> handler, params IEffectTriggerConvertible[] triggers);

    void UseEffect(Func<IDisposable> handler, params IEffectTriggerConvertible[] triggers);

    void UseEffect(Action handler, params IEffectTriggerConvertible[] triggers);

    T CreateContext<T>(Func<T> factory);

    T UseContext<T>();

    object UseContext(Type serviceType);

    T UseService<T>();

    object UseService(Type serviceType);

    T? UseArgs<T>() where T : class;
}