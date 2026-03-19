using Ivy.Core.Hooks;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Delegate for showing the save dialog. Accepts a callback that receives
/// the save result when the file is saved. Following the UseAlert pattern.
/// </summary>
public delegate void ShowSaveDialogDelegate(Action<SaveDialogResult> callback);

public static class UseSaveDialogExtensions
{
    /// <summary>
    /// Opens the native save dialog. The factory provides the file content when triggered.
    /// On Chromium, uses showSaveFilePicker for true "Save As" UX.
    /// On other browsers, falls back to a programmatic download.
    /// Returns (dialogView, showSaveDialog) tuple following the UseAlert pattern.
    /// Call showSaveDialog(callback) to open the dialog; the callback receives the save result.
    /// </summary>
    public static (object? dialogView, ShowSaveDialogDelegate showSaveDialog) UseSaveDialog(
        this IViewContext context,
        Func<Task<byte[]>> contentFactory,
        string mimeType,
        string suggestedName)
    {
        var downloadService = context.UseService<IDownloadService>();
        var triggerCount = context.UseState(0);
        var downloadUrl = context.UseState<string?>();
        var saveCallback = context.UseRef<Action<SaveDialogResult>?>();

        context.UseEffect(() =>
        {
            var (cleanup, url) = downloadService.AddDownload(contentFactory, mimeType, suggestedName);
            downloadUrl.Set(url);
            return cleanup;
        }, [EffectTrigger.OnMount()]);

        var dialog = new SaveDialog
        {
            TriggerCount = triggerCount.Value,
            SuggestedName = suggestedName,
            Accept = MimeTypeToAccept(mimeType),
            DownloadUrl = downloadUrl.Value,
            OnSaved = new(e => { saveCallback.Value?.Invoke(e.Value); return ValueTask.CompletedTask; }),
        };

        var showSaveDialog = new ShowSaveDialogDelegate(callback =>
        {
            saveCallback.Set(callback);
            triggerCount.Set(triggerCount.Value + 1);
        });

        return (dialog, showSaveDialog);
    }

    private static string? MimeTypeToAccept(string mimeType)
    {
        return mimeType switch
        {
            "application/pdf" => ".pdf",
            "text/plain" => ".txt",
            "text/csv" => ".csv",
            "text/html" => ".html",
            "application/json" => ".json",
            "application/xml" => ".xml",
            "image/png" => ".png",
            "image/jpeg" => ".jpg",
            "image/gif" => ".gif",
            "image/svg+xml" => ".svg",
            _ => null
        };
    }
}
