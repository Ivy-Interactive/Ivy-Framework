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
        var lastSubmission = UseState("");

        return Layout.Vertical()
               | Text.H2("ShortcutKey + OnSubmit")
               | Text.P("Press Ctrl+Enter (or Cmd+Enter on Mac) to submit. The shortcut badge appears on the send button.")
               | text.ToContentInput(upload)
                   .Files(files.Value)
                   .Placeholder("Type and press Ctrl+Enter to submit...")
                   .ShortcutKey("Ctrl+Enter")
                   .OnSubmit(() =>
                   {
                       lastSubmission.Set($"Submitted: \"{text.Value}\" with {files.Value.Length} file(s)");
                       text.Set("");
                       files.Set(ImmutableArray<FileUpload<byte[]>>.Empty);
                   })
               | (string.IsNullOrEmpty(lastSubmission.Value)
                   ? (object)Text.Muted("Nothing submitted yet")
                   : Callout.Success(lastSubmission.Value));
    }
}
