using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Reflection;
using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// An input field for selecting an icon from the Ivy icon set (Lucide icons).
/// </summary>
public abstract record IconInputBase : WidgetBase<IconInputBase>, IAnyInput
{
    [Prop] public bool Disabled { get; set; }

    [Prop] public string? Invalid { get; set; }

    [Prop] public string? Placeholder { get; set; }

    [Prop] public bool Nullable { get; set; }

    [Event] public EventHandler<Event<IAnyInput>>? OnBlur { get; set; }

    public Type[] SupportedStateTypes() => [typeof(Icons), typeof(Icons?)];
}

public record IconInput<TIcon> : IconInputBase, IInput<TIcon>
{
    [OverloadResolutionPriority(1)]
    internal IconInput(IAnyState state, string? placeholder = null, bool disabled = false)
        : this(placeholder, disabled)
    {
        var typedState = state.As<TIcon>();
        var value = typedState.Value;
        Value = value;
        OnChange = new(e =>
        {
            typedState.Set(e.Value);
            return ValueTask.CompletedTask;
        });
    }

    [OverloadResolutionPriority(1)]
    internal IconInput(TIcon value, Func<Event<IInput<TIcon>, TIcon>, ValueTask> onChange, string? placeholder = null, bool disabled = false)
        : this(placeholder, disabled)
    {
        OnChange = onChange.ToEventHandler();
        Value = value;
    }

    internal IconInput(TIcon value, Action<Event<IInput<TIcon>, TIcon>> onChange, string? placeholder = null, bool disabled = false)
        : this(placeholder, disabled)
    {
        OnChange = new(e =>
        {
            onChange(e);
            return ValueTask.CompletedTask;
        });
        Value = value;
    }

    internal IconInput(string? placeholder = null, bool disabled = false)
    {
        Disabled = disabled;
        Placeholder = placeholder ?? "Select an icon";
    }

    internal IconInput() { }

    [Prop(AlwaysSerialize = true)] public TIcon Value { get; init; } = default!;

    [Event] public EventHandler<Event<IInput<TIcon>, TIcon>>? OnChange { get; }
}

public static class IconInputExtensions
{
    public static IconInputBase ToIconInput(this IAnyState state, string? placeholder = null, bool disabled = false)
    {
        var type = state.GetStateType();
        Type genericType = typeof(IconInput<>).MakeGenericType(type);
        IconInputBase input = (IconInputBase)Activator.CreateInstance(genericType, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new object?[] { state, placeholder ?? "Select an icon", disabled }, null)!;
        var nullableProperty = genericType.GetProperty("Nullable", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        nullableProperty?.SetValue(input, type.IsNullableType());
        return input;
    }

    public static IconInputBase Placeholder(this IconInputBase widget, string placeholder) =>
        widget with { Placeholder = placeholder };

    public static IconInputBase Disabled(this IconInputBase widget, bool disabled = true) =>
        widget with { Disabled = disabled };

    public static IconInputBase Invalid(this IconInputBase widget, string? invalid) =>
        widget with { Invalid = invalid };

    public static IconInputBase Nullable(this IconInputBase widget, bool? nullable = true) =>
        widget with { Nullable = nullable ?? true };

    [OverloadResolutionPriority(1)]
    public static IconInputBase OnBlur(this IconInputBase widget, Func<Event<IAnyInput>, ValueTask> onBlur) =>
        widget with { OnBlur = new(onBlur) };

    public static IconInputBase OnBlur(this IconInputBase widget, Action<Event<IAnyInput>> onBlur) =>
        widget.OnBlur(onBlur.ToValueTask());

    public static IconInputBase OnBlur(this IconInputBase widget, Action onBlur) =>
        widget.OnBlur(_ =>
        {
            onBlur();
            return ValueTask.CompletedTask;
        });


}
