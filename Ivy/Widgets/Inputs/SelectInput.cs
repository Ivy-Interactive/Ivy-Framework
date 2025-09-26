using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Ivy.Core;
using Ivy.Core.Docs;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;
using Ivy.Shared;
using Ivy.Widgets.Inputs;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Defines the visual variants available for select input controls.
/// </summary>
public enum SelectInputs
{
    /// <summary>Standard dropdown select control with a collapsible option list.</summary>
    Select,
    /// <summary>List-style selection with all options visible simultaneously.</summary>
    List,
    /// <summary>Toggle-style selection for boolean or small option sets.</summary>
    Toggle
}

/// <summary>
/// Interface for select input controls that extends IAnyInput with selection-specific properties.
/// </summary>
public interface IAnySelectInput : IAnyInput
{
    /// <summary>Gets or sets the placeholder text displayed when no option is selected.</summary>
    public string? Placeholder { get; set; }

    /// <summary>Gets or sets the visual variant of the select input.</summary>
    public SelectInputs Variant { get; set; }
}

/// <summary>
/// Abstract base class for select input controls that provides common selection functionality.
/// </summary>
public abstract record SelectInputBase : WidgetBase<SelectInputBase>, IAnySelectInput
{
    /// <summary>Gets or sets whether the input is disabled.</summary>
    [Prop] public bool Disabled { get; set; }

    /// <summary>Gets or sets the validation error message.</summary>
    [Prop] public string? Invalid { get; set; }

    /// <summary>Gets or sets the placeholder text displayed when no option is selected.</summary>
    [Prop] public string? Placeholder { get; set; }

    /// <summary>Gets or sets the visual variant of the select input.</summary>
    [Prop] public SelectInputs Variant { get; set; }

    /// <summary>Gets or sets the size of the select input.</summary>
    [Prop] public Sizes Size { get; set; }

    /// <summary>Gets or sets whether multiple options can be selected simultaneously.</summary>
    [Prop] public bool SelectMany { get; set; } = false;

    /// <summary>Gets or sets the character used to separate multiple selected values in display and serialization.</summary>
    [Prop] public char Separator { get; set; } = ';';

    /// <summary>Gets or sets the event handler called when the input loses focus.</summary>
    [Event] public Func<Event<IAnyInput>, ValueTask>? OnBlur { get; set; }

    /// <summary>
    /// Returns the types that this select input can bind to and work with.
    /// </summary>
    public Type[] SupportedStateTypes() => [];
}

/// <summary>
/// Generic select input control that provides type-safe option selection functionality.
/// </summary>
/// <typeparam name="TValue">The type of the selected value(s) - can be single values or collections for multi-select.</typeparam>
public record SelectInput<TValue> : SelectInputBase, IInput<TValue>, IAnySelectInput
{
    /// <summary>
    /// Initializes a new select input bound to a state object for automatic value synchronization.
    /// </summary>
    /// <param name="state">The state object to bind the select input to.</param>
    /// <param name="options">The collection of options available for selection.</param>
    /// <param name="placeholder">Optional placeholder text displayed when no option is selected.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <param name="variant">The visual variant of the select input.</param>
    /// <param name="selectMany">Whether multiple options can be selected simultaneously.</param>
    [OverloadResolutionPriority(1)]
    public SelectInput(IAnyState state, IEnumerable<IAnyOption> options, string? placeholder = null, bool disabled = false, SelectInputs variant = SelectInputs.Select, bool selectMany = false)
        : this(options, placeholder, disabled, variant, selectMany)
    {
        var typedState = state.As<TValue>();
        Value = typedState.Value;
        OnChange = e => { typedState.Set(e.Value); return ValueTask.CompletedTask; };
    }

    /// <summary>
    /// Initializes a new select input with an explicit value and async change handler.
    /// Useful for manual state management or when custom change handling is required.
    /// </summary>
    /// <param name="value">The initial selected value.</param>
    /// <param name="onChange">The async event handler called when the selection changes.</param>
    /// <param name="options">The collection of options available for selection.</param>
    /// <param name="placeholder">Optional placeholder text displayed when no option is selected.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <param name="variant">The visual variant of the select input.</param>
    /// <param name="selectMany">Whether multiple options can be selected simultaneously.</param>
    [OverloadResolutionPriority(1)]
    public SelectInput(TValue value, Func<Event<IInput<TValue>, TValue>, ValueTask>? onChange, IEnumerable<IAnyOption> options, string? placeholder = null, bool disabled = false, SelectInputs variant = SelectInputs.Select, bool selectMany = false)
        : this(options, placeholder, disabled, variant, selectMany)
    {
        OnChange = onChange;
        Value = value;
    }

