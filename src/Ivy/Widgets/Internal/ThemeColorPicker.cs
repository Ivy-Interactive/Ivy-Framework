using System.Runtime.CompilerServices;

namespace Ivy.Widgets.Internal;

public record ThemeColorPicker : ColorInput<string>
{
    [OverloadResolutionPriority(1)]
    public ThemeColorPicker(IAnyState state, string? placeholder = null, bool disabled = false)
        : base(state, placeholder, disabled, ColorInputVariant.TextAndPicker)
    {
    }

    [OverloadResolutionPriority(1)]
    public ThemeColorPicker(string value, Func<Event<IInput<string>, string>, ValueTask> onChange, string? placeholder = null, bool disabled = false)
        : base(value, onChange, placeholder, disabled, ColorInputVariant.TextAndPicker)
    {
    }

    public ThemeColorPicker(string value, Action<Event<IInput<string>, string>> onChange, string? placeholder = null, bool disabled = false)
        : base(value, onChange, placeholder, disabled, ColorInputVariant.TextAndPicker)
    {
    }

    public ThemeColorPicker(string? placeholder = null, bool disabled = false)
        : base(placeholder, disabled, ColorInputVariant.TextAndPicker)
    {
    }
}
