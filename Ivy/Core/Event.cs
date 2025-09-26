namespace Ivy.Core;

/// <summary>
/// Represents a basic event with a sender and event name.
/// </summary>
/// <typeparam name="TSender">The type of the object that raised the event.</typeparam>
public class Event<TSender>(string eventName, TSender sender)
{
    /// <summary>
    /// Gets the name of the event that occurred.
    /// </summary>
    public string EventName { get; } = eventName;

    /// <summary>
    /// Gets the object that raised the event.
    /// </summary>
    public TSender Sender { get; } = sender;
}

/// <summary>
/// Represents an event with a sender, event name, and associated value.
/// </summary>
/// <typeparam name="TSender">The type of the object that raised the event.</typeparam>
/// <typeparam name="TValue">The type of the value associated with the event.</typeparam>
public class Event<TSender, TValue>(string eventName, TSender sender, TValue value)
    : Event<TSender>(eventName, sender)
{
    /// <summary>
    /// Gets the value associated with the event.
    /// </summary>
    public TValue Value { get; } = value;
}