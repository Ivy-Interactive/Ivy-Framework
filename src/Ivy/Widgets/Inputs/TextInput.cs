using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;
using Ivy.Shared;
using Ivy.Widgets.Inputs;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record Affix
{
    public Icons? Icon { get; init; }
    public string? Text { get; init; }
}

public static class AffixExtensions
{
    public static Affix ToAffix(this Icons icon) => new() { Icon = icon };
    public static Affix ToAffix(this string text) => new() { Text = text };
}

public enum TextInputVariants
{
    Text,
    Textarea,
    Email,
    Tel,
    Url,
    Password,
    Search
}

public interface IAnyTextInput : IAnyInput
{
    public TextInputVariants Variant { get; set; }
}

public abstract record TextInputBase : WidgetBase<TextInputBase>, IAnyTextInput
{
    [Prop] public bool Disabled { get; set; }

    [Prop] public string? Invalid { get; set; }

    [Prop] public string? Placeholder { get; set; }

    [Prop] public TextInputVariants Variant { get; set; } = TextInputVariants.Text;

    [Prop] public string? ShortcutKey { get; set; }

    [Prop] public Affix? Prefix { get; set; }

    [Prop] public Affix? Suffix { get; set; }

    [Prop] public int? MaxLength { get; set; }

    [Prop] public int? MinLength { get; set; }

    [Prop] public int? Rows { get; set; }

    [Prop] public bool Nullable { get; set; }

    [Event] public EventHandler<Event<IAnyInput>>? OnBlur { get; set; }

    public Type[] SupportedStateTypes() => [];
}

public record TextInput<TString> : TextInputBase, IInput<TString>
{
    public TextInput(IAnyState state, string? placeholder = null, bool disabled = false, TextInputVariants variant = TextInputVariants.Text)
        : this(placeholder, disabled, variant)
    {
        var typedState = state.As<TString>();
        Value = typedState.Value;
        OnChange = new(e => { typedState.Set(e.Value); return ValueTask.CompletedTask; });
    }

    [OverloadResolutionPriority(1)]
    public TextInput(TString value, Func<Event<IInput<TString>, TString>, ValueTask>? onChange = null, string? placeholder = null, bool disabled = false, TextInputVariants variant = TextInputVariants.Text)
        : this(placeholder, disabled, variant)
    {
        OnChange = onChange.ToEventHandler();
        Value = value;
    }

    public TextInput(TString value, Action<Event<IInput<TString>, TString>>? onChange = null, string? placeholder = null, bool disabled = false, TextInputVariants variant = TextInputVariants.Text)
        : this(placeholder, disabled, variant)
    {
        OnChange = onChange.ToEventHandler();
        Value = value;
    }

    public TextInput(string? placeholder = null, bool disabled = false, TextInputVariants variant = TextInputVariants.Text)
    {
        Placeholder = placeholder;
        Variant = variant;
        Disabled = disabled;
    }

    internal TextInput() { }

    [Prop] public TString Value { get; init; } = default!;

    [Prop] public new bool Nullable { get; set; } = typeof(TString).IsNullableType();

    [Event] public EventHandler<Event<IInput<TString>, TString>>? OnChange { get; }
}

/// <summary>
/// A standard input field for single-line text.
/// </summary>
public record TextInput : TextInput<string>
{
    public TextInput(IAnyState state, string? placeholder = null, bool disabled = false, TextInputVariants variant = TextInputVariants.Text)
        : base(state, placeholder, disabled, variant)
    {
    }

    [OverloadResolutionPriority(1)]
    public TextInput(string value, Func<Event<IInput<string>, string>, ValueTask>? onChange = null, string? placeholder = null, bool disabled = false, TextInputVariants variant = TextInputVariants.Text)
        : base(value, onChange, placeholder, disabled, variant)
    {
    }

    public TextInput(string value, Action<Event<IInput<string>, string>>? onChange = null, string? placeholder = null, bool disabled = false, TextInputVariants variant = TextInputVariants.Text)
        : base(value, onChange?.ToValueTask(), placeholder, disabled, variant)
    {
    }

    public TextInput(string? placeholder = null, bool disabled = false, TextInputVariants variant = TextInputVariants.Text)
        : base(placeholder, disabled, variant)
    {
    }
}

