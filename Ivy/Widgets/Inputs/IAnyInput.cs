
using Ivy.Core;

namespace Ivy.Widgets.Inputs;

public interface IAnyInput
{
    [Prop] public bool Disabled { get; set; }
    [Prop] public string? Invalid { get; set; }
    [Event] public Action<Event<IAnyInput>>? OnBlur { get; set; }
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

    public static IAnyInput HandleBlur(this IAnyInput input, Action<Event<IAnyInput>>? onBlur)
    {
        input.OnBlur = onBlur;
        return input;
    }
}