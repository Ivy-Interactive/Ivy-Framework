namespace Ivy.Core;

public class Event<TSender>(string eventName, TSender sender)
{
    public string EventName { get; } = eventName;
    public TSender Sender { get; } = sender;
}

public class Event<TSender, TValue>(string eventName, TSender sender, TValue value)
    : Event<TSender>(eventName, sender)
{
    public TValue Value { get; } = value;
}

public static class EventExtensions
{
    public static Action<Event<TSender>> ToEventHandler<TSender>(this Action? handler)
    {
        return _ => handler?.Invoke();
    }

    public static Action<Event<TSender, TValue>> ToEventHandler<TSender, TValue>(this Action<TValue> handler)
    {
        return e => handler(e.Value);
    }
}
