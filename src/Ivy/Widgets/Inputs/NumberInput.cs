using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Ivy.Core;
using Ivy.Core.Docs;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;

// ReSharper disable once CheckNamespace
namespace Ivy;

public enum NumberInputVariant
{
    Number,
    Slider
}

public enum NumberFormatStyle
{
    Decimal,
    Currency,
    Percent
}

public interface IAnyNumberInput : IAnyInput
{
    public double? Min { get; set; }

    public double? Max { get; set; }

    public double? Step { get; set; }

    public int? Precision { get; set; }

    public NumberInputVariant Variant { get; set; }

    public NumberFormatStyle FormatStyle { get; set; }

    public string? Currency { get; set; }

    public string? TargetType { get; set; }
}

public abstract record NumberInputBase : WidgetBase<NumberInputBase>, IAnyNumberInput
{
    [Prop] public bool Disabled { get; set; }

    [Prop] public string? Invalid { get; set; }

    [Prop] public string? Placeholder { get; set; }

    [Prop] public bool Nullable { get; set; }

    [Prop] public double? Min { get; set; }

    [Prop] public double? Max { get; set; }

    [Prop] public double? Step { get; set; }

    [Prop] public int? Precision { get; set; }

    [Prop] public NumberInputVariant Variant { get; set; } = NumberInputVariant.Number;

    [Prop] public NumberFormatStyle FormatStyle { get; set; } = NumberFormatStyle.Decimal;

    [Prop] public string? Currency { get; set; }

    [Prop] public string? TargetType { get; set; }

    [Prop] public Affix? Prefix { get; set; }

    [Prop] public bool NoGrouping { get; init; }

    [Prop] public Affix? Suffix { get; set; }

    [Event] public EventHandler<Event<IAnyInput>>? OnBlur { get; set; }

    public Type[] SupportedStateTypes() => [
    typeof(short), typeof(short?),
        typeof(int), typeof(int?),
        typeof(long), typeof(long?),
        typeof(float), typeof(float?),
        typeof(double), typeof(double?),
        typeof(decimal), typeof(decimal?),
        typeof(byte), typeof(byte?)
];
}

/// <summary>
/// An input field restricted to numerical values.
/// </summary>
public record NumberInput<TNumber> : NumberInputBase, IInput<TNumber>, IAnyNumberInput
{
    [OverloadResolutionPriority(1)]
    internal NumberInput(IAnyState state, string? placeholder = null, bool disabled = false, NumberInputVariant variant = NumberInputVariant.Number, NumberFormatStyle formatStyle = NumberFormatStyle.Decimal)
        : this(placeholder, disabled, variant, formatStyle)
    {
        var typedState = state.As<TNumber>();
        Value = typedState.Value;
        OnChange = new(e => { typedState.Set(e.Value); return ValueTask.CompletedTask; });
    }

    [OverloadResolutionPriority(1)]
    internal NumberInput(TNumber value, Func<Event<IInput<TNumber>, TNumber>, ValueTask> onChange, string? placeholder = null, bool disabled = false, NumberInputVariant variant = NumberInputVariant.Number, NumberFormatStyle formatStyle = NumberFormatStyle.Decimal)
        : this(placeholder, disabled, variant, formatStyle)
    {
        OnChange = new(onChange);
        Value = value;
    }

    internal NumberInput(TNumber value, Action<TNumber> state, string? placeholder = null, bool disabled = false, NumberInputVariant variant = NumberInputVariant.Number, NumberFormatStyle formatStyle = NumberFormatStyle.Decimal)
        : this(placeholder, disabled, variant, formatStyle)
    {
        OnChange = new(e => { state(e.Value); return ValueTask.CompletedTask; });
        Value = value;
    }

    internal NumberInput(string? placeholder = null, bool disabled = false, NumberInputVariant variant = NumberInputVariant.Number, NumberFormatStyle formatStyle = NumberFormatStyle.Decimal)
    {
        Placeholder = placeholder;
        Disabled = disabled;
        Variant = variant;
        FormatStyle = formatStyle;
    }

    internal NumberInput() { }

    [Prop(AlwaysSerialize = true)] public TNumber Value { get; init; } = default!;

    [Prop(AlwaysSerialize = true)] public new bool Nullable { get; set; } = typeof(TNumber).IsNullableType();

    [Event] public EventHandler<Event<IInput<TNumber>, TNumber>>? OnChange { get; }
}

