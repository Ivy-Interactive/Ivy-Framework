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
               | new ValidationFolderInputDemo();
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
