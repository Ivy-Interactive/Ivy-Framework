namespace Ivy;

/// <summary>
/// Wraps Func&lt;TEvent, ValueTask&gt; to allow extension methods with the same name as event properties.
/// Because this is not a delegate type, the compiler won't try to invoke it with () syntax and will
/// fall through to extension methods.
/// </summary>
public sealed class EventHandler<TEvent>(Func<TEvent, ValueTask> handler)
{
    public Func<TEvent, ValueTask> Handler { get; } = handler;

    public ValueTask Invoke(TEvent @event) => Handler(@event);

    public static implicit operator EventHandler<TEvent>(Func<TEvent, ValueTask> func) => new(func);
    public static implicit operator Func<TEvent, ValueTask>(EventHandler<TEvent> handler) => handler.Handler;
}
