using System.Reflection;
using System.Runtime.CompilerServices;
using Ivy.Core;
using Ivy.Core.Helpers;

// ReSharper disable once CheckNamespace
namespace Ivy;

public interface IAnyNumberRangeInput : IAnyInput
{
    public double? Min { get; set; }

    public double? Max { get; set; }

    public double? Step { get; set; }

    public int? Precision { get; set; }

    public NumberFormatStyle FormatStyle { get; set; }

    public string? Currency { get; set; }

    public string? TargetType { get; set; }
}

[Slot("Prefix")]
[Slot("Suffix")]
public abstract record NumberRangeInputBase : WidgetBase<NumberRangeInputBase>, IAnyNumberRangeInput
{
    [Prop] public bool Disabled { get; set; }

    [Prop] public string? Invalid { get; set; }

    [Prop] public string? Placeholder { get; set; }

    [Prop] public bool Nullable { get; set; }

    [Prop] public bool AutoFocus { get; set; }

    [Prop] public double? Min { get; set; }

    [Prop] public double? Max { get; set; }

    [Prop] public double? Step { get; set; }

    [Prop] public int? Precision { get; set; }

    [Prop] public NumberFormatStyle FormatStyle { get; set; } = NumberFormatStyle.Decimal;

    [Prop] public string? Currency { get; set; }

    [Prop] public string? TargetType { get; set; }

    [Prop] public bool NoGrouping { get; init; }

    [Event] public EventHandler<Event<IAnyInput>>? OnBlur { get; set; }
    [Event] public EventHandler<Event<IAnyInput>>? OnFocus { get; set; }

    public Type[] SupportedStateTypes() => [
        typeof((short, short)), typeof((short?, short?)),
        typeof((int, int)), typeof((int?, int?)),
        typeof((long, long)), typeof((long?, long?)),
        typeof((float, float)), typeof((float?, float?)),
        typeof((double, double)), typeof((double?, double?)),
        typeof((decimal, decimal)), typeof((decimal?, decimal?)),
        typeof((byte, byte)), typeof((byte?, byte?))
    ];
}

/// <summary>
/// An input field for selecting a numeric range with two draggable handles.
/// </summary>
public record NumberRangeInput<TNumber> : NumberRangeInputBase, IInput<(TNumber, TNumber)>
{
    [OverloadResolutionPriority(1)]
    public NumberRangeInput(IAnyState state, bool disabled = false, NumberFormatStyle formatStyle = NumberFormatStyle.Decimal)
        : this(disabled, formatStyle)
    {
        var typedState = state.As<(TNumber, TNumber)>();
        Value = typedState.Value;
        OnChange = new(e => { typedState.Set(e.Value); return ValueTask.CompletedTask; });
    }

    [OverloadResolutionPriority(1)]
    public NumberRangeInput((TNumber, TNumber) value, Func<Event<IInput<(TNumber, TNumber)>, (TNumber, TNumber)>, ValueTask> onChange, bool disabled = false, NumberFormatStyle formatStyle = NumberFormatStyle.Decimal)
        : this(disabled, formatStyle)
    {
        OnChange = new(onChange);
        Value = value;
    }

    public NumberRangeInput((TNumber, TNumber) value, Action<(TNumber, TNumber)> state, bool disabled = false, NumberFormatStyle formatStyle = NumberFormatStyle.Decimal)
        : this(disabled, formatStyle)
    {
        OnChange = new(e => { state(e.Value); return ValueTask.CompletedTask; });
        Value = value;
    }

    public NumberRangeInput(bool disabled = false, NumberFormatStyle formatStyle = NumberFormatStyle.Decimal)
    {
        Disabled = disabled;
        FormatStyle = formatStyle;
    }

    internal NumberRangeInput() { }

    [Prop(AlwaysSerialize = true)] public (TNumber, TNumber) Value { get; init; } = default!;

    [Prop] public TNumber LowerValue => Value.Item1;

    [Prop] public TNumber UpperValue => Value.Item2;

    [Prop] public new bool Nullable { get; set; } = typeof(TNumber).IsNullableType();

    [Event] public EventHandler<Event<IInput<(TNumber, TNumber)>, (TNumber, TNumber)>>? OnChange { get; }
}

public static class NumberRangeInputExtensions
{
    public static NumberRangeInputBase ToNumberRangeInput(this IAnyState state, bool disabled = false, NumberFormatStyle formatStyle = NumberFormatStyle.Decimal, double? min = null, double? max = null)
    {
        var type = state.GetStateType();

        if (!type.IsGenericType || type.GetGenericArguments().Length != 2)
        {
            throw new Exception("NumberRangeInput can only be used with a tuple of two numeric values");
        }

        Type genericType = typeof(NumberRangeInput<>).MakeGenericType(type.GetGenericArguments()[0]);
        NumberRangeInputBase input = (NumberRangeInputBase)Activator.CreateInstance(genericType, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new object?[] { state, disabled, formatStyle }, null)!;

        input.ScaffoldDefaults(null, type.GetGenericArguments()[0]);
        if (min is not null) input = input with { Min = min };
        if (max is not null) input = input with { Max = max };
        return input;
    }

