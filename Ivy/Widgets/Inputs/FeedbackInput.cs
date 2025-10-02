
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;
using Ivy.Widgets.Inputs;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Defines the visual variants available for feedback input controls.
/// </summary>
public enum FeedbackInputs
{
    /// <summary>Star rating system for numerical feedback (typically 1-5 stars).</summary>
    Stars,
    /// <summary>Thumbs up/down system for binary positive/negative feedback.</summary>
    Thumbs,
    /// <summary>Emoji-based feedback system for emotional or satisfaction ratings.</summary>
    Emojis,
}

/// <summary>
/// Interface for feedback input controls.
/// </summary>
public interface IAnyFeedbackInput : IAnyInput
{
    /// <summary>Gets or sets the placeholder text displayed when the feedback input is empty.</summary>
    public string? Placeholder { get; set; }

    /// <summary>Gets or sets the visual variant of the feedback input.</summary>
    public FeedbackInputs Variant { get; set; }
}

/// <summary>
/// Abstract base class for feedback input controls.
/// </summary>
public abstract record FeedbackInputBase : WidgetBase<FeedbackInputBase>, IAnyFeedbackInput
{
    /// <summary>Gets or sets whether the input is disabled.</summary>
    [Prop] public bool Disabled { get; set; }

    /// <summary>Gets or sets the validation error message.</summary>
    [Prop] public string? Invalid { get; set; }

    /// <summary>Gets or sets the placeholder text displayed when the input is empty.</summary>
    [Prop] public string? Placeholder { get; set; }

    /// <summary>Gets or sets the visual variant of the feedback input.</summary>
    [Prop] public FeedbackInputs Variant { get; set; }

    /// <summary>Gets or sets the size of the feedback input.</summary>
    [Prop] public Sizes Size { get; set; }

    /// <summary>Gets or sets the event handler called when the input loses focus.</summary>
    [Event] public Func<Event<IAnyInput>, ValueTask>? OnBlur { get; set; }

    /// <summary>
    /// Returns the types that this feedback input can bind to and work with.
    /// </summary>
    public Type[] SupportedStateTypes() => [
        typeof(bool), typeof(bool?),
        typeof(int), typeof(int?),
    ];
}

/// <summary>
/// Generic feedback input control.
/// </summary>
/// <typeparam name="TNumber">The type of the feedback value.</typeparam>
public record FeedbackInput<TNumber> : FeedbackInputBase, IInput<TNumber>
{
    /// <summary>
    /// Initializes a new instance bound to a state object.
    /// </summary>
    /// <param name="state">The state object to bind to.</param>
    /// <param name="placeholder">Optional placeholder text displayed when the input is empty.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <param name="variant">The visual variant of the feedback input.</param>
    [OverloadResolutionPriority(1)]
    public FeedbackInput(IAnyState state, string? placeholder = null, bool disabled = false, FeedbackInputs variant = FeedbackInputs.Stars)
        : this(placeholder, disabled, variant)
    {
        var typedState = state.As<TNumber>();
        Value = typedState.Value;
        OnChange = e => { typedState.Set(e.Value); return ValueTask.CompletedTask; };
    }

    /// <summary>
    /// Initializes a new instance with an explicit value and async change handler.
    /// </summary>
    /// <param name="value">The initial feedback value.</param>
    /// <param name="onChange">Async function to handle feedback value changes.</param>
    /// <param name="placeholder">Optional placeholder text displayed when the input is empty.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <param name="variant">The visual variant of the feedback input.</param>
    [OverloadResolutionPriority(1)]
    public FeedbackInput(TNumber value, Func<Event<IInput<TNumber>, TNumber>, ValueTask> onChange, string? placeholder = null, bool disabled = false, FeedbackInputs variant = FeedbackInputs.Stars)
        : this(placeholder, disabled, variant)
    {
        OnChange = onChange;
        Value = value;
    }

    /// <summary>
    /// Initializes a new instance with an explicit value.
    /// </summary>
    /// <param name="value">The initial feedback value.</param>
    /// <param name="state">Function to update the state when the feedback value changes.</param>
    /// <param name="placeholder">Optional placeholder text displayed when the input is empty.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <param name="variant">The visual variant of the feedback input.</param>
    public FeedbackInput(TNumber value, Action<TNumber> state, string? placeholder = null, bool disabled = false, FeedbackInputs variant = FeedbackInputs.Stars)
        : this(placeholder, disabled, variant)
    {
        OnChange = e => { state(e.Value); return ValueTask.CompletedTask; };
        Value = value;
    }

    /// <summary>
    /// Initializes a new instance with basic configuration.
    /// </summary>
    /// <param name="placeholder">Optional placeholder text displayed when the input is empty.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <param name="variant">The visual variant of the feedback input.</param>
    public FeedbackInput(string? placeholder = null, bool disabled = false, FeedbackInputs variant = FeedbackInputs.Stars)
    {
        Placeholder = placeholder;
        Disabled = disabled;
        Variant = variant;
    }

    /// <summary>Gets the current feedback value.</summary>
    [Prop] public TNumber Value { get; } = default!;

