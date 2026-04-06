using System.Reactive.Subjects;
using System.Runtime.CompilerServices;

namespace Ivy.Test;

internal class MockState<T>(T value) : IState<T>
{
    private readonly Subject<T> _subject = new();
    public T Value { get; set; } = value;

    [OverloadResolutionPriority(1)]
    public T Set(T value) { Value = value; return Value; }
    public T Set(Func<T, T> setter) { Value = setter(Value); return Value; }
    public T Reset() => Set(default(T)!);
    public Type GetStateType() => typeof(T);

    public IDisposable Subscribe(IObserver<T> observer)
    {
        observer.OnNext(Value);
        return _subject.Subscribe(observer);
    }

    public void Dispose() => _subject.Dispose();
    public IDisposable SubscribeAny(Action action) => _subject.Subscribe(_ => action());
    public IDisposable SubscribeAny(Action<object?> action) => _subject.Subscribe(x => action(x));
    public IEffectTrigger ToTrigger() => EffectTrigger.OnStateChange(this);
    public object? GetValueAsObject() => Value;
}
