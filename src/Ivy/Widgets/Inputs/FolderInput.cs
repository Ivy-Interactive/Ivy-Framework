// ReSharper disable once CheckNamespace
namespace Ivy;

public enum FolderInputMode
{
    Name,     // Default — returns folder name only (web-safe)
    FullPath  // Returns full filesystem path (desktop environments only)
}

public record FolderInput : WidgetBase<FolderInput>, IAnyInput
{
    internal FolderInput() { }

    [Prop] public string? Value { get; set; }
    [Prop] public bool Disabled { get; set; }
    [Prop] public string? Invalid { get; set; }
    [Prop] public string? Placeholder { get; set; }
    [Prop] public bool Nullable { get; set; } = true;
    [Prop] public bool AutoFocus { get; set; }
    [Prop] public FolderInputMode Mode { get; set; } = FolderInputMode.Name;

    [Event] public EventHandler<Event<IAnyInput>>? OnBlur { get; set; }
    [Event] public EventHandler<Event<IAnyInput>>? OnFocus { get; set; }
    [Event] public EventHandler<Event<FolderInput, string?>>? OnChange { get; set; }

    public Type[] SupportedStateTypes() => [typeof(string)];
    public ValidationResult ValidateValue(object? value)
    {
        if (Mode == FolderInputMode.FullPath
            && value is string path
            && !string.IsNullOrEmpty(path)
            && !Directory.Exists(path))
        {
            return ValidationResult.Error("Directory does not exist");
        }

        return ValidationResult.Success();
    }
}

public static class FolderInputExtensions
{
    public static FolderInput ToFolderInput(this IState<string?> state, string? placeholder = null, bool disabled = false, FolderInputMode mode = FolderInputMode.Name)
    {
        return new FolderInput
        {
            Value = state.Value,
            Placeholder = placeholder ?? "Select a folder...",
            Disabled = disabled,
            Mode = mode,
            OnChange = new(e => { state.Set(e.Value); return ValueTask.CompletedTask; })
        };
    }

    public static FolderInput Placeholder(this FolderInput widget, string placeholder)
        => widget with { Placeholder = placeholder };

    public static FolderInput Disabled(this FolderInput widget, bool disabled = true)
        => widget with { Disabled = disabled };

    public static FolderInput Invalid(this FolderInput widget, string? invalid)
        => widget with { Invalid = invalid };

    public static FolderInput Mode(this FolderInput widget, FolderInputMode mode)
        => widget with { Mode = mode };

    public static FolderInput ValidatePath(this FolderInput widget)
    {
        if (widget.Mode == FolderInputMode.FullPath
            && !string.IsNullOrEmpty(widget.Value)
            && !Directory.Exists(widget.Value))
        {
            return widget with { Invalid = "Directory does not exist" };
        }

        return widget;
    }
}
