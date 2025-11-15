using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;
using Ivy.Widgets.Inputs;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

public enum TextInputs
{
    Text,
    Textarea,
    Email,
    Tel,
    Url,
    Password,
    Search
}

public abstract record PrefixSuffix
{
    private PrefixSuffix() { } // Prevent external inheritance

    public sealed record Text(string Value) : PrefixSuffix;

    public sealed record Icon(Icons Value) : PrefixSuffix;
}

/// <summary> JSON converter factory for PrefixSuffix discriminated union. </summary>
internal class PrefixSuffixJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(PrefixSuffix).IsAssignableFrom(typeToConvert);
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return new PrefixSuffixJsonConverter();
    }
}

/// <summary> JSON converter for PrefixSuffix discriminated union. </summary>
internal class PrefixSuffixJsonConverter : JsonConverter<PrefixSuffix>
{
    public override PrefixSuffix? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (!root.TryGetProperty("type", out var typeElement) || !root.TryGetProperty("value", out var valueElement))
        {
            return null;
        }

        var type = typeElement.GetString();

        return type switch
        {
            "text" => new PrefixSuffix.Text(valueElement.GetString() ?? string.Empty),
            "icon" => Enum.TryParse<Icons>(valueElement.GetString(), out var iconValue) ? new PrefixSuffix.Icon(iconValue) : null,
            _ => null
        };
    }

    public override void Write(Utf8JsonWriter writer, PrefixSuffix value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        switch (value)
        {
            case PrefixSuffix.Text text:
                writer.WriteString("type", "text");
                writer.WriteString("value", text.Value);
                break;
            case PrefixSuffix.Icon icon:
                writer.WriteString("type", "icon");
                writer.WriteString("value", icon.Value.ToString());
                break;
        }

        writer.WriteEndObject();
    }
}

/// <summary> Interface for text input controls that extends IAnyInput with text-specific properties. </summary>
public interface IAnyTextInput : IAnyInput
{
    public string? Placeholder { get; set; }

    public TextInputs Variant { get; set; }
}

/// <summary> Abstract base class for text input controls that provides common text input functionality. </summary>
public abstract record TextInputBase : WidgetBase<TextInputBase>, IAnyTextInput
{
    [Prop] public bool Disabled { get; set; }

    [Prop] public string? Invalid { get; set; }

    [Prop] public string? Placeholder { get; set; }

    [Prop] public TextInputs Variant { get; set; }

    [Prop] public string? ShortcutKey { get; set; }

    [Prop] public Sizes Size { get; set; } = Sizes.Medium;

    [Prop] public PrefixSuffix? Prefix { get; set; }

    [Prop] public PrefixSuffix? Suffix { get; set; }

    [Event] public Func<Event<IAnyInput>, ValueTask>? OnBlur { get; set; }

    /// <summary> Returns the types that this text input can bind to and work with. </summary>
    public Type[] SupportedStateTypes() => [];
}

/// <summary> Generic text input control that provides type-safe text entry functionality for string-like types. </summary>
/// <typeparam name="TString">The type of the text value (typically string or string-convertible types).</typeparam>
public record TextInput<TString> : TextInputBase, IInput<TString>
{
    /// <summary> Initializes a new text input bound to a state object for automatic value synchronization. </summary>
    /// <param name="state">The state object to bind the text input to.</param>
    /// <param name="placeholder">Optional placeholder text displayed when the input is empty.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <param name="variant">The visual and functional variant of the text input.</param>
    public TextInput(IAnyState state, string? placeholder = null, bool disabled = false, TextInputs variant = TextInputs.Text)
        : this(placeholder, disabled, variant)
    {
        var typedState = state.As<TString>();
        Value = typedState.Value;
        OnChange = e => { typedState.Set(e.Value); return ValueTask.CompletedTask; };
    }

    /// <summary>
    /// Initializes a new text input with an explicit value and change handler.
    /// Useful for manual state management or when custom change handling is required.
    /// </summary>
    /// <param name="value">The initial text value.</param>
    /// <param name="onChange">Optional event handler called when the input value changes.</param>
    /// <param name="placeholder">Optional placeholder text displayed when the input is empty.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <param name="variant">The visual and functional variant of the text input.</param>
    [OverloadResolutionPriority(1)]
    public TextInput(TString value, Func<Event<IInput<TString>, TString>, ValueTask>? onChange = null, string? placeholder = null, bool disabled = false, TextInputs variant = TextInputs.Text)
        : this(placeholder, disabled, variant)
    {
        OnChange = onChange;
        Value = value;
    }

    public TextInput(TString value, Action<Event<IInput<TString>, TString>>? onChange = null, string? placeholder = null, bool disabled = false, TextInputs variant = TextInputs.Text)
        : this(placeholder, disabled, variant)
    {
        OnChange = onChange?.ToValueTask();
        Value = value;
    }

