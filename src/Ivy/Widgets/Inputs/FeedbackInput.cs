
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;

// ReSharper disable once CheckNamespace
namespace Ivy;

public enum FeedbackInputVariant
{
    Stars,
    Thumbs,
    Emojis,
}

public interface IAnyFeedbackInput : IAnyInput
{
    public FeedbackInputVariant Variant { get; set; }
}

public abstract record FeedbackInputBase : WidgetBase<FeedbackInputBase>, IAnyFeedbackInput
{
    [Prop] public bool Disabled { get; set; }

    [Prop] public string? Invalid { get; set; }

    [Prop] public string? Placeholder { get; set; }

    [Prop] public bool Nullable { get; set; }

    [Prop] public FeedbackInputVariant Variant { get; set; } = FeedbackInputVariant.Stars;

    [Event] public EventHandler<Event<IAnyInput>>? OnBlur { get; set; }

    public Type[] SupportedStateTypes() => [
        typeof(bool), typeof(bool?),
        typeof(int), typeof(int?),
    ];
}

/// <summary>
/// A specialized input for collecting user feedback or ratings.
/// </summary>
public record FeedbackInput<TNumber> : FeedbackInputBase, IInput<TNumber>
{
    [OverloadResolutionPriority(1)]
    internal FeedbackInput(IAnyState state, string? placeholder = null, bool disabled = false, FeedbackInputVariant variant = FeedbackInputVariant.Stars)
        : this(placeholder, disabled, variant)
    {
        var typedState = state.As<TNumber>();
        Value = typedState.Value;
        OnChange = new(e => { typedState.Set(e.Value); return ValueTask.CompletedTask; });
    }

    [OverloadResolutionPriority(1)]
    internal FeedbackInput(TNumber value, Func<Event<IInput<TNumber>, TNumber>, ValueTask> onChange, string? placeholder = null, bool disabled = false, FeedbackInputVariant variant = FeedbackInputVariant.Stars)
        : this(placeholder, disabled, variant)
    {
        OnChange = onChange.ToEventHandler();
        Value = value;
    }

    internal FeedbackInput(TNumber value, Action<TNumber> state, string? placeholder = null, bool disabled = false, FeedbackInputVariant variant = FeedbackInputVariant.Stars)
        : this(placeholder, disabled, variant)
    {
        OnChange = new(e => { state(e.Value); return ValueTask.CompletedTask; });
        Value = value;
    }

    internal FeedbackInput(string? placeholder = null, bool disabled = false, FeedbackInputVariant variant = FeedbackInputVariant.Stars)
    {
        Placeholder = placeholder;
        Disabled = disabled;
        Variant = variant;
    }

    internal FeedbackInput() { }

    [Prop] public TNumber Value { get; init; } = default!;

    [Prop] public new bool Nullable { get; set; } = typeof(TNumber).IsNullableType();

    [Event] public EventHandler<Event<IInput<TNumber>, TNumber>>? OnChange { get; }
}

public static class FeedbackInputExtensions
{
    public static FeedbackInputBase ToFeedbackInput(this IAnyState state, string? placeholder = null, bool disabled = false, FeedbackInputVariant? variant = null)
    {
        var type = state.GetStateType();

        variant ??= type == typeof(bool) || type == typeof(bool?) ? FeedbackInputVariant.Thumbs : FeedbackInputVariant.Stars;

        Type genericType = typeof(FeedbackInput<>).MakeGenericType(type);
        FeedbackInputBase input = (FeedbackInputBase)Activator.CreateInstance(genericType, state, placeholder, disabled, variant)!;
        return input;
    }

    public static FeedbackInputBase Placeholder(this FeedbackInputBase widget, string placeholder) => widget with { Placeholder = placeholder };

    public static FeedbackInputBase Disabled(this FeedbackInputBase widget, bool enabled = true) => widget with { Disabled = enabled };

    public static FeedbackInputBase Variant(this FeedbackInputBase widget, FeedbackInputVariant variant) => widget with { Variant = variant };

    public static FeedbackInputBase Invalid(this FeedbackInputBase widget, string invalid) => widget with { Invalid = invalid };
    public static FeedbackInputBase Nullable(this FeedbackInputBase widget, bool? nullable = true) => widget with { Nullable = nullable ?? true };

    [OverloadResolutionPriority(1)]
    public static FeedbackInputBase OnBlur(this FeedbackInputBase widget, Func<Event<IAnyInput>, ValueTask> onBlur)
    {
        return widget with { OnBlur = new(onBlur) };
    }

    public static FeedbackInputBase OnBlur(this FeedbackInputBase widget, Action<Event<IAnyInput>> onBlur)
    {
        return widget.OnBlur(onBlur.ToValueTask());
    }

    public static FeedbackInputBase OnBlur(this FeedbackInputBase widget, Action onBlur)
    {
        return widget.OnBlur(_ => { onBlur(); return ValueTask.CompletedTask; });
    }

    public static FeedbackInputBase Stars(this FeedbackInputBase widget) => widget with { Variant = FeedbackInputVariant.Stars };
    public static FeedbackInputBase Thumbs(this FeedbackInputBase widget) => widget with { Variant = FeedbackInputVariant.Thumbs };
    public static FeedbackInputBase Emojis(this FeedbackInputBase widget) => widget with { Variant = FeedbackInputVariant.Emojis };


}