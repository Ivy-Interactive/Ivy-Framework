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