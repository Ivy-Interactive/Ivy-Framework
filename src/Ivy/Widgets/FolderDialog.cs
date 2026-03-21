// ReSharper disable once CheckNamespace
namespace Ivy;

public record FolderDialogEntry
{
    public string Name { get; init; } = "";
    public string Kind { get; init; } = ""; // "file" or "directory"
    public string RelativePath { get; init; } = "";
}

public record FolderDialog : WidgetBase<FolderDialog>
{
    internal FolderDialog() { }

    [Prop] public int TriggerCount { get; set; }

    [Event] public EventHandler<Event<FolderDialog>>? OnCancel { get; set; }
    [Event] public EventHandler<Event<FolderDialog, FolderDialogEntry[]>>? OnFolderSelected { get; set; }
}
