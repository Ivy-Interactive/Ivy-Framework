using System.Net.Mail;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;
using Ivy.Shared;
using Ivy.Widgets;
using Ivy.Widgets.Inputs;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Used by the framework to set current build context so state.ToEmailInput() etc. validate on blur without duplicate overloads.</summary>
public static class TextInputBuildContext
{
    private static readonly AsyncLocal<IViewContext?> Current = new();
    public static void SetCurrent(IViewContext? context) => Current.Value = context;
    internal static IViewContext? GetCurrent() => Current.Value;
}

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

    [Event] public Func<Event<IAnyInput>, ValueTask>? OnBlur { get; set; }

    public Type[] SupportedStateTypes() => [];
}

public record TextInput<TString> : TextInputBase, IInput<TString>
{
    public TextInput(IAnyState state, string? placeholder = null, bool disabled = false, TextInputVariants variant = TextInputVariants.Text)
        : this(placeholder, disabled, variant)
    {
        var typedState = state.As<TString>();
        Value = typedState.Value;
        OnChange = e => { typedState.Set(e.Value); return ValueTask.CompletedTask; };
    }

    [OverloadResolutionPriority(1)]
    public TextInput(TString value, Func<Event<IInput<TString>, TString>, ValueTask>? onChange = null, string? placeholder = null, bool disabled = false, TextInputVariants variant = TextInputVariants.Text)
        : this(placeholder, disabled, variant)
    {
        OnChange = onChange;
        Value = value;
    }