    /// <summary> Initializes a new text input with basic configuration. </summary>
    public TextInput(string? placeholder = null, bool disabled = false, TextInputs variant = TextInputs.Text)
    {
        Placeholder = placeholder;
        Variant = variant;
        Disabled = disabled;
    }

    [Prop] public TString Value { get; } = default!;

    [Event] public Func<Event<IInput<TString>, TString>, ValueTask>? OnChange { get; }
}

/// <summary>Concrete text input control for string values that provides convenient string-specific text entry functionality.</summary>
public record TextInput : TextInput<string>
{
    /// <summary>
    /// Initializes a new string text input bound to a state object for automatic value synchronization.
    /// The input will display the current state value and update the state when text changes.
    /// </summary>
    /// <param name="state">The state object to bind to for automatic value updates and change handling.</param>
    /// <param name="placeholder">Optional placeholder text displayed when the input is empty.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <param name="variant">The visual and functional variant of the text input.</param>
    public TextInput(IAnyState state, string? placeholder = null, bool disabled = false, TextInputs variant = TextInputs.Text)
        : base(state, placeholder, disabled, variant)
    {
    }

    /// <summary>
    /// Initializes a new string text input with an explicit value and change handler.
    /// Useful for manual state management or when custom change handling is required.
    /// </summary>
    /// <param name="value">The initial string value.</param>
    /// <param name="onChange">Optional event handler called when the text value changes.</param>
    /// <param name="placeholder">Optional placeholder text displayed when the input is empty.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <param name="variant">The visual and functional variant of the text input.</param>
    [OverloadResolutionPriority(1)]
    public TextInput(string value, Func<Event<IInput<string>, string>, ValueTask>? onChange = null, string? placeholder = null, bool disabled = false, TextInputs variant = TextInputs.Text)
        : base(value, onChange, placeholder, disabled, variant)
    {
    }

    // Overload for Action<Event<IInput<string>, string>>
    public TextInput(string value, Action<Event<IInput<string>, string>>? onChange = null, string? placeholder = null, bool disabled = false, TextInputs variant = TextInputs.Text)
        : base(value, onChange?.ToValueTask(), placeholder, disabled, variant)
    {
    }

    /// <summary> Initializes a new string text input with basic configuration. </summary>
    /// <param name="placeholder">Optional placeholder text displayed when the input is empty.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <param name="variant">The visual and functional variant of the text input.</param>
    public TextInput(string? placeholder = null, bool disabled = false, TextInputs variant = TextInputs.Text)
        : base(placeholder, disabled, variant)
    {
    }
}

/// <summary> Provides extension methods for creating and configuring text input controls with fluent syntax. </summary>
public static class TextInputExtensions
{
    /// <summary> Creates a text input from a state object with automatic type detection. </summary>
    /// <param name="state">The state object to bind to.</param>
    /// <param name="placeholder">Optional placeholder text displayed when the input is empty.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <param name="variant">The visual and functional variant of the text input.</param>
    public static TextInputBase ToTextInput(this IAnyState state, string? placeholder = null, bool disabled = false, TextInputs variant = TextInputs.Text)
    {
        var type = state.GetStateType();
        Type genericType = typeof(TextInput<>).MakeGenericType(type);
        TextInputBase input = (TextInputBase)Activator.CreateInstance(genericType, state, placeholder, disabled, variant)!;
        return input;
    }

    /// <summary> Creates a multi-line textarea input from a state object. </summary>
    /// <param name="state">The state object to bind to.</param>
    /// <param name="placeholder">Optional placeholder text displayed when the input is empty.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    public static TextInputBase ToTextAreaInput(this IAnyState state, string? placeholder = null, bool disabled = false) => state.ToTextInput(placeholder, disabled, TextInputs.Textarea);

    /// <summary> Creates a search input from a state object. </summary>
    /// <param name="state">The state object to bind to.</param>
    /// <param name="placeholder">Optional placeholder text displayed when the input is empty.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    public static TextInputBase ToSearchInput(this IAnyState state, string? placeholder = null, bool disabled = false) => state.ToTextInput(placeholder, disabled, TextInputs.Search);

    /// <summary> Creates a password input from a state object. </summary>
    /// <param name="state">The state object to bind to.</param>
    /// <param name="placeholder">Optional placeholder text displayed when the input is empty.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    public static TextInputBase ToPasswordInput(this IAnyState state, string? placeholder = null, bool disabled = false) => state.ToTextInput(placeholder, disabled, TextInputs.Password);

    /// <summary> Creates an email input from a state object. </summary>
    /// <param name="state">The state object to bind to.</param>
    /// <param name="placeholder">Optional placeholder text displayed when the input is empty.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    public static TextInputBase ToEmailInput(this IAnyState state, string? placeholder = null, bool disabled = false) => state.ToTextInput(placeholder, disabled, TextInputs.Email);

