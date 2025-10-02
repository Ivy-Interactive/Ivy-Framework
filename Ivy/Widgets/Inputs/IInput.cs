using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy.Widgets.Inputs;

/// <summary>
/// Generic interface for type-safe input controls that extends IAnyInput with strongly-typed value and event handling.
/// Provides a contract for input controls that work with specific data types while maintaining type safety
/// and consistent event handling patterns across the input system.
/// </summary>
/// <typeparam name="T">The type of the input value (e.g., string, int, bool, DateTime, etc.).</typeparam>
public interface IInput<T> : IAnyInput
{
    /// <summary>Gets the current value of the input control.</summary>
    [Prop] public T Value { get; }

    /// <summary>Gets the event handler called when the input value changes.</summary>
    [Event] public Func<Event<IInput<T>, T>, ValueTask>? OnChange { get; }

    /// <summary>
    /// Operator overload that prevents adding child elements to input controls.
    /// Input controls are leaf elements in the widget tree and cannot contain children.
    /// </summary>
    /// <param name="input">The input control.</param>
    /// <param name="child">The attempted child element.</param>
    /// <exception cref="NotSupportedException">Always thrown as input controls do not support child elements.</exception>
    public static IInput<T> operator |(IInput<T> input, object child)
    {
        throw new NotSupportedException("IInput does not support children.");
    }
}