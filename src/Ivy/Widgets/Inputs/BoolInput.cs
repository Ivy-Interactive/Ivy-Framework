using System.Reflection;
using System.Runtime.CompilerServices;
using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;

// ReSharper disable once CheckNamespace
namespace Ivy;

public enum BoolInputVariant
{
    Checkbox,
    Switch,
    Toggle
}

public interface IAnyBoolInput : IAnyInput
{
    public string? Label { get; set; }

    public string? Description { get; set; }

    public BoolInputVariant Variant { get; set; }

    public Icons Icon { get; set; }

    public bool Loading { get; set; }
}

public abstract record BoolInputBase : WidgetBase<BoolInputBase>, IAnyBoolInput
{
    [Prop] public bool Disabled { get; set; }

    [Prop] public string? Invalid { get; set; }

    [Prop] public string? Label { get; set; }

    [Prop] public string? Description { get; set; }

    [Prop] public BoolInputVariant Variant { get; set; } = BoolInputVariant.Checkbox;

    [Prop] public Icons Icon { get; set; }

    [Prop] public bool Loading { get; set; }

    [Prop] public string? Placeholder { get; set; } //not really used but included to consistency with IAnyInput
    [Prop] public bool Nullable { get; set; }

    [Event] public EventHandler<Event<IAnyInput>>? OnBlur { get; set; }

    public Type[] SupportedStateTypes() =>
    [
        // Boolean types
        typeof(bool), typeof(bool?),
        // Signed integer types
        typeof(short), typeof(short?),
        typeof(int), typeof(int?),
        typeof(long), typeof(long?),
        // Unsigned integer types
        typeof(byte), typeof(byte?),
        // Floating-point types
        typeof(float), typeof(float?),
        typeof(double), typeof(double?),
        typeof(decimal), typeof(decimal?)
    ];
}

public record BoolInput<TBool> : BoolInputBase, IInput<TBool>
{
    [OverloadResolutionPriority(1)]
    internal BoolInput(IAnyState state, string? label = null, bool disabled = false,
        BoolInputVariant variant = BoolInputVariant.Checkbox)
        : this(label, disabled, variant)
    {
        var typedState = state.As<TBool>();
        Value = typedState.Value;
        OnChange = new(e => { typedState.Set(e.Value); return ValueTask.CompletedTask; });
    }

    [OverloadResolutionPriority(1)]
    internal BoolInput(TBool value, Func<Event<IInput<TBool>, TBool>, ValueTask> onChange, string? label = null,
        bool disabled = false, BoolInputVariant variant = BoolInputVariant.Checkbox) : this(label, disabled, variant)
    {
        OnChange = new(onChange);
        Value = value;
    }

    internal BoolInput(TBool value, Action<Event<IInput<TBool>, TBool>> onChange, string? label = null,
        bool disabled = false, BoolInputVariant variant = BoolInputVariant.Checkbox) : this(label, disabled, variant)
    {
        OnChange = new(onChange.ToValueTask());
        Value = value;
    }

    internal BoolInput(string? label = null, bool disabled = false, BoolInputVariant variant = BoolInputVariant.Checkbox)
    {
        Label = label;
        Disabled = disabled;
        Variant = variant;
    }

    internal BoolInput() { }

    [Prop(AlwaysSerialize = true)] public TBool Value { get; init; } = default!;

    [Prop] public new bool Nullable { get; set; } = typeof(TBool) == typeof(bool?);

    [Event] public EventHandler<Event<IInput<TBool>, TBool>>? OnChange { get; }
}

/// <summary>
/// A checkbox or switch for toggling boolean values.
/// </summary>
public record BoolInput : BoolInput<bool>
{
    [OverloadResolutionPriority(1)]
    internal BoolInput(IAnyState state, string? label = null, bool disabled = false,
        BoolInputVariant variant = BoolInputVariant.Checkbox)
        : base(state, label, disabled, variant)
    {
    }

    [OverloadResolutionPriority(1)]
    internal BoolInput(bool value, Func<Event<IInput<bool>, bool>, ValueTask> onChange, string? label = null,
        bool disabled = false, BoolInputVariant variant = BoolInputVariant.Checkbox)
        : base(value, onChange, label, disabled, variant)
    {
    }

    internal BoolInput(bool value, Action<Event<IInput<bool>, bool>> onChange, string? label = null,
        bool disabled = false, BoolInputVariant variant = BoolInputVariant.Checkbox)
        : base(value, onChange, label, disabled, variant)
    {
    }

    internal BoolInput(string? label = null, bool disabled = false, BoolInputVariant variant = BoolInputVariant.Checkbox)
        : base(label, disabled, variant)
    {
    }
}