    public static NumberRangeInputBase ToMoneyRangeInput(this IAnyState state, bool disabled = false, string currency = "USD")
        => state.ToNumberRangeInput(disabled, NumberFormatStyle.Currency).Currency(currency);

    internal static IAnyNumberRangeInput ScaffoldDefaults(this IAnyNumberRangeInput input, string? name, Type type)
    {
        input.Precision ??= type.SuggestPrecision();
        input.Step ??= type.SuggestStep();
        input.Min ??= type.SuggestMin();
        input.Max ??= type.SuggestMax();

        input.TargetType = GetTargetTypeName(type);

        if (input.FormatStyle == NumberFormatStyle.Currency && string.IsNullOrEmpty(input.Currency))
        {
            input.Currency = "USD";
        }

        return input;
    }

    private static string GetTargetTypeName(Type type)
    {
        var underlyingType = System.Nullable.GetUnderlyingType(type);
        var actualType = underlyingType ?? type;

        return actualType.Name.ToLowerInvariant();
    }

    public static NumberRangeInputBase Placeholder(this NumberRangeInputBase widget, string placeholder)
    {
        return widget with { Placeholder = placeholder };
    }

    public static NumberRangeInputBase Nullable(this NumberRangeInputBase widget, bool? nullable = true)
    {
        return widget with { Nullable = nullable ?? true };
    }

    public static NumberRangeInputBase Disabled(this NumberRangeInputBase widget, bool enabled = true)
    {
        return widget with { Disabled = enabled };
    }

    public static NumberRangeInputBase AutoFocus(this NumberRangeInputBase widget, bool autoFocus = true)
    {
        return widget with { AutoFocus = autoFocus };
    }

    public static NumberRangeInputBase Min(this NumberRangeInputBase widget, double min)
    {
        return widget with { Min = min };
    }

    public static NumberRangeInputBase Max(this NumberRangeInputBase widget, double max)
    {
        return widget with { Max = max };
    }

    public static NumberRangeInputBase Step(this NumberRangeInputBase widget, double step)
    {
        return widget with { Step = step };
    }

    public static NumberRangeInputBase Precision(this NumberRangeInputBase widget, int precision)
    {
        return widget with { Precision = precision };
    }

    public static NumberRangeInputBase FormatStyle(this NumberRangeInputBase widget, NumberFormatStyle formatStyle)
    {
        var result = widget with { FormatStyle = formatStyle };
        if (formatStyle == NumberFormatStyle.Currency && string.IsNullOrEmpty(result.Currency))
        {
            result = result with { Currency = "USD" };
        }
        return result;
    }

    public static NumberRangeInputBase Currency(this NumberRangeInputBase widget, string currency)
    {
        return widget with { Currency = currency };
    }

    public static NumberRangeInputBase NoGrouping(this NumberRangeInputBase widget, bool noGrouping = true)
        => widget with { NoGrouping = noGrouping };

    public static NumberRangeInputBase Invalid(this NumberRangeInputBase widget, string invalid)
    {
        return widget with { Invalid = invalid };
    }

    public static NumberRangeInputBase Prefix(this NumberRangeInputBase widget, object prefix)
        => widget with { Children = widget.WithSlot("Prefix", prefix) };

    public static NumberRangeInputBase Suffix(this NumberRangeInputBase widget, object suffix)
        => widget with { Children = widget.WithSlot("Suffix", suffix) };

    [OverloadResolutionPriority(1)]
    public static NumberRangeInputBase OnBlur(this NumberRangeInputBase widget, Func<Event<IAnyInput>, ValueTask> onBlur)
    {
        return widget with { OnBlur = new(onBlur) };
    }

    public static NumberRangeInputBase OnBlur(this NumberRangeInputBase widget, Action<Event<IAnyInput>> onBlur)
    {
        return widget with { OnBlur = new(onBlur.ToValueTask()) };
    }

    public static NumberRangeInputBase OnBlur(this NumberRangeInputBase widget, Action onBlur)
    {
        return widget with { OnBlur = new(_ => { onBlur(); return ValueTask.CompletedTask; }) };
    }

    [OverloadResolutionPriority(1)]
    public static NumberRangeInputBase OnFocus(this NumberRangeInputBase widget, Func<Event<IAnyInput>, ValueTask> onFocus)
    {
        return widget with { OnFocus = new(onFocus) };
    }

    public static NumberRangeInputBase OnFocus(this NumberRangeInputBase widget, Action<Event<IAnyInput>> onFocus)
    {
        return widget with { OnFocus = new(onFocus.ToValueTask()) };
    }

    public static NumberRangeInputBase OnFocus(this NumberRangeInputBase widget, Action onFocus)
    {
        return widget with { OnFocus = new(_ => { onFocus(); return ValueTask.CompletedTask; }) };
    }
}
