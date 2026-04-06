// ReSharper disable once CheckNamespace
namespace Ivy;

public record FolderInput : WidgetBase<FolderInput>, IAnyInput
{
    internal FolderInput() { }

    [Prop] public string? Value { get; set; }
    [Prop] public bool Disabled { get; set; }
    [Prop] public string? Invalid { get; set; }
    [Prop] public string? Placeholder { get; set; }
    [Prop] public bool Nullable { get; set; } = true;
    [Prop] public bool AutoFocus { get; set; }

    [Event] public EventHandler<Event<IAnyInput>>? OnBlur { get; set; }
    [Event] public EventHandler<Event<IAnyInput>>? OnFocus { get; set; }
    [Event] public EventHandler<Event<FolderInput, string?>>? OnChange { get; set; }

    public Type[] SupportedStateTypes() => [typeof(string)];
    public ValidationResult ValidateValue(object? value) => ValidationResult.Success();
}

public static class FolderInputExtensions
{
    public static FolderInput ToFolderInput(this IState<string?> state, string? placeholder = null, bool disabled = false)
    {
        return new FolderInput
        {
            Value = state.Value,
            Placeholder = placeholder ?? "Select a folder...",
            Disabled = disabled,
            OnChange = new(e => { state.Set(e.Value); return ValueTask.CompletedTask; })
        };
    }

    public static FolderInput Placeholder(this FolderInput widget, string placeholder)
        => widget with { Placeholder = placeholder };

    public static FolderInput Disabled(this FolderInput widget, bool disabled = true)
        => widget with { Disabled = disabled };

    public static FolderInput Invalid(this FolderInput widget, string? invalid)
        => widget with { Invalid = invalid };
}