    /// <summary>
    /// Initializes a new select input with an explicit value and synchronous change handler.
    /// Compatibility overload for Action-based change handlers.
    /// </summary>
    /// <param name="value">The initial selected value.</param>
    /// <param name="onChange">The synchronous event handler called when the selection changes.</param>
    /// <param name="options">The collection of options available for selection.</param>
    /// <param name="placeholder">Optional placeholder text displayed when no option is selected.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <param name="variant">The visual variant of the select input.</param>
    /// <param name="selectMany">Whether multiple options can be selected simultaneously.</param>
    public SelectInput(TValue value, Action<Event<IInput<TValue>, TValue>>? onChange, IEnumerable<IAnyOption> options, string? placeholder = null, bool disabled = false, SelectInputs variant = SelectInputs.Select, bool selectMany = false)
        : this(options, placeholder, disabled, variant, selectMany)
    {
        OnChange = onChange == null ? null : e => { onChange(e); return ValueTask.CompletedTask; };
        Value = value;
    }

    /// <summary>
    /// Initializes a new select input with basic configuration.
    /// Requires separate value and change handler assignment for functionality.
    /// </summary>
    /// <param name="options">The collection of options available for selection.</param>
    /// <param name="placeholder">Optional placeholder text displayed when no option is selected.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <param name="variant">The visual variant of the select input.</param>
    /// <param name="selectMany">Whether multiple options can be selected simultaneously.</param>
    public SelectInput(IEnumerable<IAnyOption> options, string? placeholder = null, bool disabled = false, SelectInputs variant = SelectInputs.Select, bool selectMany = false)
    {
        Placeholder = placeholder;
        Variant = variant;
        Disabled = disabled;
        Options = [.. options];
        SelectMany = selectMany;
    }

    /// <summary>Gets the currently selected value(s).</summary>
    [Prop] public TValue Value { get; } = default!;

    /// <summary>Gets or sets whether the input accepts null values.</summary>
    [Prop] public bool Nullable { get; set; } = typeof(TValue).IsNullableType();

    /// <summary>Gets or sets the collection of options available for selection.</summary>
    [Prop] public IAnyOption[] Options { get; set; }

    /// <summary>Gets the event handler called when the selection changes.</summary>
    [Event] public Func<Event<IInput<TValue>, TValue>, ValueTask>? OnChange { get; }
}

/// <summary>
/// Provides extension methods for creating and configuring select input controls with fluent syntax.
/// </summary>
public static class SelectInputExtensions
{
    /// <summary>
    /// Creates a select input from a state object with automatic type detection and intelligent defaults.
    /// </summary>
    /// <param name="state">The state object to bind to.</param>
    /// <param name="options">Optional collection of options; if null, attempts automatic generation for enums and collections.</param>
    /// <param name="placeholder">Optional placeholder text displayed when no option is selected.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <param name="variant">The visual variant of the select input.</param>
    /// <returns>A select input bound to the state object with type-appropriate configuration.</returns>
    /// <exception cref="ArgumentException">Thrown when options are null and cannot be automatically generated for the state type.</exception>
    public static SelectInputBase ToSelectInput(this IAnyState state, IEnumerable<IAnyOption>? options = null, string? placeholder = null, bool disabled = false, SelectInputs variant = SelectInputs.Select)
    {
        var type = state.GetStateType();
        bool selectMany = type.IsCollectionType();
        Type genericType = typeof(SelectInput<>).MakeGenericType(type);

        if (options == null)
        {
            var nonNullableType = Nullable.GetUnderlyingType(type) ?? type;
            if (nonNullableType.IsEnum)
            {
                options = nonNullableType.ToOptions();
            }
            else if (selectMany && type.GetCollectionTypeParameter() is { } itemType)
            {
                options = itemType.ToOptions();
            }
            else
            {
                throw new ArgumentException("Options must be provided for non-enum types.", nameof(options));
            }
        }

        // Set a default placeholder for multi-selects if not provided
        if (selectMany && string.IsNullOrWhiteSpace(placeholder))
        {
            placeholder = "Select options...";
        }

        SelectInputBase input = (SelectInputBase)Activator.CreateInstance(genericType, state, options, placeholder, disabled, variant, selectMany)!;
        return input;
    }

