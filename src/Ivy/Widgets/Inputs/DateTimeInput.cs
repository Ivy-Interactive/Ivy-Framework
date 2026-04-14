using System.Reflection;
using System.Runtime.CompilerServices;
using Ivy.Core;
using Ivy.Core.Helpers;

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

    [Prop] public bool AutoFocus { get; set; }

    [Prop] public string? Invalid { get; set; }

    [Prop] public DayOfWeek? FirstDayOfWeek { get; set; }

    [Prop] public DateTime? Min { get; set; }
    [Prop] public DateTime? Max { get; set; }
    [Prop] public TimeSpan? Step { get; set; }

    [Event] public EventHandler<Event<IAnyInput>>? OnBlur { get; set; }
    [Event] public EventHandler<Event<IAnyInput>>? OnFocus { get; set; }

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
    internal DateTimeInput(IAnyState state, string? placeholder = null, bool disabled = false, DateTimeInputVariant variant = DateTimeInputVariant.Date) : this(placeholder, disabled, variant)
    {
        var typedState = state.As<TDate>();
        Value = typedState.Value;
        OnChange = new(e => { typedState.Set(Clamp(e.Value)); return ValueTask.CompletedTask; });
    }

    [OverloadResolutionPriority(1)]
    internal DateTimeInput(TDate value, Func<Event<IInput<TDate>, TDate>, ValueTask> onChange, string? placeholder = null, bool disabled = false, DateTimeInputVariant variant = DateTimeInputVariant.Date) : this(placeholder, disabled, variant)
    {
        OnChange = new(e => onChange(new Event<IInput<TDate>, TDate>(e.EventName, e.Sender, Clamp(e.Value))));
        Value = value;
    }

    internal DateTimeInput(TDate value, Action<Event<IInput<TDate>, TDate>> onChange, string? placeholder = null, bool disabled = false, DateTimeInputVariant variant = DateTimeInputVariant.Date) : this(placeholder, disabled, variant)
    {
        OnChange = new(e => { onChange(new Event<IInput<TDate>, TDate>(e.EventName, e.Sender, Clamp(e.Value))); return ValueTask.CompletedTask; });
        Value = value;
    }

    internal DateTimeInput(string? placeholder = null, bool disabled = false, DateTimeInputVariant variant = DateTimeInputVariant.Date)
    {
        Variant = variant;
        Placeholder = placeholder;
        Disabled = disabled;
    }

    internal DateTimeInput() { }

    [Prop(AlwaysSerialize = true)] public TDate Value { get; init; } = default!;

    [Prop] public new bool Nullable { get; set; } = typeof(TDate) == typeof(DateTime?) || typeof(TDate) == typeof(DateTimeOffset?) || typeof(TDate) == typeof(DateOnly?) || typeof(TDate) == typeof(TimeOnly?);

    [Event] public EventHandler<Event<IInput<TDate>, TDate>>? OnChange { get; set; }

    private TDate Clamp(TDate value)
    {
        if (Min == null && Max == null) return value;

        try
        {
            DateTime dtValue = value switch
            {
                DateTime dt => dt,
                DateTimeOffset dto => dto.DateTime,
                DateOnly d => d.ToDateTime(TimeOnly.MinValue),
                TimeOnly t => DateTime.Today.Add(t.ToTimeSpan()),
                _ => (DateTime)(Core.Utils.BestGuessConvert(value, typeof(DateTime)) ?? DateTime.Now)
            };

            if (Min.HasValue && dtValue < Min.Value) return (TDate)Core.Utils.BestGuessConvert(Min.Value, typeof(TDate))!;
            if (Max.HasValue && dtValue > Max.Value) return (TDate)Core.Utils.BestGuessConvert(Max.Value, typeof(TDate))!;
        }
        catch
        {
            // If conversion fails, just return the value as is
        }

        return value;
    }
}

