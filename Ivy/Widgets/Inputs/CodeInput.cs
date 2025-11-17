using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;
using Ivy.Shared;
using Ivy.Widgets.Inputs;

// ReSharper disable once CheckNamespace
namespace Ivy;

public enum CodeInputs
{
    Default
}

public interface IAnyCodeInput : IAnyInput
{
    public string? Placeholder { get; set; }

    public CodeInputs Variant { get; set; }
}

public abstract record CodeInputBase : WidgetBase<CodeInputBase>, IAnyCodeInput
{
    [Prop] public bool Disabled { get; set; }

    [Prop] public string? Invalid { get; set; }

    [Prop] public string? Placeholder { get; set; }

    [Prop] public CodeInputs Variant { get; set; }

    [Prop] public Languages? Language { get; set; } = null;

    [Prop] public bool ShowCopyButton { get; set; } = false;

    [Prop] public Sizes Size { get; set; }

    [Event] public Func<Event<IAnyInput>, ValueTask>? OnBlur { get; set; }

    public Type[] SupportedStateTypes() => [typeof(string)];
}

/// <typeparam name="TString">Typically string.</typeparam>
public record CodeInput<TString> : CodeInputBase, IInput<TString>
{
    [OverloadResolutionPriority(1)]
    public CodeInput(IAnyState state, string? placeholder = null, bool disabled = false, CodeInputs variant = CodeInputs.Default)
        : this(placeholder, disabled, variant)
    {
        var typedState = state.As<TString>();
        Value = typedState.Value;
        OnChange = e => { typedState.Set(e.Value); return ValueTask.CompletedTask; };
    }

    [OverloadResolutionPriority(1)]
    public CodeInput(TString value, Func<Event<IInput<TString>, TString>, ValueTask>? onChange = null, string? placeholder = null, bool disabled = false, CodeInputs variant = CodeInputs.Default)
        : this(placeholder, disabled, variant)
    {
        OnChange = onChange;
        Value = value;
    }

    public CodeInput(TString value, Action<Event<IInput<TString>, TString>>? onChange = null, string? placeholder = null, bool disabled = false, CodeInputs variant = CodeInputs.Default)
        : this(placeholder, disabled, variant)
    {
        OnChange = onChange == null ? null : e => { onChange(e); return ValueTask.CompletedTask; };
        Value = value;
    }

    public CodeInput(string? placeholder = null, bool disabled = false, CodeInputs variant = CodeInputs.Default)
    {
        Placeholder = placeholder;
        Variant = variant;
        Disabled = disabled;
        Size = Sizes.Medium;
        Width = Ivy.Shared.Size.Full();
        Height = Ivy.Shared.Size.Units(25);
    }

    [Prop] public TString Value { get; } = default!;

    [Event] public Func<Event<IInput<TString>, TString>, ValueTask>? OnChange { get; }
}

public static class CodeInputExtensions
{
    /// <summary>Creates a code input from a state object with automatic type binding.</summary>
    public static CodeInputBase ToCodeInput(this IAnyState state, string? placeholder = null, bool disabled = false, CodeInputs variant = CodeInputs.Default, Languages language = Languages.Json)
    {
        var type = state.GetStateType();
        Type genericType = typeof(CodeInput<>).MakeGenericType(type);
        CodeInputBase input = (CodeInputBase)Activator.CreateInstance(genericType, state, placeholder, disabled, variant)!;
        return input;
    }

    public static CodeInputBase Placeholder(this CodeInputBase widget, string placeholder)
    {
        return widget with { Placeholder = placeholder };
    }

    public static CodeInputBase Disabled(this CodeInputBase widget, bool disabled = true)
    {
        return widget with { Disabled = disabled };
    }

    public static CodeInputBase Variant(this CodeInputBase widget, CodeInputs variant)
    {
        return widget with { Variant = variant };
    }

    public static CodeInputBase Invalid(this CodeInputBase widget, string invalid)
    {
        return widget with { Invalid = invalid };
    }

    public static CodeInputBase Language(this CodeInputBase widget, Languages language)
    {
        return widget with { Language = language };
    }

    public static CodeInputBase ShowCopyButton(this CodeInputBase widget, bool showCopyButton = true)
    {
        return widget with { ShowCopyButton = showCopyButton };
    }

    public static CodeInputBase Size(this CodeInputBase widget, Sizes size)
    {
        return widget with { Size = size };
    }

    public static CodeInputBase Large(this CodeInputBase widget)
    {
        return widget.Size(Sizes.Large);
    }

    public static CodeInputBase Small(this CodeInputBase widget)
    {
        return widget.Size(Sizes.Small);
    }

    [OverloadResolutionPriority(1)]
    public static CodeInputBase HandleBlur(this CodeInputBase widget, Func<Event<IAnyInput>, ValueTask> onBlur)
    {
        return widget with { OnBlur = onBlur };
    }

    public static CodeInputBase HandleBlur(this CodeInputBase widget, Action<Event<IAnyInput>> onBlur)
    {
        return widget.HandleBlur(onBlur.ToValueTask());
    }

    public static CodeInputBase HandleBlur(this CodeInputBase widget, Action onBlur)
    {
        return widget.HandleBlur(_ => { onBlur(); return ValueTask.CompletedTask; });
    }
}