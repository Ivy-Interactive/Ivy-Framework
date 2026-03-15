using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Internal: set during view Build() so ToEmailInput() etc. can validate on blur without duplicating methods.</summary>
internal static class TextInputBuildContext
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

public enum TextInputVariant
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
    public TextInputVariant Variant { get; set; }
}

public abstract record TextInputBase : WidgetBase<TextInputBase>, IAnyTextInput
{
    [Prop] public bool Disabled { get; set; }

    [Prop] public string? Invalid { get; set; }

    [Prop] public string? Placeholder { get; set; }

    [Prop] public TextInputVariant Variant { get; set; } = TextInputVariant.Text;

    [Prop] public string? ShortcutKey { get; set; }

    [Prop] public Affix? Prefix { get; set; }

    [Prop] public Affix? Suffix { get; set; }

    [Prop] public int? MaxLength { get; set; }

    [Prop] public int? MinLength { get; set; }

    [Prop] public int? Rows { get; set; }

    [Prop] public bool Nullable { get; set; }

    [Event] public EventHandler<Event<IAnyInput>>? OnBlur { get; set; }

    [Event] public EventHandler<Event<IAnyInput>>? OnSubmit { get; set; }

    public Type[] SupportedStateTypes() => [];
}

public record TextInput<TString> : TextInputBase, IInput<TString>
{
    public TextInput(IAnyState state, string? placeholder = null, bool disabled = false, TextInputVariant variant = TextInputVariant.Text)
        : this(placeholder, disabled, variant)
    {
        var typedState = state.As<TString>();
        Value = typedState.Value;
        OnChange = new(e => { typedState.Set(e.Value); return ValueTask.CompletedTask; });
        // So .Variant(Email) etc. can attach the same blur validation as ToTextInput (no API change).
        if (TextInputBuildContext.GetCurrent() != null)
            SetAttachedValue(TextInputExtensions.ValidationOwner, TextInputExtensions.AttachedValidationState, state);
    }

    [OverloadResolutionPriority(1)]
    public TextInput(TString value, Func<Event<IInput<TString>, TString>, ValueTask>? onChange = null, string? placeholder = null, bool disabled = false, TextInputVariant variant = TextInputVariant.Text)
        : this(placeholder, disabled, variant)
    {
        OnChange = onChange.ToEventHandler();
        Value = value;
    }

    public TextInput(TString value, Action<Event<IInput<TString>, TString>>? onChange = null, string? placeholder = null, bool disabled = false, TextInputVariant variant = TextInputVariant.Text)
        : this(placeholder, disabled, variant)
    {
        OnChange = onChange.ToEventHandler();
        Value = value;
    }

    public TextInput(string? placeholder = null, bool disabled = false, TextInputVariant variant = TextInputVariant.Text)
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
    public TextInput(IAnyState state, string? placeholder = null, bool disabled = false, TextInputVariant variant = TextInputVariant.Text)
        : base(state, placeholder, disabled, variant)
    {
    }

    [OverloadResolutionPriority(1)]
    public TextInput(string value, Func<Event<IInput<string>, string>, ValueTask>? onChange = null, string? placeholder = null, bool disabled = false, TextInputVariant variant = TextInputVariant.Text)
        : base(value, onChange, placeholder, disabled, variant)
    {
    }

    public TextInput(string value, Action<Event<IInput<string>, string>>? onChange = null, string? placeholder = null, bool disabled = false, TextInputVariant variant = TextInputVariant.Text)
        : base(value, onChange?.ToValueTask(), placeholder, disabled, variant)
    {
    }

    public TextInput(string? placeholder = null, bool disabled = false, TextInputVariant variant = TextInputVariant.Text)
        : base(placeholder, disabled, variant)
    {
    }
}

public static class TextInputExtensions
{
    internal static readonly Type ValidationOwner = typeof(TextInputExtensions);
    internal const string AttachedValidationState = "ValidationState";
    internal const string AttachedValidatedVariant = "ValidatedVariant";

    private static bool VariantHasBuiltInValidation(TextInputVariant variant) =>
        variant is TextInputVariant.Email or TextInputVariant.Tel or TextInputVariant.Url or TextInputVariant.Password;