    /// <summary>Sets the placeholder text for the select input.</summary>
    /// <param name="widget">The select input to configure.</param>
    /// <param name="title">The placeholder text to display when no option is selected.</param>
    public static SelectInputBase Placeholder(this SelectInputBase widget, string title)
    {
        return widget with { Placeholder = title };
    }

    /// <summary>Sets the disabled state of the select input.</summary>
    /// <param name="widget">The select input to configure.</param>
    /// <param name="disabled">Whether the input should be disabled.</param>
    public static SelectInputBase Disabled(this SelectInputBase widget, bool disabled = true)
    {
        return widget with { Disabled = disabled };
    }

    /// <summary>Sets the visual variant of the select input.</summary>
    /// <param name="widget">The select input to configure.</param>
    /// <param name="variant">The visual variant (Select, List, or Toggle).</param>
    public static SelectInputBase Variant(this SelectInputBase widget, SelectInputs variant)
    {
        return widget with { Variant = variant };
    }

    /// <summary>Sets the size of the select input.</summary>
    /// <param name="widget">The select input to configure.</param>
    /// <param name="size">The size of the select input.</param>
    public static SelectInputBase Size(this SelectInputBase widget, Sizes size)
    {
        return widget with { Size = size };
    }

    /// <summary>Sets the select input size to large for prominent display.</summary>
    /// <param name="widget">The select input to configure.</param>
    /// <returns>A new SelectInputBase instance with large size applied.</returns>
    [RelatedTo(nameof(SelectInputBase.Size))]
    public static SelectInputBase Large(this SelectInputBase widget)
    {
        return widget.Size(Sizes.Large);
    }

    /// <summary>Sets the select input size to small for compact display.</summary>
    /// <param name="widget">The select input to configure.</param>
    /// <returns>A new SelectInputBase instance with small size applied.</returns>
    [RelatedTo(nameof(SelectInputBase.Size))]
    public static SelectInputBase Small(this SelectInputBase widget)
    {
        return widget.Size(Sizes.Small);
    }

    /// <summary>Sets the validation error message for the select input.</summary>
    /// <param name="widget">The select input to configure.</param>
    /// <param name="invalid">The validation error message to display, or null to clear the error.</param>
    public static SelectInputBase Invalid(this SelectInputBase widget, string? invalid)
    {
        return widget with { Invalid = invalid };
    }

    /// <summary>Sets the separator character used for multi-select value display and serialization.</summary>
    /// <param name="widget">The select input to configure.</param>
    /// <param name="separator">The character to use for separating multiple selected values.</param>
    public static SelectInputBase Separator(this SelectInputBase widget, char separator)
    {
        return widget with { Separator = separator };
    }

    /// <summary>
    /// Configures the select input to use the List variant for displaying all options simultaneously.
    /// Convenience method that sets the variant to SelectInputs.List.
    /// </summary>
    /// <param name="widget">The select input to configure.</param>
    public static SelectInputBase List(this SelectInputBase widget)
    {
        return widget with { Variant = SelectInputs.List };
    }


    /// <summary>
    /// Sets the blur event handler for the select input.
    /// This method allows you to configure the select input's blur behavior,
    /// enabling it to perform custom actions when the input loses focus.
    /// </summary>
    /// <param name="widget">The select input to configure.</param>
    /// <param name="onBlur">The event handler to call when the input loses focus.</param>
    [OverloadResolutionPriority(1)]
    public static SelectInputBase HandleBlur(this SelectInputBase widget, Func<Event<IAnyInput>, ValueTask> onBlur)
    {
        return widget with { OnBlur = onBlur };
    }

    /// <summary>
    /// Sets the blur event handler for the select input.
    /// Compatibility overload for Action-based event handlers.
    /// </summary>
    /// <param name="widget">The select input to configure.</param>
    /// <param name="onBlur">The event handler to call when the input loses focus.</param>
    public static SelectInputBase HandleBlur(this SelectInputBase widget, Action<Event<IAnyInput>> onBlur)
    {
        return widget.HandleBlur(onBlur.ToValueTask());
    }

    /// <summary>
    /// Sets a simple blur event handler for the select input.
    /// This method allows you to configure the select input's blur behavior with
    /// a simple action that doesn't require the input event context.
    /// </summary>
    /// <param name="widget">The select input to configure.</param>
    /// <param name="onBlur">The simple action to perform when the input loses focus.</param>
    public static SelectInputBase HandleBlur(this SelectInputBase widget, Action onBlur)
    {
        return widget.HandleBlur(_ => { onBlur(); return ValueTask.CompletedTask; });
    }
}