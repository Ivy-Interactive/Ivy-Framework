// ReSharper disable once CheckNamespace
namespace Ivy;

public enum FileDialogMode
{
    Upload,
    PathOnly
}

public record FileDialogFileInfo
{
    public string FileName { get; init; } = "";
    public string ContentType { get; init; } = "";
    public long Size { get; init; }
}

public record FileDialog : WidgetBase<FileDialog>
{
    internal FileDialog() { }

    [Prop] public int TriggerCount { get; set; }
    [Prop] public string? Accept { get; set; }
    [Prop] public bool Multiple { get; set; }
    [Prop] public long? MaxFileSize { get; set; }
    [Prop] public long? MinFileSize { get; set; }
    [Prop] public FileDialogMode Mode { get; set; } = FileDialogMode.Upload;
    [Prop] public string? UploadUrl { get; set; }

    [Event] public EventHandler<Event<FileDialog>>? OnCancel { get; set; }
    [Event] public EventHandler<Event<FileDialog, FileDialogFileInfo[]>>? OnFilesSelected { get; set; }
}
