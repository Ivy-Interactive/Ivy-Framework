using Ivy.Core.Hooks;

namespace Ivy.Widgets.Inputs;

public static class InputExtensions
{
    public static IInput<T> ToInput<T>(this IState<T> state, string? placeholder = null, bool disabled = false)
    {
        //todo: Can we detect the name of the state using [?] attribute

        return typeof(T) switch
        {
            //numbers:
            { } t when t == typeof(int) => new NumberInput<T>(state, placeholder, disabled),
            { } t when t == typeof(int?) => new NumberInput<T>(state, placeholder, disabled),
            { } t when t == typeof(double) => new NumberInput<T>(state, placeholder, disabled),
            { } t when t == typeof(double?) => new NumberInput<T>(state, placeholder, disabled),
            { } t when t == typeof(long) => new NumberInput<T>(state, placeholder, disabled),
            { } t when t == typeof(long?) => new NumberInput<T>(state, placeholder, disabled),
            { } t when t == typeof(short) => new NumberInput<T>(state, placeholder, disabled),
            { } t when t == typeof(short?) => new NumberInput<T>(state, placeholder, disabled),
            { } t when t == typeof(float) => new NumberInput<T>(state, placeholder, disabled),
            { } t when t == typeof(float?) => new NumberInput<T>(state, placeholder, disabled),
            { } t when t == typeof(decimal) => new NumberInput<T>(state, placeholder, disabled),
            { } t when t == typeof(decimal?) => new NumberInput<T>(state, placeholder, disabled),

            //bools:
            { } t when t == typeof(bool) => new BoolInput<T>(state, placeholder, disabled),
            { } t when t == typeof(bool?) => new BoolInput<T>(state, placeholder, disabled),

            //strings:
            { } t when t == typeof(string) => new TextInput<T>(state, placeholder, disabled),

            //dates:
            { } t when t == typeof(DateTime) => new DateTimeInput<T>(state, placeholder, disabled),
            { } t when t == typeof(DateTime?) => new DateTimeInput<T>(state, placeholder, disabled),
            { } t when t == typeof(DateOnly) => new DateTimeInput<T>(state, placeholder, disabled),
            { } t when t == typeof(DateOnly?) => new DateTimeInput<T>(state, placeholder, disabled),
            { } t when t == typeof(DateTimeOffset) => new DateTimeInput<T>(state, placeholder, disabled),
            { } t when t == typeof(DateTimeOffset?) => new DateTimeInput<T>(state, placeholder, disabled),

            //todo:enums and IEnumerable

            //colors:
            //{ } t when t == typeof(Color) => new ColorInput<T>(state, placeholder, disabled),

            _ => throw new InvalidOperationException($"Invalid state type: {state.GetType()} for ToInput conversion.")
        };
    }
}