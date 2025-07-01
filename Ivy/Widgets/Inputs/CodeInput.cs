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
    [Event] public Action<Event<IAnyInput>>? OnBlur { get; set; }
    public Type[] SupportedStateTypes() => [typeof(string)];
}

public record CodeInput<TString> : CodeInputBase, IInput<TString>
{
    public CodeInput(IAnyState state, string? placeholder = null, bool disabled = false, CodeInputs variant = CodeInputs.Default)
        : this(placeholder, disabled, variant)
    {
        var typedState = state.As<TString>();
        Value = typedState.Value;
        OnChange = e => typedState.Set(e.Value);
    }

    public CodeInput(TString value, Action<Event<IInput<TString>, TString>>? onChange = null, string? placeholder = null, bool disabled = false, CodeInputs variant = CodeInputs.Default)
        : this(placeholder, disabled, variant)
    {
        OnChange = onChange;
        Value = value;
    }

    public CodeInput(string? placeholder = null, bool disabled = false, CodeInputs variant = CodeInputs.Default)
    {
        Placeholder = placeholder;
        Variant = variant;
        Disabled = disabled;
        Width = Size.Full();
        Height = Size.Units(25);
    }

    [Prop] public TString Value { get; } = default!;
    [Event] public Action<Event<IInput<TString>, TString>>? OnChange { get; }
}

public static class CodeInputExtensions
{
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
}