using System.ComponentModel;
using Ivy.Shared;

namespace Ivy.Widgets.Inputs;

/// <summary>
/// Interface for option items used in select inputs, dropdowns, and other choice-based controls.
/// </summary>
public interface IAnyOption
{
    /// <summary>
    /// Gets the type of the option's value.
    /// Used for type checking and validation in generic selection controls.
    /// </summary>
    public Type GetOptionType();

    /// <summary>Gets or sets the display label for the option.</summary>
    public string Label { get; set; }

    /// <summary>Gets or sets the group name for organizing related options.</summary>
    public string? Group { get; set; }

    /// <summary>Gets or sets the underlying value of the option.</summary>
    public object Value { get; set; }
}

/// <summary>
/// Generic option class that provides type-safe option items for selection controls.
/// </summary>
/// <typeparam name="TValue">The type of the option's value.</typeparam>
public class Option<TValue>(string label, TValue value, string? group = null) : IAnyOption
{
    /// <summary>
    /// Initializes a new option with automatic label generation from the value.
    /// </summary>
    /// <param name="value">The value to use for both the option value and automatic label generation.</param>
    public Option(TValue value) : this(value?.ToString() ?? "?", value, null)
    {
    }

    /// <summary>
    /// Gets the type of the option's value.
    /// Returns the generic type parameter TValue for type checking and validation.
    /// </summary>
    public Type GetOptionType()
    {
        return typeof(TValue);
    }

    /// <summary>Gets or sets the display label for the option.</summary>
    public string Label { get; set; } = label;

    /// <summary>Gets or sets the underlying value of the option as an object.</summary>
    public object Value { get; set; } = value!;

    /// <summary>Gets the strongly-typed value of the option.</summary>
    public TValue TypedValue => (TValue)Value;

    /// <summary>Gets or sets the group name for organizing related options.</summary>
    public string? Group { get; set; } = group;
}

/// <summary>
/// Provides extension methods for creating and converting option collections.
/// </summary>
public static class OptionExtensions
{
    /// <summary>
    /// Converts an enumerable collection of values to an array of strongly-typed options.
    /// Each value becomes both the option value and the display label (using ToString()).
    /// </summary>
    /// <typeparam name="TValue">The type of the values in the collection.</typeparam>
    /// <param name="options">The collection of values to convert to options.</param>
    public static Option<TValue>[] ToOptions<TValue>(this IEnumerable<TValue> options)
    {
        return options.Select(e => new Option<TValue>(e)).ToArray();
    }

    /// <summary>
    /// Converts an enum type to an array of options with intelligent label generation.
    /// </summary>
    /// <param name="enumType">The enum type to convert to options.</param>
    /// <exception cref="ArgumentException">Thrown when the provided type is not an enum.</exception>
    public static IAnyOption[] ToOptions(this Type enumType)
    {
        if (!enumType.IsEnum)
            throw new ArgumentException("Type must be an enum", nameof(enumType));

        IAnyOption MakeOption(object e)
        {
            var description = enumType.GetField(e.ToString()!)?
                .GetCustomAttributes(typeof(DescriptionAttribute), false)
                .Cast<DescriptionAttribute>()
                .FirstOrDefault()?.Description ?? Utils.SplitPascalCase(e.ToString());

            return (IAnyOption)Activator.CreateInstance(
                typeof(Option<>).MakeGenericType(enumType),
                description,
                Convert.ChangeType(e, enumType),
                null
            )!;
        }

        return Enum.GetValues(enumType).Cast<object>().Select(MakeOption).ToArray();
    }

    /// <summary>
    /// Converts a collection of options to menu items for use in menu controls.
    /// </summary>
    /// <param name="options">The collection of options to convert to menu items.</param>
    public static MenuItem[] ToMenuItems(this IEnumerable<IAnyOption> options)
    {
        return options.Select(e => MenuItem.Default(e.Label, e.Value)).ToArray();
    }

}