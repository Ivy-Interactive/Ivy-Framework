using Ivy.Core.Hooks;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Delegate for showing the file dialog. Accepts a callback that receives
/// the selected file metadata when files are chosen. Following the UseAlert
/// pattern, the callback is passed at invocation time, not at hook creation.
/// </summary>
public delegate void ShowFileDialogDelegate(Action<FileDialogFileInfo[]> callback);

public static class UseFileDialogExtensions
{
    /// <summary>
    /// Upload mode — opens the native file dialog and uploads the selected file(s) to the server.
    /// Accepts an IUploadHandler, same as FileInput (e.g., MemoryStreamUploadHandler.Create(state)).
    /// Returns (dialogView, showFileDialog) tuple following the UseAlert pattern.
    /// Call showFileDialog(callback) to open the dialog; the callback receives file metadata after upload.
    /// </summary>
    public static (object? dialogView, ShowFileDialogDelegate showFileDialog) UseFileDialog(
        this IViewContext context,
        IUploadHandler handler,
        string? accept = null,
        bool multiple = false,
        long? maxFileSize = null,
        long? minFileSize = null)
    {
        var uploadService = context.UseService<IUploadService>();
        var triggerCount = context.UseState(0);
        var uploadUrl = context.UseState<string?>();
        var fileDialogCallback = context.UseRef<Action<FileDialogFileInfo[]>?>();

        context.UseEffect(() =>
        {
            var (cleanup, url) = uploadService.AddUpload(
                handler.HandleUploadAsync,
                () => (accept, maxFileSize, minFileSize));
            uploadUrl.Set(url);
            return cleanup;
        }, [EffectTrigger.OnMount()]);

        var dialog = new FileDialog
        {
            TriggerCount = triggerCount.Value,
            Accept = accept,
            Multiple = multiple,
            MaxFileSize = maxFileSize,
            MinFileSize = minFileSize,
            Mode = FileDialogMode.Upload,
            UploadUrl = uploadUrl.Value,
            OnFilesSelected = new(e => { fileDialogCallback.Value?.Invoke(e.Value); return ValueTask.CompletedTask; }),
        };

        var showFileDialog = new ShowFileDialogDelegate(callback =>
        {
            fileDialogCallback.Set(callback);
            triggerCount.Set(triggerCount.Value + 1);
        });

        return (dialog, showFileDialog);
    }

    /// <summary>
    /// PathOnly mode — opens the native file dialog and returns file metadata
    /// (name, content type, size) without uploading. Useful when only the file
    /// reference is needed.
    /// Returns (dialogView, showFileDialog) tuple following the UseAlert pattern.
    /// Call showFileDialog(callback) to open the dialog; the callback receives file metadata.
    /// </summary>
    public static (object? dialogView, ShowFileDialogDelegate showFileDialog) UseFileDialog(
        this IViewContext context,
        string? accept = null,
        bool multiple = false)
    {
        var triggerCount = context.UseState(0);
        var fileDialogCallback = context.UseRef<Action<FileDialogFileInfo[]>?>();

        var dialog = new FileDialog
        {
            TriggerCount = triggerCount.Value,
            Accept = accept,
            Multiple = multiple,
            Mode = FileDialogMode.PathOnly,
            OnFilesSelected = new(e => { fileDialogCallback.Value?.Invoke(e.Value); return ValueTask.CompletedTask; }),
        };

        var showFileDialog = new ShowFileDialogDelegate(callback =>
        {
            fileDialogCallback.Set(callback);
            triggerCount.Set(triggerCount.Value + 1);
        });

        return (dialog, showFileDialog);
    }
}
