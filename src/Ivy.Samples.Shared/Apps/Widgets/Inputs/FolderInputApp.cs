namespace Ivy.Samples.Shared.Apps.Widgets.Inputs;

[App(icon: Icons.FolderOpen, group: ["Widgets", "Inputs"], searchHints: ["folder", "directory", "browse", "path", "picker"])]
public class FolderInputApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Vertical()
               | Text.H1("Folder Input")
               | Text.H2("Basic Usage")
               | new BasicFolderInputDemo()
               | Text.H2("Disabled")
               | new DisabledFolderInputDemo()
               | Text.H2("Validation")
               | new ValidationFolderInputDemo()
               | Text.H2("Full Path Mode (Desktop)")
               | new FullPathFolderInputDemo();
    }
}

public class BasicFolderInputDemo : ViewBase
{
    public override object? Build()
    {
        var folder = UseState<string?>();
        return Layout.Vertical()
               | folder.ToFolderInput()
               | Text.P($"Selected: {folder.Value ?? "None"}");
    }
}

public class DisabledFolderInputDemo : ViewBase
{
    public override object? Build()
    {
        var folder = UseState<string?>("my-project");
        return Layout.Vertical()
               | folder.ToFolderInput().Disabled()
               | Text.P($"Selected: {folder.Value ?? "None"}");
    }
}

public class ValidationFolderInputDemo : ViewBase
{
    public override object? Build()
    {
        var folder = UseState<string?>();
        var invalid = folder.Value == null ? "Please select a folder" : null;
        return Layout.Vertical()
               | folder.ToFolderInput().Invalid(invalid)
               | Text.P($"Selected: {folder.Value ?? "None"}");
    }
}

public class FullPathFolderInputDemo : ViewBase
{
    public override object? Build()
    {
        var folder = UseState<string?>();
        return Layout.Vertical()
               | folder.ToFolderInput(mode: FolderInputMode.FullPath)
               | Text.P($"Full path: {folder.Value ?? "None"}")
               | Text.Muted("Full path mode requires a desktop environment (Electron/Tauri). In browsers, falls back to folder name.");
    }
}
