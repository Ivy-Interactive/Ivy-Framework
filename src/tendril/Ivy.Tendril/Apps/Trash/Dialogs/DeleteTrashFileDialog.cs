namespace Ivy.Tendril.Apps.Trash.Dialogs;

public class DeleteTrashFileDialog(
    IState<bool> confirmDelete,
    TrashFileInfo? selected,
    IState<string?> selectedFile,
    RefreshToken refreshToken) : ViewBase
{
    private readonly IState<bool> _confirmDelete = confirmDelete;
    private readonly TrashFileInfo? _selected = selected;
    private readonly IState<string?> _selectedFile = selectedFile;
    private readonly RefreshToken _refreshToken = refreshToken;

    public override object? Build()
    {
        if (!_confirmDelete.Value || _selected is null) return null;

        var deletePath = _selected.FilePath;

        return new Dialog(
            _ => _confirmDelete.Set(false),
            new DialogHeader("Delete Trash File"),
            new DialogBody(
                Text.P($"Permanently delete {_selected.FileName}?")
            ),
            new DialogFooter(
                new Button("Cancel").Outline().ShortcutKey("Escape").OnClick(() => _confirmDelete.Set(false)),
                new Button("Delete").Destructive().ShortcutKey("Enter").AutoFocus().OnClick(() =>
                {
                    if (File.Exists(deletePath))
                        File.Delete(deletePath);
                    _selectedFile.Set(null);
                    _confirmDelete.Set(false);
                    _refreshToken.Refresh();
                })
            )
        ).Width(Size.Rem(40));
    }
}
