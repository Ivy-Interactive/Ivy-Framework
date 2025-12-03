using System.ComponentModel;
using Ivy.Shared;

namespace Ivy.Widgets.Inputs;

public interface IAnyOption
{
    public Type GetOptionType();

    public string? Label { get; set; }

    public string? Description { get; set; }

    public string? Group { get; set; }

    public object Value { get; set; }

    public Icons? Icon { get; set; }
}

public class Option<TValue>(string? label, TValue value, string? group = null, string? description = null, Icons? icon = null) : IAnyOption
{
    public Option(TValue value) : this(value?.ToString() ?? "?", value, null)
    {
    }

    public Type GetOptionType()
    {
        return typeof(TValue);
    }

    public string? Label { get; set; } = label;

    public string? Description { get; set; } = description;

    public object Value { get; set; } = value!;

    public TValue TypedValue => (TValue)Value;

    public string? Group { get; set; } = group;

    public Icons? Icon { get; set; } = icon;
}

public static class OptionExtensions
{
    public static Option<TValue>[] ToOptions<TValue>(this IEnumerable<TValue> options)
    {
        return options.Select(e => new Option<TValue>(e)).ToArray();
    }

    public static IAnyOption[] ToOptions(this Type enumType)
    {
        if (!enumType.IsEnum)
            throw new ArgumentException("Type must be an enum", nameof(enumType));

        IAnyOption MakeOption(object e)
        {
            var label = enumType.GetField(e.ToString()!)?
                .GetCustomAttributes(typeof(DescriptionAttribute), false)
                .Cast<DescriptionAttribute>()
                .FirstOrDefault()?.Description ?? Utils.SplitPascalCase(e.ToString());

            return (IAnyOption)Activator.CreateInstance(
                typeof(Option<>).MakeGenericType(enumType),
                label,
                Convert.ChangeType(e, enumType),
                null,
                null
            )!;
        }

        return Enum.GetValues(enumType).Cast<object>().Select(MakeOption).ToArray();
    }

    public static MenuItem[] ToMenuItems(this IEnumerable<IAnyOption> options)
    {
        return options.Select(e => MenuItem.Default(e.Label, e.Value)).ToArray();
    }
}