public static class DateTimeInputExtensions
{
    public static DateTimeInputBase ToDateTimeInput(this IAnyState state, string? placeholder = null, bool disabled = false, DateTimeInputVariant variant = DateTimeInputVariant.DateTime)
    {
        var stateType = state.GetStateType();
        var isNullable = stateType.IsNullableType();

        Type genericType = typeof(DateTimeInput<>).MakeGenericType(stateType);

        DateTimeInputBase input = (DateTimeInputBase)Activator.CreateInstance(genericType, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new object?[] { state, placeholder, disabled, variant }, null)!;
        var nullableProperty = genericType.GetProperty("Nullable", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        nullableProperty?.SetValue(input, isNullable);

        input.ScaffoldDefaults(null!, stateType);
        return input;
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
            input.Placeholder = StringHelper.LabelFor(name, type);
        }

        return input;
    }

    public static T Disabled<T>(this T widget, bool disabled = true) where T : DateTimeInputBase => widget with { Disabled = disabled };

    public static T Variant<T>(this T widget, DateTimeInputVariant variant) where T : DateTimeInputBase => widget with { Variant = variant };

    public static T Placeholder<T>(this T widget, string placeholder) where T : DateTimeInputBase => widget with { Placeholder = placeholder };

    public static T Format<T>(this T widget, string format) where T : DateTimeInputBase => widget with { Format = format };

    public static T Invalid<T>(this T widget, string? invalid) where T : DateTimeInputBase => widget with { Invalid = invalid };
    public static T FirstDayOfWeek<T>(this T widget, DayOfWeek day) where T : DateTimeInputBase => widget with { FirstDayOfWeek = day };
    public static T Nullable<T>(this T widget, bool? nullable = true) where T : DateTimeInputBase => widget with { Nullable = nullable ?? true };

    public static T Min<T>(this T widget, DateTime min) where T : DateTimeInputBase => widget with { Min = min };
    public static T Max<T>(this T widget, DateTime max) where T : DateTimeInputBase => widget with { Max = max };
    public static T Step<T>(this T widget, TimeSpan step) where T : DateTimeInputBase => widget with { Step = step };

    [OverloadResolutionPriority(1)]
    public static T OnBlur<T>(this T widget, Func<Event<IAnyInput>, ValueTask> onBlur) where T : DateTimeInputBase
    {
        return widget with { OnBlur = new(onBlur) };
    }

    public static T OnBlur<T>(this T widget, Action<Event<IAnyInput>> onBlur) where T : DateTimeInputBase
    {
        return widget.OnBlur(onBlur.ToValueTask());
    }

    public static T OnBlur<T>(this T widget, Action onBlur) where T : DateTimeInputBase
    {
        return widget.OnBlur(_ => { onBlur(); return ValueTask.CompletedTask; });
    }

    [OverloadResolutionPriority(1)]
    public static DateTimeInputBase OnFocus(this DateTimeInputBase widget, Func<Event<IAnyInput>, ValueTask> onFocus)
    {
        return widget with { OnFocus = new(onFocus) };
    }

    public static DateTimeInputBase OnFocus(this DateTimeInputBase widget, Action<Event<IAnyInput>> onFocus)
    {
        return widget.OnFocus(onFocus.ToValueTask());
    }

    public static DateTimeInputBase OnFocus(this DateTimeInputBase widget, Action onFocus)
    {
        return widget.OnFocus(_ => { onFocus(); return ValueTask.CompletedTask; });
    }

    private static object[] WithSlot(DateTimeInputBase widget, string slotName, object? value)
    {
        var others = widget.Children.Where(c => c is not Slot s || s.Name != slotName);
        var result = value != null ? others.Append(new Slot(slotName, value)) : others;
        return result.ToArray();
    }

    public static DateTimeInputBase Prefix(this DateTimeInputBase widget, object prefix)
        => widget with { Children = WithSlot(widget, "Prefix", prefix) };

    public static DateTimeInputBase Suffix(this DateTimeInputBase widget, object suffix)
        => widget with { Children = WithSlot(widget, "Suffix", suffix) };
}