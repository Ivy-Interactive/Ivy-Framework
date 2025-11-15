using System.ComponentModel;
using Ivy.Shared;

namespace Ivy.Widgets.Inputs;

public interface IAnyOption
{
    public Type GetOptionType();

    public string Label { get; set; }

    public string? Description { get; set; }

    public string? Group { get; set; }

    public object Value { get; set; }
}

/// <typeparam name="TValue">Type of option's value.</typeparam>
public class Option<TValue>(string label, TValue value, string? group = null, string? description = null) : IAnyOption
{
    public Option(TValue value) : this(value?.ToString() ?? "?", value, null)
    {
    }

    public Type GetOptionType()
    {
        return typeof(TValue);
    }

    public string Label { get; set; } = label;

    public string? Description { get; set; } = description;

    public object Value { get; set; } = value!;

    public TValue TypedValue => (TValue)Value;

    public string? Group { get; set; } = group;
}

public static class OptionExtensions
{
    /// <typeparam name="TValue">Type of values in collection.</typeparam>
    /// <param name="options">Collection of values to convert to options.</param>
    public static Option<TValue>[] ToOptions<TValue>(this IEnumerable<TValue> options)
    {
        return options.Select(e => new Option<TValue>(e)).ToArray();
    }

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

    /// <param name="options">Collection of options to convert to menu items.</param>
    public static MenuItem[] ToMenuItems(this IEnumerable<IAnyOption> options)
    {
        return options.Select(e => MenuItem.Default(e.Label, e.Value)).ToArray();
    }

}