public static class BoolInputExtensions
{
    public static BoolInputBase ToBoolInput(this IAnyState state, string? label = null, bool disabled = false,
    BoolInputVariant variant = BoolInputVariant.Checkbox)
    {
        var stateType = state.GetStateType();
        var isNullable = stateType.IsNullableType();

        Type genericType = typeof(BoolInput<>).MakeGenericType(stateType);

        BoolInputBase input = (BoolInputBase)Activator.CreateInstance(genericType, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new object?[] { state, label, disabled, variant }, null)!;
        var nullableProperty = genericType.GetProperty("Nullable", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        nullableProperty?.SetValue(input, isNullable);

        input.ScaffoldDefaults(null!, stateType);
        return input;
    }

    private static T ConvertToBoolValue<T>(IAnyState state)
    {
        var stateType = state.GetStateType();
        var value = state.As<object>().Value;

        // Convert to boolean based on type
        var boolValue = stateType switch
        {
            // Boolean types - direct conversion
            _ when stateType == typeof(bool) => (bool)value,
            _ when stateType == typeof(bool?) => (bool?)value,

            // Numeric types - convert to boolean (0 = false, non-zero = true)
            // Expression value==null should always be null (suggestion by IntelliJ), but in this case it is a valid check.
            // ReSharper disables once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            _ when stateType.IsNumeric() && stateType.IsNullableType() => value == null ? null : Convert.ToBoolean(value),
            _ when stateType.IsNumeric() => Convert.ToBoolean(value),

            // Other types - try BestGuessConvert, fallback to false
            _ => Core.Utils.BestGuessConvert(value, typeof(bool)) is true
        };

        // Handle the return type T appropriately
        if (typeof(T) == typeof(bool?))
        {
            return (T)(object)boolValue!;
        }

        // For non-nullable bool, convert null to false
        var nonNullableBool = boolValue ?? false;
        return (T)(object)nonNullableBool;
    }

    private static void SetStateValue(IAnyState state, bool boolValue)
    {
        SetStateValue(state, (bool?)boolValue);
    }

    private static void SetStateValue(IAnyState state, bool? boolValue)
    {
        var stateType = state.GetStateType();
        var isNullable = stateType.IsNullableType();

        // Convert boolean back to the original state type
        var convertedValue = stateType switch
        {
            // Boolean types - direct conversion
            _ when stateType == typeof(bool) => boolValue ?? false,
            _ when stateType == typeof(bool?) => boolValue,

            // Numeric types - convert boolean to numeric
            _ when stateType.IsNumeric() => ConvertBoolToNumeric(boolValue, stateType, isNullable),

            // Other types - use BestGuessConvert
            _ => Core.Utils.BestGuessConvert(boolValue ?? false, stateType) ?? false
        };

        // Set the state value
        state.As<object>().Set(convertedValue!);
    }

    private static object ConvertBoolToNumeric(bool? boolValue, Type targetType, bool isNullable)
    {
        if (isNullable)
        {
            var underlyingType = System.Nullable.GetUnderlyingType(targetType);
            if (boolValue == null)
            {
                return null!;
            }

            var numericValue = boolValue == true ? 1 : 0;
            return Convert.ChangeType(numericValue, underlyingType!);
        }
        else
        {
            var numericValue = boolValue == true ? 1 : 0;
            return Convert.ChangeType(numericValue, targetType);
        }
    }

    public static BoolInputBase ToSwitchInput(this IAnyState state, Icons? icon = null, string? label = null,
        bool disabled = false)
    {
        var input = state.ToBoolInput(label, disabled, BoolInputVariant.Switch);
        if (icon != null)
        {
            input.Icon = icon.Value;
        }

        return input;
    }

    public static BoolInputBase ToToggleInput(this IAnyState state, Icons? icon = null, string? label = null,
        bool disabled = false)
    {
        var input = state.ToBoolInput(label, disabled, BoolInputVariant.Toggle);
        if (icon != null)
        {
            input.Icon = icon.Value;
        }

        return input;
    }

    internal static IAnyBoolInput ScaffoldDefaults(this IAnyBoolInput input, string? name, Type type)
    {
        if (string.IsNullOrEmpty(input.Label))
        {
            input.Label = StringHelper.SplitPascalCase(name) ?? name;
        }

        return input;
    }

    public static BoolInputBase Label(this BoolInputBase widget, string label) => widget with { Label = label };

    public static BoolInputBase Disabled(this BoolInputBase widget, bool disabled = true) =>
        widget with { Disabled = disabled };

    public static BoolInputBase Variant(this BoolInputBase widget, BoolInputVariant variant) =>
        widget with { Variant = variant };

    public static BoolInputBase Icon(this BoolInputBase widget, Icons icon) => widget with { Icon = icon };

    public static BoolInputBase Description(this BoolInputBase widget, string description) =>
        widget with { Description = description };

    public static BoolInputBase Invalid(this BoolInputBase widget, string? invalid) =>
        widget with { Invalid = invalid };

    public static BoolInputBase Loading(this BoolInputBase widget, bool loading = true) =>
        widget with { Loading = loading };

    public static BoolInputBase Nullable(this BoolInputBase widget, bool? nullable = true) =>
        widget with { Nullable = nullable ?? true };

    [OverloadResolutionPriority(1)]
    public static BoolInputBase OnBlur(this BoolInputBase widget, Func<Event<IAnyInput>, ValueTask> onBlur)
    {
        return widget with { OnBlur = new(onBlur) };
    }

    public static BoolInputBase OnBlur(this BoolInputBase widget, Action<Event<IAnyInput>> onBlur)
    {
        return widget with { OnBlur = new(onBlur.ToValueTask()) };
    }

    public static BoolInputBase OnBlur(this BoolInputBase widget, Action onBlur)
    {
        return widget with { OnBlur = new(_ => { onBlur(); return ValueTask.CompletedTask; }) };
    }


}