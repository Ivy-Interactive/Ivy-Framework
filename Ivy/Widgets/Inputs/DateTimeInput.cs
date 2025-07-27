using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;
using Ivy.Widgets.Inputs;

// ReSharper disable once CheckNamespace
namespace Ivy;

public enum DateTimeInputs
{
    Date,
    DateTime,
    Time
}

public interface IAnyDateTimeInput : IAnyInput
{
    public DateTimeInputs Variant { get; set; }
    public string? Placeholder { get; set; }
    public string? Format { get; set; }
}

public abstract record DateTimeInputBase : WidgetBase<DateTimeInputBase>, IAnyDateTimeInput
{
    [Prop] public DateTimeInputs Variant { get; set; }
    [Prop] public string? Placeholder { get; set; }
    [Prop] public string? Format { get; set; }
    [Prop] public bool Disabled { get; set; }
    [Prop] public string? Invalid { get; set; }
    [Event] public Action<Event<IAnyInput>>? OnBlur { get; set; }

    public Type[] SupportedStateTypes() =>
    [
        // DateTime types
        typeof(DateTime), typeof(DateTime?),
        typeof(DateTimeOffset), typeof(DateTimeOffset?),
        typeof(DateOnly), typeof(DateOnly?),
        typeof(TimeOnly), typeof(TimeOnly?),
    ];
}

public record DateTimeInput<TDate> : DateTimeInputBase, IInput<TDate>
{
    public DateTimeInput(IAnyState state, string? placeholder = null, bool disabled = false, DateTimeInputs variant = DateTimeInputs.Date) : this(placeholder, disabled, variant)
    {
        var typedState = state.As<TDate>();
        Value = typedState.Value;
        OnChange = e => typedState.Set(e.Value);
    }

    public DateTimeInput(TDate value, Action<Event<IInput<TDate>, TDate>> onChange, string? placeholder = null, bool disabled = false, DateTimeInputs variant = DateTimeInputs.Date) : this(placeholder, disabled, variant)
    {
        OnChange = onChange;
        Value = value;
    }

    public DateTimeInput(string? placeholder = null, bool disabled = false, DateTimeInputs variant = DateTimeInputs.Date)
    {
        Variant = variant;
        Placeholder = placeholder;
        Disabled = disabled;
    }

    [Prop] public TDate Value { get; set; } = default!;
    [Prop] public bool Nullable { get; set; } = typeof(TDate) == typeof(DateTime?) || typeof(TDate) == typeof(DateTimeOffset?) || typeof(TDate) == typeof(DateOnly?) || typeof(TDate) == typeof(TimeOnly?);
    [Event] public Action<Event<IInput<TDate>, TDate>>? OnChange { get; set; }
}

public static class DateTimeInputExtensions
{
    public static DateTimeInputBase ToDateInput(this IAnyState state, string? placeholder = null, bool disabled = false,
        DateTimeInputs variant = DateTimeInputs.Date)
        => ToDateTimeInput(state, placeholder, disabled, variant);

    public static DateTimeInputBase ToDateTimeInput(this IAnyState state, string? placeholder = null, bool disabled = false, DateTimeInputs variant = DateTimeInputs.DateTime)
    {
        var stateType = state.GetStateType();
        var isNullable = stateType.IsNullableType();

        // Create the appropriate DateTimeInput based on the original state type
        if (isNullable)
        {
            var dateValue = ConvertToDateValue<object?>(state);
            var input = new DateTimeInput<object?>(dateValue, e => SetStateValue(state, e.Value), placeholder, disabled, variant);
            input.ScaffoldDefaults(null!, stateType);
            input.Nullable = true;
            return input;
        }
        else
        {
            var dateValue = ConvertToDateValue<object>(state);
            var input = new DateTimeInput<object>(dateValue, e => SetStateValue(state, e.Value), placeholder, disabled, variant);
            input.ScaffoldDefaults(null!, stateType);
            return input;
        }
    }

    private static T ConvertToDateValue<T>(IAnyState state)
    {
        var stateType = state.GetStateType();
        var value = state.As<object>().Value;

        // Convert to appropriate date value based on type
        var dateValue = stateType switch
        {
            // DateTime types - direct conversion
            _ when stateType == typeof(DateTime) => value,
            _ when stateType == typeof(DateTime?) => value,
            _ when stateType == typeof(DateTimeOffset) => value,
            _ when stateType == typeof(DateTimeOffset?) => value,
            _ when stateType == typeof(DateOnly) => value,
            _ when stateType == typeof(DateOnly?) => value,
            _ when stateType == typeof(TimeOnly) => value,
            _ when stateType == typeof(TimeOnly?) => value,

            // Other types - try BestGuessConvert, fallback to current date
            _ => Core.Utils.BestGuessConvert(value, typeof(DateTime)) ?? DateTime.Now
        };

        return (T)dateValue!;
    }

