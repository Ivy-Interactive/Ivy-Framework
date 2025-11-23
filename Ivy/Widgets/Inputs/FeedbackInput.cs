
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;
using Ivy.Widgets.Inputs;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

public enum FeedbackInputs
{
    Stars,
    Thumbs,
    Emojis,
}

public interface IAnyFeedbackInput : IAnyInput
{
    public FeedbackInputs Variant { get; set; }
}

public abstract record FeedbackInputBase : WidgetBase<FeedbackInputBase>, IAnyFeedbackInput
{
    [Prop] public bool Disabled { get; set; }

    [Prop] public string? Invalid { get; set; }

    [Prop] public string? Placeholder { get; set; }

    [Prop] public FeedbackInputs Variant { get; set; }

    [Prop] public new Scale? Scale { get; set; }

    [Event] public Func<Event<IAnyInput>, ValueTask>? OnBlur { get; set; }

    public Type[] SupportedStateTypes() => [
        typeof(bool), typeof(bool?),
        typeof(int), typeof(int?),
    ];
}

public record FeedbackInput<TNumber> : FeedbackInputBase, IInput<TNumber>
{
    [OverloadResolutionPriority(1)]
    public FeedbackInput(IAnyState state, string? placeholder = null, bool disabled = false, FeedbackInputs variant = FeedbackInputs.Stars)
        : this(placeholder, disabled, variant)
    {
        var typedState = state.As<TNumber>();
        Value = typedState.Value;
        OnChange = e => { typedState.Set(e.Value); return ValueTask.CompletedTask; };
    }

    [OverloadResolutionPriority(1)]
    public FeedbackInput(TNumber value, Func<Event<IInput<TNumber>, TNumber>, ValueTask> onChange, string? placeholder = null, bool disabled = false, FeedbackInputs variant = FeedbackInputs.Stars)
        : this(placeholder, disabled, variant)
    {
        OnChange = onChange;
        Value = value;
    }

    public FeedbackInput(TNumber value, Action<TNumber> state, string? placeholder = null, bool disabled = false, FeedbackInputs variant = FeedbackInputs.Stars)
        : this(placeholder, disabled, variant)
    {
        OnChange = e => { state(e.Value); return ValueTask.CompletedTask; };
        Value = value;
    }

    public FeedbackInput(string? placeholder = null, bool disabled = false, FeedbackInputs variant = FeedbackInputs.Stars)
    {
        Placeholder = placeholder;
        Disabled = disabled;
        Variant = variant;
    }

    [Prop] public TNumber Value { get; } = default!;

    [Prop] public bool Nullable { get; set; } = typeof(TNumber).IsNullableType();

    [Event] public Func<Event<IInput<TNumber>, TNumber>, ValueTask>? OnChange { get; }
}

public static class FeedbackInputExtensions
{
    public static FeedbackInputBase ToFeedbackInput(this IAnyState state, string? placeholder = null, bool disabled = false, FeedbackInputs? variant = null)
    {
        var type = state.GetStateType();

        variant ??= type == typeof(bool) || type == typeof(bool?) ? FeedbackInputs.Thumbs : FeedbackInputs.Stars;

        Type genericType = typeof(FeedbackInput<>).MakeGenericType(type);
        FeedbackInputBase input = (FeedbackInputBase)Activator.CreateInstance(genericType, state, placeholder, disabled, variant)!;
        return input;
    }

    public static FeedbackInputBase Placeholder(this FeedbackInputBase widget, string placeholder) => widget with { Placeholder = placeholder };

    public static FeedbackInputBase Disabled(this FeedbackInputBase widget, bool enabled = true) => widget with { Disabled = enabled };

    public static FeedbackInputBase Variant(this FeedbackInputBase widget, FeedbackInputs variant) => widget with { Variant = variant };

    public static FeedbackInputBase Invalid(this FeedbackInputBase widget, string invalid) => widget with { Invalid = invalid };

    [OverloadResolutionPriority(1)]
    public static FeedbackInputBase HandleBlur(this FeedbackInputBase widget, Func<Event<IAnyInput>, ValueTask> onBlur)
    {
        return widget with { OnBlur = onBlur };
    }

    public static FeedbackInputBase HandleBlur(this FeedbackInputBase widget, Action<Event<IAnyInput>> onBlur)
    {
        return widget.HandleBlur(onBlur.ToValueTask());
    }

    public static FeedbackInputBase HandleBlur(this FeedbackInputBase widget, Action onBlur)
    {
        return widget.HandleBlur(_ => { onBlur(); return ValueTask.CompletedTask; });
    }
}