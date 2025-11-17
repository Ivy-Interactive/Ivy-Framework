using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy.Widgets.Inputs;

/// <typeparam name="T">e.g., string, int, bool, DateTime, etc.</typeparam>
public interface IInput<T> : IAnyInput
{
    [Prop] public T Value { get; }

    [Event] public Func<Event<IInput<T>, T>, ValueTask>? OnChange { get; }

    /// <exception cref="NotSupportedException">Always thrown as input controls do not support child elements.</exception>
    public static IInput<T> operator |(IInput<T> input, object child)
    {
        throw new NotSupportedException("IInput does not support children.");
    }
}