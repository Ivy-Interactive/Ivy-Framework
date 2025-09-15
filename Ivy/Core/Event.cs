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

/// <summary>
/// Provides extension methods for converting action handlers to event handlers.
/// </summary>
public static class EventExtensions
{
    /// <summary>
    /// Converts a simple action handler to an event handler that ignores the event details.
    /// </summary>
    /// <typeparam name="TSender">The type of the event sender.</typeparam>
    /// <param name="handler">The action handler to convert, or null if no handler is needed.</param>
    /// <returns>An event handler that invokes the original action when called.</returns>
    [Obsolete("Use ValueTasks pattern instead.")]
    public static Action<Event<TSender>> ToEventHandler<TSender>(this Action? handler)
    {
        return _ => handler?.Invoke();
    }

    /// <summary>
    /// Converts a value-based action handler to an event handler that extracts the value.
    /// </summary>
    /// <typeparam name="TSender">The type of the event sender.</typeparam>
    /// <typeparam name="TValue">The type of the event value.</typeparam>
    /// <param name="handler">The value-based action handler to convert.</param>
    /// <returns>An event handler that extracts the value and invokes the original action.</returns>
    [Obsolete("Use ValueTasks pattern instead.")]
    public static Action<Event<TSender, TValue>> ToEventHandler<TSender, TValue>(this Action<TValue> handler)
    {
        return e => handler(e.Value);
    }
}
