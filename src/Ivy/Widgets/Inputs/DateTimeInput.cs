using System.Runtime.CompilerServices;
using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;

// ReSharper disable once CheckNamespace
namespace Ivy;

public enum DateTimeInputVariant
{
    DateTime,
    Time,
    Date,
    Week,
    Month,
    Year
}

public interface IAnyDateTimeInput : IAnyInput
{
    public DateTimeInputVariant Variant { get; set; }

    public string? Format { get; set; }
}

public abstract record DateTimeInputBase : WidgetBase<DateTimeInputBase>, IAnyDateTimeInput
{
    [Prop] public DateTimeInputVariant Variant { get; set; } = DateTimeInputVariant.Date;

    [Prop] public string? Placeholder { get; set; }

    [Prop] public string? Format { get; set; }

    [Prop] public bool Disabled { get; set; }
    [Prop] public bool Nullable { get; set; }

    [Prop] public string? Invalid { get; set; }

    [Event] public EventHandler<Event<IAnyInput>>? OnBlur { get; set; }

    public Type[] SupportedStateTypes() =>
[
    typeof(DateTime), typeof(DateTime?),
        typeof(DateTimeOffset), typeof(DateTimeOffset?),
        typeof(DateOnly), typeof(DateOnly?),
        typeof(TimeOnly), typeof(TimeOnly?),
    ];
}

/// <summary>
/// An input for selecting both date and time.
/// </summary>
public record DateTimeInput<TDate> : DateTimeInputBase, IInput<TDate>
{
    [OverloadResolutionPriority(1)]
    public DateTimeInput(IAnyState state, string? placeholder = null, bool disabled = false, DateTimeInputVariant variant = DateTimeInputVariant.Date) : this(placeholder, disabled, variant)
    {
        var typedState = state.As<TDate>();
        Value = typedState.Value;
        OnChange = new(e => { typedState.Set(e.Value); return ValueTask.CompletedTask; });
    }

    [OverloadResolutionPriority(1)]
    public DateTimeInput(TDate value, Func<Event<IInput<TDate>, TDate>, ValueTask> onChange, string? placeholder = null, bool disabled = false, DateTimeInputVariant variant = DateTimeInputVariant.Date) : this(placeholder, disabled, variant)
    {
        OnChange = onChange.ToEventHandler();
        Value = value;
    }

    public DateTimeInput(TDate value, Action<Event<IInput<TDate>, TDate>> onChange, string? placeholder = null, bool disabled = false, DateTimeInputVariant variant = DateTimeInputVariant.Date) : this(placeholder, disabled, variant)
    {
        OnChange = new(e => { onChange(e); return ValueTask.CompletedTask; });
        Value = value;
    }

    public DateTimeInput(string? placeholder = null, bool disabled = false, DateTimeInputVariant variant = DateTimeInputVariant.Date)
    {
        Variant = variant;
        Placeholder = placeholder;
        Disabled = disabled;
    }

    internal DateTimeInput() { }

    [Prop] public TDate Value { get; init; } = default!;

    [Prop] public new bool Nullable { get; set; } = typeof(TDate) == typeof(DateTime?) || typeof(TDate) == typeof(DateTimeOffset?) || typeof(TDate) == typeof(DateOnly?) || typeof(TDate) == typeof(TimeOnly?);

    [Event] public EventHandler<Event<IInput<TDate>, TDate>>? OnChange { get; set; }
}

