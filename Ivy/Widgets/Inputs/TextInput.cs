using System.Runtime.CompilerServices;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;
using Ivy.Views.Forms;
using Ivy.Widgets.Inputs;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary> Defines the visual and functional variants available for text input controls. </summary>
public enum TextInputs
{
    /// <summary>Standard single-line text input for general text entry.</summary>
    Text,
    /// <summary>Multi-line textarea input for longer text content and descriptions.</summary>
    Textarea,
    /// <summary>Email input with email-specific validation and keyboard optimization.</summary>
    Email,
    /// <summary>Telephone input optimized for phone number entry with appropriate keyboard.</summary>
    Tel,
    /// <summary>URL input with URL validation and web address formatting.</summary>
    Url,
    /// <summary>Password input with masked character display for secure text entry.</summary>
    Password,
    /// <summary>Search input optimized for search queries with search-specific styling and behavior.</summary>
    Search
}

/// <summary> Interface for text input controls that extends IAnyInput with text-specific properties. </summary>
public interface IAnyTextInput : IAnyInput
{
    /// <summary>Gets or sets the placeholder text displayed when the input is empty.</summary>
    public string? Placeholder { get; set; }

    /// <summary>Gets or sets the visual and functional variant of the text input.</summary>
    public TextInputs Variant { get; set; }
}

/// <summary> Abstract base class for text input controls that provides common text input functionality. </summary>
public abstract record TextInputBase : WidgetBase<TextInputBase>, IAnyTextInput
{
    /// <summary>Gets or sets whether the input is disabled.</summary>
    [Prop] public bool Disabled { get; set; }

    /// <summary>Gets or sets the validation error message.</summary>
    [Prop] public string? Invalid { get; set; }

    /// <summary>Gets or sets the placeholder text displayed when the input is empty.</summary>
    [Prop] public string? Placeholder { get; set; }

    /// <summary>Gets or sets the visual and functional variant of the text input.</summary>
    [Prop] public TextInputs Variant { get; set; }

    /// <summary>Internal: Gets or sets the state object this input is bound to (used for automatic validation).</summary>
    internal IAnyState? BoundState { get; set; }

    /// <summary>Gets or sets the keyboard shortcut key for focusing this input.</summary>
    [Prop] public string? ShortcutKey { get; set; }

    /// <summary>Gets or sets the size of the text input.</summary>
    [Prop] public Sizes Size { get; set; } = Sizes.Medium;

    /// <summary>Gets or sets the event handler called when the input loses focus.</summary>
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
        BoundState = state;
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

    /// <summary>Gets the current text value.</summary>
    [Prop] public TString Value { get; } = default!;

    /// <summary>Gets the event handler called when the text value changes.</summary>
    [Event] public Func<Event<IInput<TString>, TString>, ValueTask>? OnChange { get; }
}

/// <summary>
/// Concrete text input control for string values that provides convenient string-specific text entry functionality.
/// </summary>
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

    /// <summary>Sets the placeholder text for the text input.</summary>
    /// <param name="widget">The text input to configure.</param>
    /// <param name="placeholder">The placeholder text to display when the input is empty.</param>
    public static TextInputBase Placeholder(this TextInputBase widget, string placeholder) => widget with { Placeholder = placeholder };

    /// <summary>Sets the disabled state of the text input.</summary>
    /// <param name="widget">The text input to configure.</param>
    /// <param name="disabled">Whether the input should be disabled.</param>
    public static TextInputBase Disabled(this TextInputBase widget, bool disabled = true) => widget with { Disabled = disabled };

    /// <summary>Sets the visual and functional variant of the text input.</summary>
    /// <param name="widget">The text input to configure.</param>
    /// <param name="variant">The text input variant (Text, Textarea, Email, Tel, Url, Password, or Search).</param>
    public static TextInputBase Variant(this TextInputBase widget, TextInputs variant) => widget with { Variant = variant };

    /// <summary>Sets the validation error message for the text input.</summary>
    /// <param name="widget">The text input to configure.</param>
    /// <param name="invalid">The validation error message to display.</param>
    public static TextInputBase Invalid(this TextInputBase widget, string invalid) => widget with { Invalid = invalid };

    /// <summary>Sets the keyboard shortcut key for focusing the text input.</summary>
    /// <param name="widget">The text input to configure.</param>
    /// <param name="shortcutKey">The keyboard shortcut key combination for focusing this input.</param>
    public static TextInputBase ShortcutKey(this TextInputBase widget, string shortcutKey) => widget with { ShortcutKey = shortcutKey };

    /// <summary>Sets the size of the text input.</summary>
    public static TextInputBase Size(this TextInputBase widget, Sizes size) => widget with { Size = size };

    /// <summary>Sets the text input size to large for prominent display.</summary>
    public static TextInputBase Large(this TextInputBase widget) => widget.Size(Sizes.Large);

    /// <summary>Sets the text input size to small for compact display.</summary>
    public static TextInputBase Small(this TextInputBase widget) => widget.Size(Sizes.Small);

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