    public TextInput(TString value, Action<Event<IInput<TString>, TString>>? onChange = null, string? placeholder = null, bool disabled = false, TextInputVariants variant = TextInputVariants.Text)
        : this(placeholder, disabled, variant)
    {
        OnChange = onChange?.ToValueTask();
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

    [Event] public Func<Event<IInput<TString>, TString>, ValueTask>? OnChange { get; }
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

    public static TextInputBase ToTextAreaInput(this IAnyState state, string? placeholder = null, bool disabled = false) => state.ToTextInput(placeholder, disabled, TextInputVariants.Textarea);

    public static TextInputBase ToSearchInput(this IAnyState state, string? placeholder = null, bool disabled = false) => state.ToTextInput(placeholder, disabled, TextInputVariants.Search);

    /// <summary>Email/Password/Url/Tel input. Validates on blur when built inside a view; in forms FormView also runs validation.</summary>
    public static TextInputBase ToEmailInput(this IAnyState state, string? placeholder = null, bool disabled = false) =>
        TextInputBuildContext.GetCurrent() is { } ctx ? BuildValidatedInput(state, TextInputVariants.Email, ctx, placeholder, disabled) : state.ToTextInput(placeholder, disabled, TextInputVariants.Email);
    public static TextInputBase ToPasswordInput(this IAnyState state, string? placeholder = null, bool disabled = false) =>
        TextInputBuildContext.GetCurrent() is { } ctxP ? BuildValidatedInput(state, TextInputVariants.Password, ctxP, placeholder, disabled) : state.ToTextInput(placeholder, disabled, TextInputVariants.Password);
    public static TextInputBase ToUrlInput(this IAnyState state, string? placeholder = null, bool disabled = false) =>
        TextInputBuildContext.GetCurrent() is { } ctxU ? BuildValidatedInput(state, TextInputVariants.Url, ctxU, placeholder, disabled) : state.ToTextInput(placeholder, disabled, TextInputVariants.Url);
    public static TextInputBase ToTelInput(this IAnyState state, string? placeholder = null, bool disabled = false) =>
        TextInputBuildContext.GetCurrent() is { } ctxT ? BuildValidatedInput(state, TextInputVariants.Tel, ctxT, placeholder, disabled) : state.ToTextInput(placeholder, disabled, TextInputVariants.Tel);

    private static TextInputBase BuildValidatedInput(IAnyState state, TextInputVariants variant, IViewContext context, string? placeholder, bool disabled)
    {
        var invalidState = context.UseState(default(string?), true);
        var blurOnceState = context.UseState(false, true);
        context.UseEffect(() =>
        {
            if (blurOnceState.Value)
                invalidState.Set((variant switch
                {
                    TextInputVariants.Email => ValidateEmail(state.As<object>().Value),
                    TextInputVariants.Password => ValidatePassword(state.As<object>().Value),
                    TextInputVariants.Url => ValidateUrl(state.As<object>().Value),
                    TextInputVariants.Tel => ValidateTel(state.As<object>().Value),
                    _ => null
                }) ?? "");
        }, state, blurOnceState);
        void OnBlur(Event<IAnyInput> _) => blurOnceState.Set(true);
        return state.ToTextInput(placeholder, disabled, variant).Invalid(invalidState.Value ?? "").HandleBlur(OnBlur);
    }

    /// <summary>Returns (true, null) if valid, (false, errorMessage) if invalid. Used by Validators and form validation.</summary>
    public static (bool isValid, string? errorMessage) ValidateForVariant(object? value, TextInputVariants variant)
    {
        if (value is not string s || string.IsNullOrWhiteSpace(s))
            return (true, null);
        var err = variant switch
        {
            TextInputVariants.Email => ValidateEmail(value),
            TextInputVariants.Password => ValidatePassword(value),
            TextInputVariants.Tel => ValidateTel(value),
            TextInputVariants.Url => ValidateUrl(value),
            _ => null
        };
        return (err == null, err);
    }

    private static string? ValidateEmail(object? value)
    {
        if (value is not string s || string.IsNullOrWhiteSpace(s)) return null;
        try
        {
            var addr = new MailAddress(s);
            return addr.Host.Contains('.') ? null : "Please enter a valid email address";
        }
        catch (FormatException) { return "Please enter a valid email address"; }
    }

    private static string? ValidatePassword(object? value, int minLength = 8)
    {
        if (value is not string s || string.IsNullOrWhiteSpace(s)) return null;
        return s.Length >= minLength ? null : $"Password must be at least {minLength} characters";
    }

    private static string? ValidateTel(object? value)
    {
        if (value is not string s || string.IsNullOrWhiteSpace(s)) return null;
        var digitsOnly = Regex.Replace(s, @"\D", "");
        if (digitsOnly.Length < 7 || digitsOnly.Length > 15) return "Please enter a valid phone number";
        return Regex.IsMatch(s, @"^[\d\s+\-().]+$") ? null : "Please enter a valid phone number";
    }

    private static string? ValidateUrl(object? value)
    {
        if (value is not string s || string.IsNullOrWhiteSpace(s)) return null;
        if (!Uri.TryCreate(s, UriKind.Absolute, out var uri) || !uri.IsAbsoluteUri) return "Please enter a valid URL";
        return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps ? null : "Please enter a valid URL (http or https)";
    }

    public static TextInputBase Placeholder(this TextInputBase widget, string placeholder) => widget with { Placeholder = placeholder };

    public static TextInputBase Disabled(this TextInputBase widget, bool disabled = true) => widget with { Disabled = disabled };

    public static TextInputBase Variant(this TextInputBase widget, TextInputVariants variant) => widget with { Variant = variant };

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
    public static TextInputBase HandleBlur(this TextInputBase widget, Func<Event<IAnyInput>, ValueTask> onBlur)
    {
        return widget with { OnBlur = onBlur };
    }

    public static TextInputBase HandleBlur(this TextInputBase widget, Action<Event<IAnyInput>> onBlur)
    {
        return widget.HandleBlur(onBlur.ToValueTask());
    }

    public static TextInputBase HandleBlur(this TextInputBase widget, Action onBlur)
    {
        return widget.HandleBlur(_ => { onBlur(); return ValueTask.CompletedTask; });
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