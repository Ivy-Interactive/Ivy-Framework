using System.Runtime.CompilerServices;
using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Widgets.Inputs;

namespace Ivy.Widgets.Internal;

public record ThemeColorPicker : ColorInput<string>
{
    [OverloadResolutionPriority(1)]
    public ThemeColorPicker(IAnyState state, string? placeholder = null, bool disabled = false)
        : base(state, placeholder, disabled, ColorInputVariants.TextAndPicker)
    {
    }

    [OverloadResolutionPriority(1)]
    public ThemeColorPicker(string value, Func<Event<IInput<string>, string>, ValueTask> onChange, string? placeholder = null, bool disabled = false)
        : base(value, onChange, placeholder, disabled, ColorInputVariants.TextAndPicker)
    {
    }

    public ThemeColorPicker(string value, Action<Event<IInput<string>, string>> onChange, string? placeholder = null, bool disabled = false)
        : base(value, onChange, placeholder, disabled, ColorInputVariants.TextAndPicker)
    {
    }

    public ThemeColorPicker(string? placeholder = null, bool disabled = false)
        : base(placeholder, disabled, ColorInputVariants.TextAndPicker)
    {
    }
}
