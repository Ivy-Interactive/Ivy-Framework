---
searchHints:
  - folder
  - directory
  - browse
  - path
  - picker
---

# FolderInput

<Ingress>
Select a folder from the user's file system with a native directory picker.
</Ingress>

The `FolderInput` widget provides a form-compatible input for selecting folders. It displays the selected folder name in a read-only text field with a browse button that opens the native directory picker.

## Basic Usage

```csharp demo-below
public class FolderInputDemo : ViewBase
{
    public override object? Build()
    {
        var folder = UseState<string?>();
        return Layout.Vertical()
            | folder.ToFolderInput()
            | Text.P($"Selected: {folder.Value ?? "None"}");
    }
}
```

## Placeholder

```csharp demo-below
public class FolderInputPlaceholderDemo : ViewBase
{
    public override object? Build()
    {
        var folder = UseState<string?>();
        return folder.ToFolderInput().Placeholder("Choose project directory...");
    }
}
```

## Disabled

```csharp demo-below
public class FolderInputDisabledDemo : ViewBase
{
    public override object? Build()
    {
        var folder = UseState<string?>("my-project");
        return folder.ToFolderInput().Disabled();
    }
}
```

## Validation

```csharp demo-below
public class FolderInputValidationDemo : ViewBase
{
    public override object? Build()
    {
        var folder = UseState<string?>();
        var invalid = folder.Value == null ? "Please select a folder" : null;
        return folder.ToFolderInput().Invalid(invalid);
    }
}
```

## Mode

By default, `FolderInput` returns only the folder name (web-safe). In desktop environments (Electron/Tauri), you can use `FolderInputMode.FullPath` to get the full filesystem path.

```csharp demo-below
public class FolderInputModeDemo : ViewBase
{
    public override object? Build()
    {
        var folder = UseState<string?>();
        return Layout.Vertical()
               | folder.ToFolderInput(mode: FolderInputMode.FullPath)
               | Text.P($"Path: {folder.Value ?? "None"}");
    }
}
```

> **Note:** In browser environments, `FullPath` mode falls back to returning the folder name only, since the File System Access API does not expose full paths.

<WidgetDocs Type="Ivy.FolderInput" ExtensionTypes="Ivy.FolderInputExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/src/Ivy/Widgets/Inputs/FolderInput.cs"/>
