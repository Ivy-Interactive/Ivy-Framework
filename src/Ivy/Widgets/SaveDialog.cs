// ReSharper disable once CheckNamespace
namespace Ivy;

public record SaveDialogResult
{
    public bool Success { get; init; }
    public string FileName { get; init; } = "";
}

public record SaveDialog : WidgetBase<SaveDialog>
{
    internal SaveDialog() { }

    [Prop] public int TriggerCount { get; set; }
    [Prop] public string? SuggestedName { get; set; }
    [Prop] public string? Accept { get; set; }
    [Prop] public string? DownloadUrl { get; set; }

    [Event] public EventHandler<Event<SaveDialog>>? OnCancel { get; set; }
    [Event] public EventHandler<Event<SaveDialog, SaveDialogResult>>? OnSaved { get; set; }
}
