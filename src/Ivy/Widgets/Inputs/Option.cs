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
}

public class Option<TValue> : IAnyOption
{
    public Option(TValue value) : this(value?.ToString() ?? "?", value, null)
    {
    }

    internal Option()
    {
        Value = null!;
    }

    public Option(string? label, TValue value, string? group = null, string? description = null, Icons? icon = null, bool disabled = false)
    {
        Label = label;
        Description = description;
        Value = value!;
        Group = group;
        Icon = icon;
        Disabled = disabled;
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
            var label = ((Enum)e).GetDescription();
            var value = Convert.ChangeType(e, enumType);
            return MakeOptionDynamic(label, (dynamic)value);
        }

        return Enum.GetValues(enumType).Cast<object>().Select(MakeOption).ToArray();
    }

    private static IAnyOption MakeOptionDynamic<TValue>(string label, TValue value)
    {
        return new Option<TValue>(label, value, null, null, null, false);
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