using Ivy.Core.Hooks;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Delegate for showing the folder dialog. Accepts a callback that receives
/// the folder entries when a folder is selected. Following the UseAlert pattern.
/// </summary>
public delegate void ShowFolderDialogDelegate(Action<FolderDialogEntry[]> callback);

public static class UseFolderDialogExtensions
{
    /// <summary>
    /// Opens the native folder picker dialog. Returns the list of entries
    /// (files and subdirectories) in the selected folder.
    /// On Chromium, uses showDirectoryPicker for true folder selection.
    /// On other browsers, falls back to input[webkitdirectory].
    /// Returns (dialogView, showFolderDialog) tuple following the UseAlert pattern.
    /// Call showFolderDialog(callback) to open the dialog; the callback receives folder entries.
    /// </summary>
    public static (object? dialogView, ShowFolderDialogDelegate showFolderDialog) UseFolderDialog(
        this IViewContext context)
    {
        var triggerCount = context.UseState(0);
        var folderCallback = context.UseRef<Action<FolderDialogEntry[]>?>();

        var dialog = new FolderDialog
        {
            TriggerCount = triggerCount.Value,
            OnFolderSelected = new(e => { folderCallback.Value?.Invoke(e.Value); return ValueTask.CompletedTask; }),
        };

        var showFolderDialog = new ShowFolderDialogDelegate(callback =>
        {
            folderCallback.Set(callback);
            triggerCount.Set(triggerCount.Value + 1);
        });

        return (dialog, showFolderDialog);
    }
}
