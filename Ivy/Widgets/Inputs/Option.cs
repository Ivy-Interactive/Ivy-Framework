using System.ComponentModel;
using Ivy.Shared;

namespace Ivy.Widgets.Inputs;

/// <summary>Interface for option items used in select inputs, dropdowns, and other choice-based controls.</summary>
public interface IAnyOption
{
    /// <summary>Gets type of option's value for type checking and validation in generic selection controls.</summary>
    public Type GetOptionType();

    /// <summary>Gets or sets the display label for the option.</summary>
    public string Label { get; set; }

    /// <summary>Gets or sets the group name for organizing related options.</summary>
    public string? Group { get; set; }

    /// <summary>Gets or sets the underlying value of the option.</summary>
    public object Value { get; set; }
}

/// <summary>Generic option class providing type-safe option items for selection controls.</summary>
/// <typeparam name="TValue">Type of option's value.</typeparam>
public class Option<TValue>(string label, TValue value, string? group = null) : IAnyOption
{
    /// <summary>Initializes option with automatic label generation from value.</summary>
    /// <param name="value">Value to use for both option value and automatic label generation.</param>
    public Option(TValue value) : this(value?.ToString() ?? "?", value, null)
    {
    }

    /// <summary>Gets type of option's value returning generic type parameter TValue for type checking and validation.</summary>
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

/// <summary>Extension methods for creating and converting option collections.</summary>
public static class OptionExtensions
{
    /// <summary>Converts enumerable collection of values to array of strongly-typed options.</summary>
    /// <typeparam name="TValue">Type of values in collection.</typeparam>
    /// <param name="options">Collection of values to convert to options.</param>
    public static Option<TValue>[] ToOptions<TValue>(this IEnumerable<TValue> options)
    {
        return options.Select(e => new Option<TValue>(e)).ToArray();
    }

    /// <summary>Converts enum type to array of options with intelligent label generation.</summary>
    /// <param name="enumType">Enum type to convert to options.</param>
    /// <exception cref="ArgumentException">Thrown when provided type is not an enum.</exception>
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

    /// <summary>Converts collection of options to menu items for use in menu controls.</summary>
    /// <param name="options">Collection of options to convert to menu items.</param>
    public static MenuItem[] ToMenuItems(this IEnumerable<IAnyOption> options)
    {
        return options.Select(e => MenuItem.Default(e.Label, e.Value)).ToArray();
    }

}