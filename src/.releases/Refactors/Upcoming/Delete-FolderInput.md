# [Upcoming] Delete FolderInput Widget

The `FolderInput` widget has been removed from the Ivy Framework.

## Rationale

`FolderInput` was found to be redundant or not fitting the desired framework patterns. Core folder selection functionality should now be handled via the `UseFolderDialog` hook for custom UI, or via `TextInput` for manual path entry when appropriate.

## Migration Guide

### 1. Replace FolderInput with TextInput

If you were using `FolderInput` primarily for text entry of paths, replace it with `TextInput`.

**Old:**

```csharp
folderPath.ToFolderInput("Select folder...")
```

**New:**

```csharp
folderPath.ToTextInput("Enter folder path...")
```

### 2. Manual Folder Selection

If you need a "Browse" button, combine `TextInput` with the `UseFolderDialog` hook.

```csharp
var (folderDialogView, showFolderDialog) = UseFolderDialog();

// In Build():
return Layout.Horizontal()
    | folderDialogView
    | pathState.ToTextInput("Path...")
    | new Button().Icon(Icons.Folder).OnClick(() => showFolderDialog(entries => {
        var first = entries.FirstOrDefault();
        if (first != null) pathState.Set(first.RelativePath);
    }));
```

> [!NOTE]
> `FolderDialogEntry` currently provides `Name`, `Kind`, and `RelativePath`. For desktop environments requiring absolute paths, manual entry in a `TextInput` is currently recommended.
