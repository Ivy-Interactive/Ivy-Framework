using System.Runtime.CompilerServices;
using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

public interface IAnyInput
{
    [Prop] public bool Disabled { get; set; }

    [Prop] public string? Placeholder { get; set; }

    [Prop] public string? Invalid { get; set; }
    [Prop] public bool Nullable { get; set; }
    [Prop] public bool AutoFocus { get; set; }

    [Event] public EventHandler<Event<IAnyInput>>? OnBlur { get; set; }
    [Event] public EventHandler<Event<IAnyInput>>? OnFocus { get; set; }

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
    public static IAnyInput Nullable(this IAnyInput input, bool? nullable = true)
    {
        input.Nullable = nullable ?? true;
        return input;
    }

    public static IAnyInput AutoFocus(this IAnyInput input, bool autoFocus = true)
    {
        input.AutoFocus = autoFocus;
        return input;
    }

    [OverloadResolutionPriority(1)]
    public static IAnyInput OnBlur(this IAnyInput input, Func<Event<IAnyInput>, ValueTask>? onBlur)
    {
        input.OnBlur = onBlur.ToEventHandler();
        return input;
    }

    public static IAnyInput OnBlur(this IAnyInput input, Action<Event<IAnyInput>> onBlur)
    {
        input.OnBlur = new(onBlur.ToValueTask());
        return input;
    }

    public static IAnyInput OnBlur(this IAnyInput input, Action onBlur)
    {
        input.OnBlur = new(_ => { onBlur(); return ValueTask.CompletedTask; });
        return input;
    }

    [OverloadResolutionPriority(1)]
    public static IAnyInput OnFocus(this IAnyInput input, Func<Event<IAnyInput>, ValueTask>? onFocus)
    {
        input.OnFocus = onFocus.ToEventHandler();
        return input;
    }

    public static IAnyInput OnFocus(this IAnyInput input, Action<Event<IAnyInput>> onFocus)
    {
        input.OnFocus = new(onFocus.ToValueTask());
        return input;
    }

    public static IAnyInput OnFocus(this IAnyInput input, Action onFocus)
    {
        input.OnFocus = new(_ => { onFocus(); return ValueTask.CompletedTask; });
        return input;
    }
}