public static class NumberInputExtensions
{
    public static NumberInputBase ToSliderInput(this IAnyState state, string? placeholder = null, bool disabled = false, NumberFormatStyle formatStyle = NumberFormatStyle.Decimal)
    {
        return state.ToNumberInput(placeholder, disabled, NumberInputVariant.Slider, formatStyle);
    }

    public static NumberInputBase ToNumberInput(this IAnyState state, string? placeholder = null, bool disabled = false, NumberInputVariant variant = NumberInputVariant.Number, NumberFormatStyle formatStyle = NumberFormatStyle.Decimal, double? min = null, double? max = null)
    {
        var type = state.GetStateType();
        Type genericType = typeof(NumberInput<>).MakeGenericType(type);
        NumberInputBase input = (NumberInputBase)Activator.CreateInstance(genericType, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new object?[] { state, placeholder, disabled, variant, formatStyle }, null)!;

        input.ScaffoldDefaults(null, type);
        if (min is not null) input = input with { Min = min };
        if (max is not null) input = input with { Max = max };
        return input;
    }

    public static NumberInputBase ToMoneyInput(this IAnyState state, string? placeholder = null, bool disabled = false, NumberInputVariant variant = NumberInputVariant.Number, string currency = "USD")
    => state.ToNumberInput(placeholder, disabled, variant, NumberFormatStyle.Currency).Currency(currency);

    internal static IAnyNumberInput ScaffoldDefaults(this IAnyNumberInput input, string? name, Type type)
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

    public static NumberInputBase Placeholder(this NumberInputBase widget, string placeholder)
    {
        return widget with { Placeholder = placeholder };
    }
    public static NumberInputBase Nullable(this NumberInputBase widget, bool? nullable = true)
    {
        return widget with { Nullable = nullable ?? true };
    }

    public static NumberInputBase Disabled(this NumberInputBase widget, bool enabled = true)
    {
        return widget with { Disabled = enabled };
    }

    public static NumberInputBase Min(this NumberInputBase widget, double min)
    {
        return widget with { Min = min };
    }

    public static NumberInputBase Max(this NumberInputBase widget, double max)
    {
        return widget with { Max = max };
    }

    public static NumberInputBase Step(this NumberInputBase widget, double step)
    {
        return widget with { Step = step };
    }

    public static NumberInputBase Variant(this NumberInputBase widget, NumberInputVariant variant)
    {
        return widget with { Variant = variant };
    }

    public static NumberInputBase Precision(this NumberInputBase widget, int precision)
    {
        return widget with { Precision = precision };
    }

    public static NumberInputBase FormatStyle(this NumberInputBase widget, NumberFormatStyle formatStyle)
    {
        var result = widget with { FormatStyle = formatStyle };
        if (formatStyle == NumberFormatStyle.Currency && string.IsNullOrEmpty(result.Currency))
        {
            result = result with { Currency = "USD" };
        }
        return result;
    }

    public static NumberInputBase Currency(this NumberInputBase widget, string currency)
    {
        return widget with { Currency = currency };
    }

    public static NumberInputBase NoGrouping(this NumberInputBase widget, bool noGrouping = true)
        => widget with { NoGrouping = noGrouping };

    public static NumberInputBase Invalid(this NumberInputBase widget, string invalid)
    {
        return widget with { Invalid = invalid };
    }

    public static NumberInputBase Prefix(this NumberInputBase widget, string prefixText)
        => widget with { Prefix = prefixText.ToAffix() };

    public static NumberInputBase Prefix(this NumberInputBase widget, Icons prefixIcon)
        => widget with { Prefix = prefixIcon.ToAffix() };

    public static NumberInputBase Suffix(this NumberInputBase widget, string suffixText)
        => widget with { Suffix = suffixText.ToAffix() };

    public static NumberInputBase Suffix(this NumberInputBase widget, Icons suffixIcon)
        => widget with { Suffix = suffixIcon.ToAffix() };

    [OverloadResolutionPriority(1)]
    public static NumberInputBase OnBlur(this NumberInputBase widget, Func<Event<IAnyInput>, ValueTask> onBlur)
    {
        return widget with { OnBlur = new(onBlur) };
    }

    public static NumberInputBase OnBlur(this NumberInputBase widget, Action<Event<IAnyInput>> onBlur)
    {
        return widget with { OnBlur = new(onBlur.ToValueTask()) };
    }

    public static NumberInputBase OnBlur(this NumberInputBase widget, Action onBlur)
    {
        return widget with { OnBlur = new(_ => { onBlur(); return ValueTask.CompletedTask; }) };
    }


}