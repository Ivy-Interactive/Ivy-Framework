using Ivy.Core.Hooks;
using Ivy.Shared;

namespace Ivy.Widgets.Inputs;

/// <summary>
/// Provides extension methods for automatic input control creation based on state types.
/// Enables intelligent input control selection by analyzing the generic type parameter
/// and creating the most appropriate input control for the data type.
/// </summary>
public static class InputExtensions
{
    /// <summary>
    /// Automatically creates the most appropriate input control based on the state's type parameter.
    /// Uses type analysis to select between NumberInput, BoolInput, TextInput, DateTimeInput, and ColorInput
    /// controls, providing intelligent defaults for common data types.
    /// </summary>
    /// <typeparam name="T">The type of the state value, used to determine the appropriate input control.</typeparam>
    /// <param name="state">The state object to bind the input control to.</param>
    /// <param name="placeholder">Optional placeholder text displayed when the input is empty.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <exception cref="InvalidOperationException">Thrown when no suitable input control exists for the specified type.</exception>
    /// <remarks>
    /// <para>Supported type mappings:</para>
    /// <list type="bullet">
    /// <item><description><strong>Numeric types:</strong> int, double, long, short, float, decimal (and nullable variants) → NumberInput</description></item>
    /// <item><description><strong>Boolean types:</strong> bool, bool? → BoolInput</description></item>
    /// <item><description><strong>String type:</strong> string → TextInput</description></item>
    /// <item><description><strong>Date/Time types:</strong> DateTime, DateOnly, DateTimeOffset (and nullable variants) → DateTimeInput</description></item>
    /// <item><description><strong>Color types:</strong> Colors, Colors? → ColorInput</description></item>
    /// </list>
    /// <para>Future enhancements may include support for enums and IEnumerable types.</para>
    /// </remarks>
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
            { } t when t == typeof(Colors) => new ColorInput<T>(state, placeholder, disabled),
            { } t when t == typeof(Colors?) => new ColorInput<T>(state, placeholder, disabled),

            _ => throw new InvalidOperationException($"Invalid state type: {state.GetType()} for ToInput conversion.")
        };
    }
}