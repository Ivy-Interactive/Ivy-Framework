using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Ivy.Core;
using Ivy.Shared;

namespace Ivy.Widgets.Inputs;

/// <summary> Base interface for all input controls in the Ivy Framework. </summary>
public interface IAnyInput
{
    /// <summary> Gets or sets whether the input control is disabled. </summary>
    [Prop] public bool Disabled { get; set; }

    /// <summary> Gets or sets the validation error message for this input control. </summary>
    [Prop] public string? Invalid { get; set; }

    /// <summary> Gets or sets the size of the input control. </summary>
    [Prop] public Sizes Size { get; set; }

    /// <summary> Gets or sets the event handler called when the input control loses focus. </summary>
    [Event] public Func<Event<IAnyInput>, ValueTask>? OnBlur { get; set; }

    /// <summary> Returns an array of types that this input control can bind to. </summary>
    public Type[] SupportedStateTypes();
}

/// <summary> Provides extension methods for configuring IAnyInput implementations with fluent syntax. </summary>
public static class AnyInputExtensions
{
    /// <summary> Sets the disabled state of the input control. </summary>
    /// <param name="input">The input control to configure.</param>
    /// <param name="disabled">true to disable the input; false to enable it. Default is true.</param>
    public static IAnyInput Disabled(this IAnyInput input, bool disabled = true)
    {
        input.Disabled = disabled;
        return input;
    }

    /// <summary> Sets the validation error message for the input control. </summary>
    /// <param name="input">The input control to configure.</param>
    /// <param name="invalid">The validation error message, or null to clear any existing error.</param>
    public static IAnyInput Invalid(this IAnyInput input, string? invalid)
    {
        input.Invalid = invalid;
        return input;
    }

    /// <summary> Sets the size of the input control. </summary>
    /// <param name="input">The input control to configure.</param>
    /// <param name="size">The size of the input control.</param>
    public static IAnyInput Size(this IAnyInput input, Sizes size)
    {
        input.Size = size;
        return input;
    }

    /// <summary> Sets the blur event handler for the input control. </summary>
    /// <param name="input">The input control to configure.</param>
    /// <param name="onBlur">The event handler to call when the input loses focus, or null to remove the handler.</param>
    [OverloadResolutionPriority(1)]
    public static IAnyInput HandleBlur(this IAnyInput input, Func<Event<IAnyInput>, ValueTask>? onBlur)
    {
        input.OnBlur = onBlur;
        return input;
    }

    /// <summary> Sets the blur event handler for the input control (Action-based). </summary>
    /// <param name="input">The input control to configure.</param>
    /// <param name="onBlur">The event handler to call when the input loses focus.</param>
    public static IAnyInput HandleBlur(this IAnyInput input, Action<Event<IAnyInput>> onBlur)
    {
        input.OnBlur = onBlur.ToValueTask();
        return input;
    }

    /// <summary> Sets a simple blur event handler for the input control. </summary>
    /// <param name="input">The input control to configure.</param>
    /// <param name="onBlur">The simple action to perform when the input loses focus.</param>
    public static IAnyInput HandleBlur(this IAnyInput input, Action onBlur)
    {
        input.OnBlur = _ => { onBlur(); return ValueTask.CompletedTask; };
        return input;
    }

    /// <summary> Sets the input control size to small for compact display. </summary>
    /// <param name="input">The input control to configure.</param>
    public static IAnyInput Small(this IAnyInput input)
    {
        input.Size = Sizes.Small;
        return input;
    }

    /// <summary> Sets the input control size to large for prominent display. </summary>
    /// <param name="input">The input control to configure.</param>
    public static IAnyInput Large(this IAnyInput input)
    {
        input.Size = Sizes.Large;
        return input;
    }
}