using System.Text;

namespace Ivy.Samples.Shared.Apps.Widgets;

[App(icon: Icons.FolderOpen, path: ["Widgets"], searchHints: ["file", "dialog", "picker", "open", "save", "folder", "browse", "native"])]
public class FileDialogApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Vertical()
               | Text.H1("File Dialog Hooks")
               | Layout.Tabs(
                   new Tab("Dropdown Menu", new DropdownMenuDemo()),
                   new Tab("Upload Mode", new UploadModeDemo()),
                   new Tab("PathOnly Mode", new PathOnlyModeDemo()),
                   new Tab("Multiple Files", new MultipleFilesDemo()),
                   new Tab("Save Dialog", new SaveDialogDemo()),
                   new Tab("Folder Picker", new FolderPickerDemo())
               ).Variant(TabsVariant.Content);
    }
}

/// <summary>
/// Primary demo: icon button → dropdown menu → "Select File…" → native dialog → upload → display image.
/// </summary>
public class DropdownMenuDemo : ViewBase
{
    public override object? Build()
    {
        var imageState = UseState<FileUpload<byte[]>?>();

        var (fileDialogView, showFileDialog) = UseFileDialog(
            MemoryStreamUploadHandler.Create(imageState),
            accept: "image/*");

        object? imageView = null;
        if (imageState.Value is { Status: FileUploadStatus.Finished } upload)
        {
            var base64 = Convert.ToBase64String(upload.Content!);
            var dataUrl = $"data:{upload.ContentType};base64,{base64}";
            imageView = new Image(dataUrl) { Alt = upload.FileName }
                .Width(Size.Units(80))
                .Height(Size.Units(80));
        }

        var trigger = new Button("Actions", icon: Icons.Image, variant: ButtonVariant.Outline);

        var menu = new DropDownMenu(DropDownMenu.DefaultSelectHandler(), trigger)
                   | MenuItem.Default("Select File…", tag: "select-file")
                       .Icon(Icons.Upload)
                       .OnSelect(() => showFileDialog(_ => { }));

        return Layout.Vertical()
               | Text.H2("Icon Button with Dropdown Menu")
               | Text.P("Click the button, then choose 'Select File…' to open the native file dialog.")
               | fileDialogView
               | menu
               | (imageView ?? Text.Muted("No image selected"));
    }
}

public class UploadModeDemo : ViewBase
{
    public override object? Build()
    {
        var imageState = UseState<FileUpload<byte[]>?>();

        var (fileDialogView, showFileDialog) = UseFileDialog(
            MemoryStreamUploadHandler.Create(imageState),
            accept: "image/*");

        object? imageView = null;
        if (imageState.Value is { Status: FileUploadStatus.Finished } upload)
        {
            var base64 = Convert.ToBase64String(upload.Content!);
            var dataUrl = $"data:{upload.ContentType};base64,{base64}";
            imageView = new Image(dataUrl) { Alt = upload.FileName }
                .Width(Size.Units(80))
                .Height(Size.Units(80));
        }

        return Layout.Vertical()
               | Text.H2("Upload Mode")
               | Text.P("Opens a native file dialog, uploads the file, and displays the image.")
               | fileDialogView
               | new Button("Pick Image", _ => showFileDialog(files =>
               {
                   // Upload is handled by the handler; callback receives metadata
               }), icon: Icons.Image)
               | (imageView ?? Text.Muted("No image selected"));
    }
}

public class PathOnlyModeDemo : ViewBase
{
    public override object? Build()
    {
        var selectedFile = UseState<string?>();

        var (fileDialogView, showFileDialog) = UseFileDialog(accept: "image/*");

        return Layout.Vertical()
               | Text.H2("PathOnly Mode")
               | Text.P("Opens the native file dialog and returns file metadata without uploading.")
               | fileDialogView
               | new Button("Select File", _ => showFileDialog(files =>
               {
                   var file = files.FirstOrDefault();
                   if (file != null)
                       selectedFile.Set($"{file.FileName} ({file.ContentType}, {file.Size} bytes)");
               }))
               | Text.P(selectedFile.Value ?? "No file selected");
    }
}

public class MultipleFilesDemo : ViewBase
{
    public override object? Build()
    {
        var fileNames = UseState<string[]?>();

        var (fileDialogView, showFileDialog) = UseFileDialog(
            accept: ".pdf,.docx,.txt",
            multiple: true);

        return Layout.Vertical()
               | Text.H2("Multiple File Selection")
               | Text.P("Select multiple files at once. Restricted to .pdf, .docx, .txt files.")
               | fileDialogView
               | new Button("Select Files", _ => showFileDialog(files =>
               {
                   fileNames.Set(files.Select(f => $"{f.FileName} ({f.Size} bytes)").ToArray());
               }))
               | (fileNames.Value != null && fileNames.Value.Length > 0
                   ? (object)Layout.Vertical(fileNames.Value.Select(n => Text.P(n)).ToArray())
                   : Text.Muted("No files selected"));
    }
}

public class SaveDialogDemo : ViewBase
{
    public override object? Build()
    {
        var saveResult = UseState<string?>();

        var (saveDialogView, showSaveDialog) = UseSaveDialog(
            contentFactory: async () => Encoding.UTF8.GetBytes("Hello, World! This is a test file generated by Ivy."),
            mimeType: "text/plain",
            suggestedName: "hello.txt");

        return Layout.Vertical()
               | Text.H2("Save Dialog")
               | Text.P("Opens the native save dialog to save generated content to a file.")
               | saveDialogView
               | new Button("Save File", _ => showSaveDialog(result =>
               {
                   saveResult.Set(result.Success ? $"Saved as {result.FileName}" : "Save cancelled");
               }), icon: Icons.Download)
               | Text.P(saveResult.Value ?? "");
    }
}

public class FolderPickerDemo : ViewBase
{
    public override object? Build()
    {
        var entries = UseState<FolderDialogEntry[]?>();

        var (folderDialogView, showFolderDialog) = UseFolderDialog();

        return Layout.Vertical()
               | Text.H2("Folder Picker")
               | Text.P("Opens the native folder picker and lists the entries found in the selected folder.")
               | folderDialogView
               | new Button("Browse Folder", _ => showFolderDialog(selected =>
               {
                   entries.Set(selected);
               }), icon: Icons.Folder)
               | (entries.Value != null && entries.Value.Length > 0
                   ? (object)Layout.Vertical(
                       entries.Value.Take(50).Select(e =>
                           Text.P($"[{e.Kind}] {e.Name}").Small()
                       ).ToArray()
                   )
                   : Text.Muted("No folder selected"));
    }
}
