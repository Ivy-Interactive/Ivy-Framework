using System.Collections.Immutable;

namespace Ivy.Samples.Shared.Apps.Widgets.Inputs;

[App(icon: Icons.MessageSquarePlus, group: ["Widgets", "Inputs"], searchHints: ["content", "text", "file", "attachment", "drag-drop", "paste", "compose"])]
public class ContentInputApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Vertical()
               | Text.H1("Content Input")
               | Text.P("A text input with integrated file attachment support via drag-and-drop, clipboard paste, and file picker.")
               | Layout.Tabs(
                   new Tab("Basic", new ContentInputBasicExample()),
                   new Tab("With Files", new ContentInputWithFilesExample()),
                   new Tab("Scale", new ContentInputScaleExample()),
                   new Tab("Invalid", new ContentInputInvalidExample()),
                   new Tab("Configured", new ContentInputConfiguredExample()),
                   new Tab("Submit", new ContentInputSubmitExample())
               ).Variant(TabsVariant.Content);
    }
}

public class ContentInputBasicExample : ViewBase
{
    public override object? Build()
    {
        var text = UseState("");
        var files = UseState(ImmutableArray<FileUpload<byte[]>>.Empty);
        var upload = UseUpload(MemoryStreamUploadHandler.Create(files));

        return Layout.Vertical()
               | Text.H2("Basic Usage")
               | Text.P("A simple content input with default settings. Drag files onto the textarea, paste from clipboard, or use the paperclip button.")
               | text.ToContentInput(upload)
                   .Files(files.Value)
                   .Placeholder("Type a message... (paste or drag files here)")
               | Layout.Vertical().Gap(2)
                   | Text.Label("Current text:")
                   | Text.Monospaced(string.IsNullOrEmpty(text.Value) ? "(empty)" : text.Value)
                   | Text.Label($"Attached files: {files.Value.Length}");
    }
}

public class ContentInputWithFilesExample : ViewBase
{
    public override object? Build()
    {
        var text = UseState("");
        var files = UseState(ImmutableArray<FileUpload<byte[]>>.Empty);
        var upload = UseUpload(MemoryStreamUploadHandler.Create(files));

        return Layout.Vertical()
               | Text.H2("With File Attachments")
               | Text.P("Upload files and see them appear as attachments below the text area. Click the X button to remove attachments.")
               | text.ToContentInput(upload)
                   .Files(files.Value)
                   .Placeholder("Describe the issue... (paste screenshots or drag files)")
                   .Accept("image/*,.pdf")
                   .MaxFiles(5)
                   .Rows(4)
               | (files.Value.Length > 0
                   ? files.Value.ToTable()
                       .Width(Size.Full())
                       .Builder(e => e.FileName, e => e.Func((string x) => x))
                       .Builder(e => e.Length, e => e.Func((long x) => StringHelper.FormatBytes(x)))
                       .Builder(e => e.Progress, e => e.Func((float x) => x.ToString("P0")))
                       .Remove(e => e.Id)
                   : (object)Text.Muted("No files attached"));
    }
}

public class ContentInputScaleExample : ViewBase
{
    public override object? Build()
    {
        var text = UseState("");
        var files = UseState(ImmutableArray<FileUpload<byte[]>>.Empty);
        var upload = UseUpload(MemoryStreamUploadHandler.Create(files));

        return Layout.Grid().Columns(3)
               | Text.Monospaced("Small")
               | Text.Monospaced("Medium")
               | Text.Monospaced("Large")
               | text.ToContentInput(upload).Files(files.Value).Placeholder("Small...").Small()
               | text.ToContentInput(upload).Files(files.Value).Placeholder("Medium...")
               | text.ToContentInput(upload).Files(files.Value).Placeholder("Large...").Large();
    }
}

public class ContentInputInvalidExample : ViewBase
{
    public override object? Build()
    {
        var text = UseState("");
        var files = UseState(ImmutableArray<FileUpload<byte[]>>.Empty);
        var upload = UseUpload(MemoryStreamUploadHandler.Create(files));

        return Layout.Vertical()
               | Text.H2("Invalid State")
               | Text.P("Content input with validation error. Hover the icon to see the error message.")
               | text.ToContentInput(upload)
                   .Files(files.Value)
                   .Placeholder("Type a message...")
                   .Invalid("Message is required and must be at least 10 characters");
    }
}

public class ContentInputConfiguredExample : ViewBase
{
    public override object? Build()
    {
        var text = UseState("");
        var files = UseState(ImmutableArray<FileUpload<byte[]>>.Empty);
        var upload = UseUpload(MemoryStreamUploadHandler.Create(files));

        return Layout.Vertical()
               | Text.H2("Configured")
               | Text.P("Content input with various configuration options.")
               | (Layout.Grid().Columns(2)
                   | Text.Label("With max length (500)")
                   | text.ToContentInput(upload)
                       .Files(files.Value)
                       .Placeholder("Max 500 characters")
                       .MaxLength(500)
                   | Text.Label("Disabled")
                   | text.ToContentInput(upload)
                       .Files(files.Value)
                       .Placeholder("This input is disabled")
                       .Disabled()
                   | Text.Label("With rows (6)")
                   | text.ToContentInput(upload)
                       .Files(files.Value)
                       .Placeholder("Taller textarea")
                       .Rows(6)
               );
    }
}

public class ContentInputSubmitExample : ViewBase
{
    public override object? Build()
    {
        var text = UseState("");
        var files = UseState(ImmutableArray<FileUpload<byte[]>>.Empty);
        var upload = UseUpload(MemoryStreamUploadHandler.Create(files));
        var client = UseService<IClientProvider>();

        return Layout.Vertical()
               | Text.H2("Submit on Enter")
               | Text.P("By default, ContentInput submits on Cmd+Enter (Ctrl+Enter on Windows).")
               | text.ToContentInput(upload)
                   .Files(files.Value)
                   .Placeholder("Type something and press Cmd+Enter...")
                   .OnSubmit((value) => client.Toast($"Value: {value}", "Submitted"))
               | Text.Muted("Use Shift+Enter for new lines.");
    }
}
