using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Ivy.Core;

namespace Ivy.Widgets.Inputs;

/// <summary>
/// Base interface for all input controls in the Ivy Framework. Defines common properties and behaviors
/// that all input widgets must implement, including validation state, disabled state, and blur events.
/// </summary>
public interface IAnyInput
{
    /// <summary>
    /// Gets or sets a value indicating whether the input control is disabled.
    /// When disabled, the input cannot receive focus, accept user input, or trigger events,
    /// and is typically rendered with a visual indication of its disabled state.
    /// </summary>
    [Prop] public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the validation error message for this input control.
    /// When not null, indicates that the input contains invalid data and displays
    /// the error message to provide feedback to the user about what needs to be corrected.
    /// </summary>
    [Prop] public string? Invalid { get; set; }

    /// <summary>
    /// Gets or sets the event handler called when the input control loses focus.
    /// The blur event is triggered when users navigate away from the input, typically
    /// used for validation, formatting, or other post-input processing.
    /// </summary>
    [Event] public Func<Event<IAnyInput>, ValueTask>? OnBlur { get; set; }

    /// <summary>
    /// Returns an array of types that this input control can bind to and work with.
    /// This method is used by the framework to determine type compatibility for automatic
    /// form generation and state binding, ensuring that inputs are matched with appropriate data types.
    /// </summary>
    public Type[] SupportedStateTypes();
}

/// <summary>
/// Provides extension methods for configuring IAnyInput implementations with fluent syntax.
/// </summary>
public static class AnyInputExtensions
{
    /// <summary>
    /// Sets the disabled state of the input control.
    /// </summary>
    /// <param name="input">The input control to configure.</param>
    /// <param name="disabled">true to disable the input; false to enable it. Default is true.</param>
    public static IAnyInput Disabled(this IAnyInput input, bool disabled = true)
    {
        input.Disabled = disabled;
        return input;
    }

    /// <summary>
    /// Sets the validation error message for the input control.
    /// </summary>
    /// <param name="input">The input control to configure.</param>
    /// <param name="invalid">The validation error message, or null to clear any existing error.</param>
    public static IAnyInput Invalid(this IAnyInput input, string? invalid)
    {
        input.Invalid = invalid;
        return input;
    }

    /// <summary>
    /// Sets the blur event handler for the input control.
    /// </summary>
    /// <param name="input">The input control to configure.</param>
    /// <param name="onBlur">The event handler to call when the input loses focus, or null to remove the handler.</param>
    [OverloadResolutionPriority(1)]
    public static IAnyInput HandleBlur(this IAnyInput input, Func<Event<IAnyInput>, ValueTask>? onBlur)
    {
        input.OnBlur = onBlur;
        return input;
    }

    /// <summary>
    /// Sets the blur event handler for the input control.
    /// Compatibility overload for Action-based event handlers.
    /// </summary>
    /// <param name="input">The input control to configure.</param>
    /// <param name="onBlur">The event handler to call when the input loses focus.</param>
    public static IAnyInput HandleBlur(this IAnyInput input, Action<Event<IAnyInput>> onBlur)
    {
        input.OnBlur = onBlur.ToValueTask();
        return input;
    }

    /// <summary>
    /// Sets a simple blur event handler for the input control.
    /// This method allows you to configure the input's blur behavior with
    /// a simple action that doesn't require the input event context.
    /// </summary>
    /// <param name="input">The input control to configure.</param>
    /// <param name="onBlur">The simple action to perform when the input loses focus.</param>
    public static IAnyInput HandleBlur(this IAnyInput input, Action onBlur)
    {
        input.OnBlur = _ => { onBlur(); return ValueTask.CompletedTask; };
        return input;
    }
}