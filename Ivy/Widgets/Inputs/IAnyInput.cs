using System.Runtime.CompilerServices;
using Ivy.Core;

namespace Ivy.Widgets.Inputs;

public interface IAnyInput
{
    [Prop] public bool Disabled { get; set; }

    [Prop] public string? Placeholder { get; set; }

    [Prop] public string? Invalid { get; set; }

    [Event] public Func<Event<IAnyInput>, ValueTask>? OnBlur { get; set; }

    public Type[] SupportedStateTypes();
}

public static class AnyInputExtensions
{
    /// <param name="input">The input control to configure.</param>
    /// <param name="disabled">true to disable the input; false to enable it. Default is true.</param>
    public static IAnyInput Disabled(this IAnyInput input, bool disabled = true)
    {
        input.Disabled = disabled;
        return input;
    }

    /// <param name="input">The input control to configure.</param>
    /// <param name="invalid">The validation error message, or null to clear any existing error.</param>
    public static IAnyInput Invalid(this IAnyInput input, string? invalid)
    {
        input.Invalid = invalid;
        return input;
    }

    public static IAnyInput Placeholder(this IAnyInput input, string? placeholder)
    {
        input.Placeholder = placeholder;
        return input;
    }

    /// <param name="input">The input control to configure.</param>
    /// <param name="onBlur">The event handler to call when the input loses focus, or null to remove the handler.</param>
    [OverloadResolutionPriority(1)]
    public static IAnyInput HandleBlur(this IAnyInput input, Func<Event<IAnyInput>, ValueTask>? onBlur)
    {
        input.OnBlur = onBlur;
        return input;
    }

    /// <param name="input">The input control to configure.</param>
    /// <param name="onBlur">The event handler to call when the input loses focus.</param>
    public static IAnyInput HandleBlur(this IAnyInput input, Action<Event<IAnyInput>> onBlur)
    {
        input.OnBlur = onBlur.ToValueTask();
        return input;
    }

    /// <param name="input">The input control to configure.</param>
    /// <param name="onBlur">The simple action to perform when the input loses focus.</param>
    public static IAnyInput HandleBlur(this IAnyInput input, Action onBlur)
    {
        input.OnBlur = _ => { onBlur(); return ValueTask.CompletedTask; };
        return input;
    }
}