    /// <summary>Gets or sets whether the input accepts null values.</summary>
    [Prop] public bool Nullable { get; set; } = typeof(TNumber).IsNullableType();

    /// <summary>Gets the event handler called when the feedback value changes.</summary>
    [Event] public Func<Event<IInput<TNumber>, TNumber>, ValueTask>? OnChange { get; }
}

/// <summary>
/// Provides extension methods for creating and configuring feedback inputs with fluent syntax.
/// </summary>
public static class FeedbackInputExtensions
{
    /// <summary>
    /// Creates a feedback input from a state object.
    /// </summary>
    /// <param name="state">The state object to bind to.</param>
    /// <param name="placeholder">Optional placeholder text displayed when the input is empty.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <param name="variant">Optional explicit variant selection.</param>
    public static FeedbackInputBase ToFeedbackInput(this IAnyState state, string? placeholder = null, bool disabled = false, FeedbackInputs? variant = null)
    {
        var type = state.GetStateType();

        variant ??= type == typeof(bool) || type == typeof(bool?) ? FeedbackInputs.Thumbs : FeedbackInputs.Stars;

        Type genericType = typeof(FeedbackInput<>).MakeGenericType(type);
        FeedbackInputBase input = (FeedbackInputBase)Activator.CreateInstance(genericType, state, placeholder, disabled, variant)!;
        return input;
    }

    /// <summary>Sets the placeholder text for the feedback input.</summary>
    /// <param name="widget">The feedback input to configure.</param>
    /// <param name="placeholder">The placeholder text to display when the input is empty.</param>
    /// <returns>The feedback input with the specified placeholder text.</returns>
    public static FeedbackInputBase Placeholder(this FeedbackInputBase widget, string placeholder) => widget with { Placeholder = placeholder };

    /// <summary>Sets the disabled state of the feedback input.</summary>
    /// <param name="widget">The feedback input to configure.</param>
    /// <param name="enabled">Whether the input should be disabled.</param>
    public static FeedbackInputBase Disabled(this FeedbackInputBase widget, bool enabled = true) => widget with { Disabled = enabled };

    /// <summary>Sets the visual variant of the feedback input.</summary>
    /// <param name="widget">The feedback input to configure.</param>
    /// <param name="variant">The visual variant (Stars, Thumbs, or Emojis).</param>
    public static FeedbackInputBase Variant(this FeedbackInputBase widget, FeedbackInputs variant) => widget with { Variant = variant };

    /// <summary>Sets the validation error message for the feedback input.</summary>
    /// <param name="widget">The feedback input to configure.</param>
    /// <param name="invalid">The validation error message to display.</param>
    public static FeedbackInputBase Invalid(this FeedbackInputBase widget, string invalid) => widget with { Invalid = invalid };

    /// <summary>Sets the size of the feedback input.</summary>
    /// <param name="widget">The feedback input to configure.</param>
    /// <param name="size">The size of the feedback input.</param>
    public static FeedbackInputBase Size(this FeedbackInputBase widget, Sizes size) => widget with { Size = size };

    /// <summary>Sets the feedback input size to large for prominent display.</summary>
    /// <param name="widget">The feedback input to configure.</param>
    /// <returns>A new FeedbackInputBase instance with large size applied.</returns>
    public static FeedbackInputBase Large(this FeedbackInputBase widget) => widget.Size(Sizes.Large);

    /// <summary>Sets the feedback input size to small for compact display.</summary>
    /// <param name="widget">The feedback input to configure.</param>
    /// <returns>A new FeedbackInputBase instance with small size applied.</returns>
    public static FeedbackInputBase Small(this FeedbackInputBase widget) => widget.Size(Sizes.Small);

    /// <summary>
    /// Sets the blur event handler for the feedback input.
    /// </summary>
    /// <param name="widget">The feedback input to configure.</param>
    /// <param name="onBlur">The event handler to call when the input loses focus.</param>
    [OverloadResolutionPriority(1)]
    public static FeedbackInputBase HandleBlur(this FeedbackInputBase widget, Func<Event<IAnyInput>, ValueTask> onBlur)
    {
        return widget with { OnBlur = onBlur };
    }

    /// <summary>
    /// Sets the blur event handler for the feedback input.
    /// </summary>
    /// <param name="widget">The feedback input to configure.</param>
    /// <param name="onBlur">The event handler to call when the input loses focus.</param>
    public static FeedbackInputBase HandleBlur(this FeedbackInputBase widget, Action<Event<IAnyInput>> onBlur)
    {
        return widget.HandleBlur(onBlur.ToValueTask());
    }

    /// <summary>
    /// Sets a simple blur event handler for the feedback input.
    /// </summary>
    /// <param name="widget">The feedback input to configure.</param>
    /// <param name="onBlur">The simple action to perform when the input loses focus.</param>
    /// <returns>A new feedback input instance with the updated blur handler.</returns>
    public static FeedbackInputBase HandleBlur(this FeedbackInputBase widget, Action onBlur)
    {
        return widget.HandleBlur(_ => { onBlur(); return ValueTask.CompletedTask; });
    }
}