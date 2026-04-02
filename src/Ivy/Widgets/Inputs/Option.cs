using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Ivy;

public interface IAnyOption
{
    public Type GetOptionType();

    public string? Label { get; set; }

    public string? Description { get; set; }

    public string? Group { get; set; }

    public object Value { get; set; }

    public Icons? Icon { get; set; }

    public bool Disabled { get; set; }

    public string? Tooltip { get; set; }
}

/// <summary>
/// Represents a selectable option with a display label and a typed value.
/// Used with <see cref="AsyncSelectInputView{TValue}"/>, SelectInput, and other selection widgets.
/// </summary>
/// <typeparam name="TValue">The type of the underlying value.</typeparam>
public class Option<TValue> : IAnyOption
{
    /// <summary>
    /// Creates an option using <c>value.ToString()</c> as the display label.
    /// </summary>
    /// <param name="value">The value for this option. Its <c>ToString()</c> result is used as the label.</param>
    public Option(TValue value) : this(value?.ToString() ?? "?", value, null)
    {
    }

    internal Option()
    {
        Value = null!;
    }

    /// <summary>
    /// Creates an option with an explicit label and value.
    /// </summary>
    /// <remarks>
    /// The parameter order is <c>(label, value)</c> — the label is what the user sees in the dropdown,
    /// and the value is what gets stored/returned when the option is selected.
    /// <example>
    /// <code>
    /// // label: "Germany", value: "DE"
    /// new Option&lt;string&gt;("Germany", "DE")
    ///
    /// // label: "John Doe", value: userId (Guid)
    /// new Option&lt;Guid&gt;("John Doe", userId, group: "Engineering")
    /// </code>
    /// </example>
    /// </remarks>
    /// <param name="label">The display text shown to the user in the dropdown.</param>
    /// <param name="value">The underlying value stored when this option is selected.</param>
    /// <param name="group">Optional group name for categorizing options.</param>
    /// <param name="description">Optional description shown below the label.</param>
    /// <param name="icon">Optional icon displayed next to the label.</param>
    /// <param name="disabled">Whether this option is disabled and cannot be selected.</param>
    /// <param name="tooltip">Optional tooltip shown on hover.</param>
    public Option(string? label, TValue value, string? group = null, string? description = null, Icons? icon = null, bool disabled = false, string? tooltip = null)
    {
        Label = label;
        Description = description;
        Value = value!;
        Group = group;
        Icon = icon;
        Disabled = disabled;
        Tooltip = tooltip;
    }

    public Type GetOptionType()
    {
        return typeof(TValue);
    }

    public string? Label { get; set; }

    public string? Description { get; set; }

    public object Value { get; set; }

    public TValue TypedValue => (TValue)Value;

    public string? Group { get; set; }

    public Icons? Icon { get; set; }

    public bool Disabled { get; set; }

    public string? Tooltip { get; set; }
}

public static class OptionExtensions
{
    public static Option<TValue>[] ToOptions<TValue>(this IEnumerable<TValue> options)
    {
        return options.Select(e =>
        {
            var label = e is Enum enumValue
                ? enumValue.GetDescription()
                : e?.ToString() ?? "?";
            return new Option<TValue>(label, e);
        }).ToArray();
    }

    public static IAnyOption[] ToOptions(this Type enumType)
    {
        if (!enumType.IsEnum)
            throw new ArgumentException("Type must be an enum", nameof(enumType));

        var optionType = typeof(Option<>).MakeGenericType(enumType);

        IAnyOption MakeOption(object e)
        {
            var label = ((Enum)e).GetDescription();
            var value = Convert.ChangeType(e, enumType);

            // Pass all 7 parameters including optional ones (label, value, group, description, icon, disabled, tooltip)
            return (IAnyOption)Activator.CreateInstance(optionType, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new object?[] { label, value, null, null, null, false, null }, null)!;
        }

        return Enum.GetValues(enumType).Cast<object>().Select(MakeOption).ToArray();
    }

    public static MenuItem[] ToMenuItems(this IEnumerable<IAnyOption> options)
    {
        return options.Select(e => MenuItem.Default(e.Label ?? "", e.Value)).ToArray();
    }

    public static Option<TValue> Disabled<TValue>(this Option<TValue> option, bool disabled = true)
    {
        option.Disabled = disabled;
        return option;
    }
}