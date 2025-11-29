using System.Runtime.CompilerServices;
using Ivy.Core;
using Ivy.Shared;

namespace Ivy.Widgets.Inputs;

public interface IAnyInput
{
    [Prop] public bool Disabled { get; set; }

    [Prop] public string? Placeholder { get; set; }

    [Prop] public string? Invalid { get; set; }

    [Prop] public Scale? Scale { get; set; }

    [Event] public Func<Event<IAnyInput>, ValueTask>? OnBlur { get; set; }

    public Type[] SupportedStateTypes();
}

public static class AnyInputExtensions
{
    public static IAnyInput Disabled(this IAnyInput input, bool disabled = true)
    {
        input.Disabled = disabled;
        return input;
    }

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

    [OverloadResolutionPriority(1)]
    public static IAnyInput HandleBlur(this IAnyInput input, Func<Event<IAnyInput>, ValueTask>? onBlur)
    {
        input.OnBlur = onBlur;
        return input;
    }

    public static IAnyInput HandleBlur(this IAnyInput input, Action<Event<IAnyInput>> onBlur)
    {
        input.OnBlur = onBlur.ToValueTask();
        return input;
    }

    public static IAnyInput HandleBlur(this IAnyInput input, Action onBlur)
    {
        input.OnBlur = _ => { onBlur(); return ValueTask.CompletedTask; };
        return input;
    }
}