/// <summary> Provides extension methods for adding validation to text inputs in ViewBase context. </summary>
public static class TextInputValidationExtensions
{
    /// <summary>
    /// Adds automatic email validation to an email input created from a state when used in a ViewBase context.
    /// The validation will trigger on blur and set the Invalid property when validation fails.
    /// </summary>
    /// <param name="state">The state object the input is bound to.</param>
    /// <param name="view">The ViewBase context.</param>
    /// <param name="input">The email input to add validation to.</param>
    /// <returns>The input with validation attached.</returns>
    public static TextInputBase WithEmailValidation(this IAnyState state, ViewBase view, TextInputBase input)
    {
        if (input.Variant != TextInputs.Email)
            return input;

        return view.AddTextInputValidation(state, input, Validators.CreateEmailValidator("Email"),
            "Please enter a valid email address");
    }

    /// <summary>
    /// Adds automatic password validation to a password input created from a state when used in a ViewBase context.
    /// The validation will trigger on blur and set the Invalid property when validation fails.
    /// </summary>
    /// <param name="state">The state object the input is bound to.</param>
    /// <param name="view">The ViewBase context.</param>
    /// <param name="input">The password input to add validation to.</param>
    /// <returns>The input with validation attached.</returns>
    public static TextInputBase WithPasswordValidation(this IAnyState state, ViewBase view, TextInputBase input)
    {
        if (input.Variant != TextInputs.Password)
            return input;

        // Basic password validation: at least 8 characters
        Func<object?, (bool, string)> passwordValidator = value =>
        {
            if (value is not string passwordStr || string.IsNullOrWhiteSpace(passwordStr))
                return (true, ""); // Empty is handled by Required validator

            if (passwordStr.Length < 8)
                return (false, "Password must be at least 8 characters long");

            return (true, "");
        };

        return view.AddTextInputValidation(state, input, passwordValidator,
            "Password must be at least 8 characters long");
    }

    /// <summary>
    /// Internal helper method to add validation to a text input with blur-based validation strategy.
    /// </summary>
    private static TextInputBase AddTextInputValidation(this ViewBase view, IAnyState state, TextInputBase input,
        Func<object?, (bool, string)> validator, string defaultErrorMessage)
    {
        var invalidState = view.Context.UseState((string?)null!);
        var blurOnceState = view.Context.UseState(false);

        // Validate on value change (after first blur)
        view.Context.UseEffect(() =>
        {
            if (blurOnceState.Value)
            {
                var currentValue = state.As<object>().Value;
                var (isValid, message) = validator(currentValue);
                invalidState.Set(isValid ? null! : message);
            }
        }, state, blurOnceState);

        // Validate on blur
        void OnBlur(Event<IAnyInput> _)
        {
            blurOnceState.Set(true);
            var currentValue = state.As<object>().Value;
            var (isValid, message) = validator(currentValue);
            invalidState.Set(isValid ? null! : message);
        }

        return input.HandleBlur(OnBlur).Invalid(invalidState.Value);
    }
}