    private static void SetStateValue(IAnyState state, object? dateValue)
    {
        var stateType = state.GetStateType();
        var isNullable = stateType.IsNullableType();

        // Convert date value back to the original state type
        var convertedValue = stateType switch
        {
            // DateTime types - direct conversion
            _ when stateType == typeof(DateTime) =>
                dateValue is DateTime dt ? dt :
                dateValue is string s ? DateTime.Parse(s) :
                DateTime.Now,
            _ when stateType == typeof(DateTime?) =>
                dateValue is null ? null :
                dateValue is DateTime dt ? dt :
                dateValue is string s ? DateTime.Parse(s) :
                (DateTime?)DateTime.Now,
            _ when stateType == typeof(DateTimeOffset) => dateValue ?? DateTimeOffset.Now,
            _ when stateType == typeof(DateTimeOffset?) => dateValue,
            _ when stateType == typeof(DateOnly) =>
                dateValue is DateOnly d ? d :
                dateValue is string s ? DateOnly.FromDateTime(DateTime.Parse(s)) :
                dateValue is DateTime dt ? DateOnly.FromDateTime(dt) :
                DateOnly.FromDateTime(DateTime.Now),
            _ when stateType == typeof(DateOnly?) =>
                dateValue is null ? null :
                dateValue is DateOnly d ? d :
                dateValue is string s ? DateOnly.FromDateTime(DateTime.Parse(s)) :
                dateValue is DateTime dt ? DateOnly.FromDateTime(dt) :
                (DateOnly?)DateOnly.FromDateTime(DateTime.Now),
            _ when stateType == typeof(TimeOnly) =>
                dateValue is TimeOnly t ? t :
                dateValue is string s ? ParseTimeOnly(s) :
                dateValue is DateTime dt ? TimeOnly.FromDateTime(dt) :
                TimeOnly.FromDateTime(DateTime.Now),
            _ when stateType == typeof(TimeOnly?) =>
                dateValue is null ? null :
                dateValue is string s && string.IsNullOrWhiteSpace(s) ? null :
                dateValue is TimeOnly t ? t :
                dateValue is string s2 ? ParseTimeOnly(s2) :
                dateValue is DateTime dt ? TimeOnly.FromDateTime(dt) :
                (TimeOnly?)TimeOnly.FromDateTime(DateTime.Now),
            _ when stateType == typeof(string) => dateValue?.ToString() ?? DateTime.Now.ToString("O"),

            // Other types - use BestGuessConvert
            _ => Core.Utils.BestGuessConvert(dateValue, stateType) ?? DateTime.Now
        };

        // Set the state value
        state.As<object>().Set(convertedValue!);
    }

    private static TimeOnly ParseTimeOnly(string timeString)
    {
        // Try different time formats
        var formats = new[] { "HH:mm:ss", "HH:mm", "H:mm:ss", "H:mm" };

        foreach (var format in formats)
        {
            if (TimeOnly.TryParseExact(timeString, format, null, System.Globalization.DateTimeStyles.None, out var result))
            {
                return result;
            }
        }

        // If all parsing fails, return current time
        return TimeOnly.FromDateTime(DateTime.Now);
    }

    public static DateTimeInputBase ToTimeInput(this IAnyState state, string? placeholder = null, bool disabled = false)
        => state.ToDateTimeInput(placeholder, disabled, DateTimeInputs.Time);

    internal static IAnyDateTimeInput ScaffoldDefaults(this IAnyDateTimeInput input, string? name, Type type)
    {
        if (string.IsNullOrEmpty(input.Placeholder))
        {
            input.Placeholder = Utils.SplitPascalCase(name) ?? name;
        }

        return input;
    }

    public static DateTimeInputBase Disabled(this DateTimeInputBase widget, bool disabled = true) => widget with { Disabled = disabled };
    public static DateTimeInputBase Variant(this DateTimeInputBase widget, DateTimeInputs variant) => widget with { Variant = variant };
    public static DateTimeInputBase Placeholder(this DateTimeInputBase widget, string placeholder) => widget with { Placeholder = placeholder };
    public static DateTimeInputBase Format(this DateTimeInputBase widget, string format) => widget with { Format = format };
    public static DateTimeInputBase Invalid(this DateTimeInputBase widget, string? invalid) => widget with { Invalid = invalid };
}