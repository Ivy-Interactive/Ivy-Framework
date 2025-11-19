using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Ivy.Core;
using Ivy.Core.Docs;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;
using Ivy.Shared;
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

    [Prop] public Sizes Size { get; set; }

    [Prop] public bool Disabled { get; set; }

    [Prop] public string? Invalid { get; set; }

    [Event] public Func<Event<IAnyInput>, ValueTask>? OnBlur { get; set; }

    /// <summary>Supports various .NET date and time types with automatic conversion between them.</summary>
    public Type[] SupportedStateTypes() =>
    [
        typeof(DateTime), typeof(DateTime?),
        typeof(DateTimeOffset), typeof(DateTimeOffset?),
        typeof(DateOnly), typeof(DateOnly?),
        typeof(TimeOnly), typeof(TimeOnly?),
    ];
}

public record DateTimeInput<TDate> : DateTimeInputBase, IInput<TDate>
{
    [OverloadResolutionPriority(1)]
    public DateTimeInput(IAnyState state, string? placeholder = null, bool disabled = false, DateTimeInputs variant = DateTimeInputs.Date) : this(placeholder, disabled, variant)
    {
        var typedState = state.As<TDate>();
        Value = typedState.Value;
        OnChange = e => { typedState.Set(e.Value); return ValueTask.CompletedTask; };
    }

    [OverloadResolutionPriority(1)]
    public DateTimeInput(TDate value, Func<Event<IInput<TDate>, TDate>, ValueTask> onChange, string? placeholder = null, bool disabled = false, DateTimeInputs variant = DateTimeInputs.Date) : this(placeholder, disabled, variant)
    {
        OnChange = onChange;
        Value = value;
    }

    public DateTimeInput(TDate value, Action<Event<IInput<TDate>, TDate>> onChange, string? placeholder = null, bool disabled = false, DateTimeInputs variant = DateTimeInputs.Date) : this(placeholder, disabled, variant)
    {
        OnChange = e => { onChange(e); return ValueTask.CompletedTask; };
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

    [Event] public Func<Event<IInput<TDate>, TDate>, ValueTask>? OnChange { get; set; }
}

public static class DateTimeInputExtensions
{
    /// <summary>Convenience method that creates a date input with the Date variant.</summary>
    public static DateTimeInputBase ToDateInput(this IAnyState state, string? placeholder = null, bool disabled = false,
        DateTimeInputs variant = DateTimeInputs.Date)
        => ToDateTimeInput(state, placeholder, disabled, variant);

    /// <summary>Creates a date/time input with type conversion and nullable handling.</summary>
    public static DateTimeInputBase ToDateTimeInput(this IAnyState state, string? placeholder = null, bool disabled = false, DateTimeInputs variant = DateTimeInputs.DateTime)
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

    /// <summary>This is the recommended way to create a time input.</summary>
    public static DateTimeInputBase ToTimeInput(this IAnyState state, string? placeholder = null, bool disabled = false)
        => state.ToDateTimeInput(placeholder, disabled, DateTimeInputs.Time);

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

    public static DateTimeInputBase Variant(this DateTimeInputBase widget, DateTimeInputs variant) => widget with { Variant = variant };

    public static DateTimeInputBase Placeholder(this DateTimeInputBase widget, string placeholder) => widget with { Placeholder = placeholder };

    /// <summary>Format string (e.g., "yyyy-MM-dd", "HH:mm:ss") for displaying and parsing values.</summary>
    public static DateTimeInputBase Format(this DateTimeInputBase widget, string format) => widget with { Format = format };

    public static DateTimeInputBase Invalid(this DateTimeInputBase widget, string? invalid) => widget with { Invalid = invalid };

    [OverloadResolutionPriority(1)]
    public static DateTimeInputBase HandleBlur(this DateTimeInputBase widget, Func<Event<IAnyInput>, ValueTask> onBlur)
    {
        return widget with { OnBlur = onBlur };
    }

    public static DateTimeInputBase HandleBlur(this DateTimeInputBase widget, Action<Event<IAnyInput>> onBlur)
    {
        return widget.HandleBlur(onBlur.ToValueTask());
    }

    public static DateTimeInputBase HandleBlur(this DateTimeInputBase widget, Action onBlur)
    {
        return widget.HandleBlur(_ => { onBlur(); return ValueTask.CompletedTask; });
    }

    public static DateTimeInputBase Size(this DateTimeInputBase widget, Sizes size)
    {
        return widget with { Size = size };
    }

    [RelatedTo(nameof(DateTimeInputBase.Size))]
    public static DateTimeInputBase Large(this DateTimeInputBase widget)
    {
        return widget.Size(Sizes.Large);
    }

    [RelatedTo(nameof(DateTimeInputBase.Size))]
    public static DateTimeInputBase Small(this DateTimeInputBase widget)
    {
        return widget.Size(Sizes.Small);
    }
}