    /// <summary>Creates the text input without blur-validation wiring (used to avoid recursion when wrapping).</summary>
    private static TextInputBase CreateTextInputCore(IAnyState state, string? placeholder, bool disabled, TextInputVariant variant)
    {
        var type = state.GetStateType();
        Type genericType = typeof(TextInput<>).MakeGenericType(type);
        TextInputBase input = (TextInputBase)Activator.CreateInstance(genericType, state, placeholder, disabled, variant)!;
        var nullableProperty = genericType.GetProperty("Nullable", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        nullableProperty?.SetValue(input, type.IsNullableType());
        return input;
    }

    /// <summary>Wire blur validation for variant; <paramref name="widget"/> must already be bound to <paramref name="state"/>.</summary>
    private static TextInputBase ApplyVariantValidation(IViewContext context, IAnyState state, TextInputBase widget, TextInputVariant variant)
    {
        if (!VariantHasBuiltInValidation(variant))
            return widget;

        var invalidState = context.UseState(default(string?), true);
        var blurOnceState = context.UseState(false, true);
        context.UseEffect(() =>
        {
            if (!blurOnceState.Value) return;
            var (_, err) = Validators.ValidateForVariant(state.As<object>().Value, variant);
            invalidState.Set(string.IsNullOrEmpty(err) ? "" : err);
        }, state, blurOnceState);

        void SetBlurFlag(Event<IAnyInput> _) => blurOnceState.Set(true);
        var withInvalid = widget.Invalid(invalidState.Value ?? "");
        if (withInvalid.OnBlur != null)
        {
            var inner = withInvalid.OnBlur;
            withInvalid = withInvalid.OnBlur(async e =>
            {
                await inner.Invoke(e);
                SetBlurFlag(e);
            });
        }
        else
        {
            withInvalid = withInvalid.OnBlur(SetBlurFlag);
        }

        withInvalid.SetAttachedValue(ValidationOwner, AttachedValidatedVariant, variant);
        return withInvalid;
    }

    public static TextInputBase ToTextInput(this IAnyState state, string? placeholder = null, bool disabled = false, TextInputVariant variant = TextInputVariant.Text)
    {
        var input = CreateTextInputCore(state, placeholder, disabled, variant);
        // Allow .Variant(Email) later to pick up the same state for validation
        if (TextInputBuildContext.GetCurrent() is { } ctx)
            input.SetAttachedValue(ValidationOwner, AttachedValidationState, state);

        if (VariantHasBuiltInValidation(variant) && TextInputBuildContext.GetCurrent() is { } ctx2)
            return ApplyVariantValidation(ctx2, state, input, variant);

        return input;
    }

    public static TextInputBase ToTextareaInput(this IAnyState state, string? placeholder = null, bool disabled = false) => state.ToTextInput(placeholder, disabled, TextInputVariant.Textarea);

    public static TextInputBase ToSearchInput(this IAnyState state, string? placeholder = null, bool disabled = false) => state.ToTextInput(placeholder, disabled, TextInputVariant.Search);

    /// <summary>Email/Password/Url/Tel: validates on blur when built inside a view (context set). Same as ToTextInput(..., variant).</summary>
    public static TextInputBase ToEmailInput(this IAnyState state, string? placeholder = null, bool disabled = false) =>
        state.ToTextInput(placeholder, disabled, TextInputVariant.Email);

    public static TextInputBase ToPasswordInput(this IAnyState state, string? placeholder = null, bool disabled = false) =>
        state.ToTextInput(placeholder, disabled, TextInputVariant.Password);

    public static TextInputBase ToUrlInput(this IAnyState state, string? placeholder = null, bool disabled = false) =>
        state.ToTextInput(placeholder, disabled, TextInputVariant.Url);

    public static TextInputBase ToTelInput(this IAnyState state, string? placeholder = null, bool disabled = false) =>
        state.ToTextInput(placeholder, disabled, TextInputVariant.Tel);

    public static TextInputBase Placeholder(this TextInputBase widget, string placeholder) => widget with { Placeholder = placeholder };

    public static TextInputBase Disabled(this TextInputBase widget, bool disabled = true) => widget with { Disabled = disabled };

    public static TextInputBase Variant(this TextInputBase widget, TextInputVariant variant)
    {
        var w = widget with { Variant = variant };
        if (!VariantHasBuiltInValidation(variant))
            return w;
        if (TextInputBuildContext.GetCurrent() is not { } ctx)
            return w;
        if (w.GetAttachedValue(ValidationOwner, AttachedValidatedVariant) is TextInputVariant v && v == variant)
            return w;
        if (w.GetAttachedValue(ValidationOwner, AttachedValidationState) is not IAnyState state)
            return w;
        return ApplyVariantValidation(ctx, state, w, variant);
    }

    public static TextInputBase Multiline(this TextInputBase widget, bool multiline = true)
        => widget with { Variant = multiline ? TextInputVariant.Textarea : TextInputVariant.Text };

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


    [OverloadResolutionPriority(1)]
    public static TextInputBase OnSubmit(this TextInputBase widget, Func<Event<IAnyInput>, ValueTask> onSubmit)
    {
        return widget with { OnSubmit = onSubmit };
    }

    public static TextInputBase OnSubmit(this TextInputBase widget, Action<Event<IAnyInput>> onSubmit)
    {
        return widget.OnSubmit(onSubmit.ToValueTask());
    }

    public static TextInputBase OnSubmit(this TextInputBase widget, Action onSubmit)
    {
        return widget.OnSubmit(_ => { onSubmit(); return ValueTask.CompletedTask; });
    }
}