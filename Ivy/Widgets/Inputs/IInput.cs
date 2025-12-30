using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy.Widgets.Inputs;

public interface IInput<T> : IAnyInput
{
    [Prop] public T Value { get; }

    [Event] public Func<Event<IInput<T>, T>, ValueTask>? OnChange { get; }

    public static IInput<T> operator |(IInput<T> input, object child)
    {
        throw new NotSupportedException("IInput does not support children.");
    }
}