public static class DateTimeInputExtensions
{
    public static DateTimeInputBase ToDateTimeInput(this IAnyState state, string? placeholder = null, bool disabled = false, DateTimeInputVariant variant = DateTimeInputVariant.DateTime)
    {
        var stateType = state.GetStateType();
        var isNullable = stateType.IsNullableType();

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

    public static DateTimeInputBase ToDateInput(this IAnyState state, string? placeholder = null, bool disabled = false,
    DateTimeInputVariant variant = DateTimeInputVariant.Date)
        => ToDateTimeInput(state, placeholder, disabled, variant);

    public static DateTimeInputBase ToTimeInput(this IAnyState state, string? placeholder = null, bool disabled = false)
        => state.ToDateTimeInput(placeholder, disabled, DateTimeInputVariant.Time);

    public static DateTimeInputBase ToWeekInput(this IAnyState state, string? placeholder = null, bool disabled = false)
        => state.ToDateTimeInput(placeholder, disabled, DateTimeInputVariant.Week);

    public static DateTimeInputBase ToMonthInput(this IAnyState state, string? placeholder = null, bool disabled = false)
        => state.ToDateTimeInput(placeholder, disabled, DateTimeInputVariant.Month);

    public static DateTimeInputBase ToYearInput(this IAnyState state, string? placeholder = null, bool disabled = false)
        => state.ToDateTimeInput(placeholder, disabled, DateTimeInputVariant.Year);

    private static T ConvertToDateValue<T>(IAnyState state)
    {
        var stateType = state.GetStateType();
        var value = state.As<object>().Value;

        var dateValue = stateType switch
        {
            _ when stateType == typeof(DateTime) => value,
            _ when stateType == typeof(DateTime?) => value,
            _ when stateType == typeof(DateTimeOffset) => value,
            _ when stateType == typeof(DateTimeOffset?) => value,
            _ when stateType == typeof(DateOnly) => value,
            _ when stateType == typeof(DateOnly?) => value,
            _ when stateType == typeof(TimeOnly) => value,
            _ when stateType == typeof(TimeOnly?) => value,

            _ => Core.Utils.BestGuessConvert(value, typeof(DateTime)) ?? DateTime.Now
        };

        return (T)dateValue!;
    }

    private static void SetStateValue(IAnyState state, object? dateValue)
    {
        var stateType = state.GetStateType();
        var isNullable = stateType.IsNullableType();

        var convertedValue = stateType switch
        {
            _ when stateType == typeof(DateTime) =>
                dateValue is DateTime dt ? dt :
                dateValue is string s ? DateTime.Parse(s) :
                DateTime.Now,
            _ when stateType == typeof(DateTime?) =>
                dateValue is null ? null :
                dateValue is DateTime dt ? dt :
                dateValue is string s ? DateTime.Parse(s) :
                (DateTime?)DateTime.Now,
            _ when stateType == typeof(DateTimeOffset) =>
                dateValue is DateTimeOffset dto ? dto :
                dateValue is string s ? DateTimeOffset.Parse(s) :
                DateTimeOffset.Now,
            _ when stateType == typeof(DateTimeOffset?) =>
                dateValue is null ? null :
                dateValue is DateTimeOffset dto ? dto :
                dateValue is string s ? DateTimeOffset.Parse(s) :
                (DateTimeOffset?)DateTimeOffset.Now,
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

            _ => Core.Utils.BestGuessConvert(dateValue, stateType) ?? DateTime.Now
        };

        state.As<object>().Set(convertedValue!);
    }

    private static TimeOnly ParseTimeOnly(string timeString)
    {
        var formats = new[] { "HH:mm:ss", "HH:mm", "H:mm:ss", "H:mm" };

        foreach (var format in formats)
        {
            if (TimeOnly.TryParseExact(timeString, format, null, System.Globalization.DateTimeStyles.None, out var result))
            {
                return result;
            }
        }

        return TimeOnly.FromDateTime(DateTime.Now);
    }

    internal static IAnyDateTimeInput ScaffoldDefaults(this IAnyDateTimeInput input, string? name, Type type)
    {
        if (string.IsNullOrEmpty(input.Placeholder)
            && !string.IsNullOrEmpty(name))
        {
            input.Placeholder = Utils.LabelFor(name, type);
        }

        return input;
    }

    public static DateTimeInputBase Disabled(this DateTimeInputBase widget, bool disabled = true) => widget with { Disabled = disabled };

    public static DateTimeInputBase Variant(this DateTimeInputBase widget, DateTimeInputVariant variant) => widget with { Variant = variant };

    public static DateTimeInputBase Placeholder(this DateTimeInputBase widget, string placeholder) => widget with { Placeholder = placeholder };

    public static DateTimeInputBase Format(this DateTimeInputBase widget, string format) => widget with { Format = format };

    public static DateTimeInputBase Invalid(this DateTimeInputBase widget, string? invalid) => widget with { Invalid = invalid };
    public static DateTimeInputBase Nullable(this DateTimeInputBase widget, bool? nullable = true) => widget with { Nullable = nullable ?? true };

    [OverloadResolutionPriority(1)]
    public static DateTimeInputBase OnBlur(this DateTimeInputBase widget, Func<Event<IAnyInput>, ValueTask> onBlur)
    {
        return widget with { OnBlur = new(onBlur) };
    }

    public static DateTimeInputBase OnBlur(this DateTimeInputBase widget, Action<Event<IAnyInput>> onBlur)
    {
        return widget.OnBlur(onBlur.ToValueTask());
    }

    public static DateTimeInputBase OnBlur(this DateTimeInputBase widget, Action onBlur)
    {
        return widget.OnBlur(_ => { onBlur(); return ValueTask.CompletedTask; });
    }

    public static DateTimeInputBase Value<T>(this DateTimeInputBase widget, T value)
    {
        if (widget is DateTimeInput<T> typedWidget)
        {
            return typedWidget with { Value = value };
        }
        throw new InvalidOperationException($"Cannot set Value: widget is not DateTimeInput<{typeof(T).Name}>");
    }

}