public static class TextInputExtensions
{
    public static TextInputBase ToTextInput(this IAnyState state, string? placeholder = null, bool disabled = false, TextInputVariants variant = TextInputVariants.Text)
    {
        var type = state.GetStateType();
        Type genericType = typeof(TextInput<>).MakeGenericType(type);
        TextInputBase input = (TextInputBase)Activator.CreateInstance(genericType, state, placeholder, disabled, variant)!;
        var nullableProperty = genericType.GetProperty("Nullable", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        nullableProperty?.SetValue(input, type.IsNullableType());
        return input;
    }

    public static TextInputBase ToTextareaInput(this IAnyState state, string? placeholder = null, bool disabled = false) => state.ToTextInput(placeholder, disabled, TextInputVariants.Textarea);

    public static TextInputBase ToSearchInput(this IAnyState state, string? placeholder = null, bool disabled = false) => state.ToTextInput(placeholder, disabled, TextInputVariants.Search);

    public static TextInputBase ToPasswordInput(this IAnyState state, string? placeholder = null, bool disabled = false) => state.ToTextInput(placeholder, disabled, TextInputVariants.Password);

    public static TextInputBase ToEmailInput(this IAnyState state, string? placeholder = null, bool disabled = false) => state.ToTextInput(placeholder, disabled, TextInputVariants.Email);

    public static TextInputBase ToUrlInput(this IAnyState state, string? placeholder = null, bool disabled = false) => state.ToTextInput(placeholder, disabled, TextInputVariants.Url);

    public static TextInputBase ToTelInput(this IAnyState state, string? placeholder = null, bool disabled = false) => state.ToTextInput(placeholder, disabled, TextInputVariants.Tel);

    public static TextInputBase Placeholder(this TextInputBase widget, string placeholder) => widget with { Placeholder = placeholder };

    public static TextInputBase Disabled(this TextInputBase widget, bool disabled = true) => widget with { Disabled = disabled };

    public static TextInputBase Variant(this TextInputBase widget, TextInputVariants variant) => widget with { Variant = variant };

    public static TextInputBase Multiline(this TextInputBase widget, bool multiline = true)
        => widget with { Variant = multiline ? TextInputVariants.Textarea : TextInputVariants.Text };

    public static TextInputBase Invalid(this TextInputBase widget, string invalid) => widget with { Invalid = invalid };

    public static TextInputBase Nullable(this TextInputBase widget, bool? nullable = true)
    {
        var property = widget.GetType().GetProperty("Nullable", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        if (property != null && property.CanWrite)
        {
            property.SetValue(widget, nullable ?? true);
            return widget;
        }
        return widget with { Nullable = nullable ?? true };
    }

    public static TextInputBase ShortcutKey(this TextInputBase widget, string shortcutKey) => widget with { ShortcutKey = shortcutKey };

    public static TextInputBase MaxLength(this TextInputBase widget, int maxLength) => widget with { MaxLength = maxLength };

    public static TextInputBase MinLength(this TextInputBase widget, int minLength) => widget with { MinLength = minLength };

    public static TextInputBase Rows(this TextInputBase widget, int rows) => widget with { Rows = rows };

    public static TextInputBase Prefix(this TextInputBase widget, string prefixText)
        => widget with { Prefix = prefixText.ToAffix() };

    public static TextInputBase Prefix(this TextInputBase widget, Icons prefixIcon)
        => widget with { Prefix = prefixIcon.ToAffix() };

    public static TextInputBase Suffix(this TextInputBase widget, string suffixText)
        => widget with { Suffix = suffixText.ToAffix() };

    public static TextInputBase Suffix(this TextInputBase widget, Icons suffixIcon)
        => widget with { Suffix = suffixIcon.ToAffix() };

    [OverloadResolutionPriority(1)]
    public static TextInputBase OnBlur(this TextInputBase widget, Func<Event<IAnyInput>, ValueTask> onBlur)
    {
        return widget with { OnBlur = new(onBlur) };
    }

    public static TextInputBase OnBlur(this TextInputBase widget, Action<Event<IAnyInput>> onBlur)
    {
        return widget with { OnBlur = new(onBlur.ToValueTask()) };
    }

    public static TextInputBase OnBlur(this TextInputBase widget, Action onBlur)
    {
        return widget with { OnBlur = new(_ => { onBlur(); return ValueTask.CompletedTask; }) };
    }

    public static TextInputBase Value<T>(this TextInputBase widget, T value)
    {
        if (widget is TextInput<T> typedWidget)
        {
            return typedWidget with { Value = value };
        }
        throw new InvalidOperationException($"Cannot set Value: widget is not TextInput<{typeof(T).Name}>");
    }

}