    /// <summary> Creates a URL input from a state object.</summary>
    /// <param name="state">The state object to bind to.</param>
    /// <param name="placeholder">Optional placeholder text displayed when the input is empty.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    public static TextInputBase ToUrlInput(this IAnyState state, string? placeholder = null, bool disabled = false) => state.ToTextInput(placeholder, disabled, TextInputs.Url);

    /// <summary> Creates a telephone input from a state object. </summary>
    /// <param name="state">The state object to bind to.</param>
    /// <param name="placeholder">Optional placeholder text displayed when the input is empty.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    public static TextInputBase ToTelInput(this IAnyState state, string? placeholder = null, bool disabled = false) => state.ToTextInput(placeholder, disabled, TextInputs.Tel);

    /// <param name="widget">The text input to configure.</param>
    /// <param name="placeholder">The placeholder text to display when the input is empty.</param>
    public static TextInputBase Placeholder(this TextInputBase widget, string placeholder) => widget with { Placeholder = placeholder };

    /// <param name="widget">The text input to configure.</param>
    /// <param name="disabled">Whether the input should be disabled.</param>
    public static TextInputBase Disabled(this TextInputBase widget, bool disabled = true) => widget with { Disabled = disabled };

    /// <param name="widget">The text input to configure.</param>
    /// <param name="variant">The text input variant (Text, Textarea, Email, Tel, Url, Password, or Search).</param>
    public static TextInputBase Variant(this TextInputBase widget, TextInputs variant) => widget with { Variant = variant };

    /// <param name="widget">The text input to configure.</param>
    /// <param name="invalid">The validation error message to display.</param>
    public static TextInputBase Invalid(this TextInputBase widget, string invalid) => widget with { Invalid = invalid };

    /// <param name="widget">The text input to configure.</param>
    /// <param name="shortcutKey">The keyboard shortcut key combination for focusing this input.</param>
    public static TextInputBase ShortcutKey(this TextInputBase widget, string shortcutKey) => widget with { ShortcutKey = shortcutKey };

    public static TextInputBase Size(this TextInputBase widget, Sizes size) => widget with { Size = size };

    public static TextInputBase Large(this TextInputBase widget) => widget.Size(Sizes.Large);

    public static TextInputBase Small(this TextInputBase widget) => widget.Size(Sizes.Small);

    /// <param name="widget">The text input to configure.</param>
    /// <param name="prefixText">The text to display before the input.</param>
    public static TextInputBase Prefix(this TextInputBase widget, string prefixText)
        => widget with { Prefix = new PrefixSuffix.Text(prefixText) };

    /// <param name="widget">The text input to configure.</param>
    /// <param name="prefixIcon">The icon to display before the input.</param>
    public static TextInputBase Prefix(this TextInputBase widget, Icons prefixIcon)
        => widget with { Prefix = new PrefixSuffix.Icon(prefixIcon) };

    /// <param name="widget">The text input to configure.</param>
    /// <param name="suffixText">The text to display after the input.</param>
    public static TextInputBase Suffix(this TextInputBase widget, string suffixText)
        => widget with { Suffix = new PrefixSuffix.Text(suffixText) };

    /// <param name="widget">The text input to configure.</param>
    /// <param name="suffixIcon">The icon to display after the input.</param>
    public static TextInputBase Suffix(this TextInputBase widget, Icons suffixIcon)
        => widget with { Suffix = new PrefixSuffix.Icon(suffixIcon) };

    /// <summary> Sets the blur event handler for the text input. </summary>
    /// <param name="widget">The text input to configure.</param>
    /// <param name="onBlur">The event handler to call when the input loses focus.</param>
    [OverloadResolutionPriority(1)]
    public static TextInputBase HandleBlur(this TextInputBase widget, Func<Event<IAnyInput>, ValueTask> onBlur)
    {
        return widget with { OnBlur = onBlur };
    }

    /// <summary> Sets the blur event handler for the text input. </summary>
    /// <param name="widget">The text input to configure.</param>
    /// <param name="onBlur">The event handler to call when the input loses focus.</param>
    public static TextInputBase HandleBlur(this TextInputBase widget, Action<Event<IAnyInput>> onBlur)
    {
        return widget.HandleBlur(onBlur.ToValueTask());
    }

    /// <summary> Sets a simple blur event handler for the text input. </summary>
    /// <param name="widget">The text input to configure.</param>
    /// <param name="onBlur">The simple action to perform when the input loses focus.</param>
    public static TextInputBase HandleBlur(this TextInputBase widget, Action onBlur)
    {
        return widget.HandleBlur(_ => { onBlur(); return ValueTask.CompletedTask; });
    }
}