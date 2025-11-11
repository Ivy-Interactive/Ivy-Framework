using Ivy.Core.Helpers;
using Ivy.Hooks;
using Ivy.Shared;
using Ivy.Views.Builders;
using Ivy.Views.Tables;
using Ivy.Views.Forms;

namespace Ivy.Samples.Shared.Apps.Concepts;

[App(icon: Icons.Upload, searchHints: ["file", "attachment", "upload", "stream", "progress", "multipart"])]
public class UploadApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Vertical()
               | Text.H1("Uploads")
               | Layout.Tabs(
                   new Tab("Single File", new SingleFileUpload()),
                   new Tab("Multiple Files", new MultipleFilesUpload()),
                   new Tab("Dialog", new DialogFileUpload()),
                   new Tab("Form", new FormFileUpload()),
                   new Tab("Validation", new FileUploadValidation())
               ).Variant(TabsVariant.Content);
    }
}

public class SingleFileUpload : ViewBase
{
    public override object? Build()
    {
        var uploadState = UseState<FileUpload<byte[]>?>();
        var upload = this.UseUpload(MemoryStreamUploadHandler.Create(uploadState))
            .Accept("*/*").MaxFileSize(10 * 1024 * 1024);

        return Layout.Vertical()
               | Text.H2("Single File")
               | Text.P("Upload a single file with progress tracking and file details display.")
               | uploadState.ToFileInput(upload).Placeholder("Choose a file to upload")
               | uploadState.ToDetails()
            ;
    }
}

public class MultipleFilesUpload : ViewBase
{
    public override object? Build()
    {
        var selectedFiles = UseState(ImmutableArray.Create<FileUpload<byte[]>>());
        var upload = this.UseUpload(MemoryStreamUploadHandler.Create(selectedFiles)).Accept("*/*").MaxFileSize(10 * 1024 * 1024);

        var layout = Layout.Vertical()
                     | Text.H2("Multiple Files")
                     | Text.P("Upload multiple files simultaneously with a table showing upload progress for each file.")
                     | selectedFiles.ToFileInput(upload).Placeholder("Choose files to upload")
                     | selectedFiles.Value.ToTable()
                         .Width(Size.Full())
                         .Builder(e => e.Length, e => e.Func((long x) => Ivy.Utils.FormatBytes(x)))
                         .Builder(e => e.Progress, e => e.Func((float x) => x.ToString("P0")))
                         .Remove(e => e.Id);

        return layout;
    }
}

public class DialogFileUpload : ViewBase
{
    public override object? Build()
    {
        var selectedFile = UseState<FileUpload<byte[]>?>();

        // Ephemeral state used inside the dialog while picking a file
        var dialogFile = UseState<FileUpload<byte[]>?>();
        var uploadContext = this.UseUpload(MemoryStreamUploadHandler.Create(dialogFile)).Accept("*/*").MaxFileSize(10 * 1024 * 1024);

        // Dialog visibility state
        var isOpen = UseState(false);

        ValueTask OnDialogClose(Event<Dialog> _)
        {
            isOpen.Value = false;
            dialogFile.Reset();
            return ValueTask.CompletedTask;
        }

        var openButton = new Button("Open Dialog", _ =>
        {
            dialogFile.Reset();
            isOpen.Value = true;
        });

        var dialog = isOpen.Value
            ? new Dialog(
                OnDialogClose,
                new DialogHeader("Select File"),
                new DialogBody(
                    Layout.Vertical()
                        | dialogFile.ToFileInput(uploadContext)
                            .Accept("*/*")
                            .Placeholder("Choose a file to upload")
                ),
                new DialogFooter(
                    new Button("Cancel", _ =>
                    {
                        isOpen.Value = false;
                        dialogFile.Reset();
                    }, variant: ButtonVariant.Outline),
                    new Button("Ok", _ =>
                    {
                        if (dialogFile.Value != null)
                        {
                            selectedFile.Set(dialogFile.Value);
                        }
                        isOpen.Value = false;
                        dialogFile.Reset();
                    })
                )
            )
            : null;

        return Layout.Vertical()
               | Text.H2("Dialog")
               | Text.P("Upload files through a dialog interface, allowing users to select files in a modal window.")
               | openButton
               | (selectedFile.Value != null
                    ? selectedFile.ToDetails()
                    : Text.P("No file selected"))
               | dialog;
    }
}

public class FormFileUpload : ViewBase
{
    public override object? Build()
    {
        var model = UseState(() => new FormFileUploadModel());

        var form = model.ToForm()
            .Builder(e => e.Attachment1, (state, view) =>
            {
                var uploadContext = view.UseUpload(MemoryStreamUploadHandler.Create(state))
                    .Accept("image/jpeg").MaxFileSize(1 * 1024 * 1024);
                return state.ToFileInput(uploadContext);
            })
            .Label(x => x.Attachment1, "image/jpeg (Required)")
            .Builder(e => e.Attachment2, (state, view) =>
            {
                var uploadContext = view.UseUpload(MemoryStreamUploadHandler.Create(state))
                    .Accept("application/pdf").MaxFileSize(5 * 1024 * 1024);
                return state.ToFileInput(uploadContext);
            })
            .Label(x => x.Attachment2, "application/pdf (Optional)");

        return Layout.Vertical()
               | Text.H2("Form")
               | Text.P("Integrate file uploads into forms with validation, supporting multiple file fields with different requirements.")
               | form
               | model.Value.Attachment1?.ToDetails()
               | model.Value.Attachment2?.ToDetails()
               ;
    }
}

public class FileUploadValidation : ViewBase
{
    public override object? Build()
    {
        var settings = UseState(new FileUploadValidationSettings());
        return Layout.Vertical()
               | Text.H2("Validation")
               | Text.P("Configure and test file upload validation rules including file size limits, file count limits, and accepted file types.")
               | (Layout.Horizontal()
                   | new FileUploadValidationUploader(settings.Value).Key(settings)
                   | settings.ToForm(submitTitle: "Update").WithLayout().Width(120));
    }
}

public record FileUploadValidationSettings
{
    public long MaxFileSize { get; init; } = 5 * 1024 * 1024; // 5 MB

    public int MaxFiles { get; init; } = 3;

    public string? Accept { get; init; }

    public string? Placeholder { get; init; } = null!;
}


public class FileUploadValidationUploader(FileUploadValidationSettings settings) : ViewBase
{
    public override object? Build()
    {
        var selectedFiles = UseState(ImmutableArray.Create<FileUpload<byte[]>>());
        var upload = this.UseUpload(MemoryStreamUploadHandler.Create(selectedFiles))
            .Accept(settings.Accept!)
            .MaxFileSize(settings.MaxFileSize)
            .MaxFiles(settings.MaxFiles);

        return Layout.Vertical()
                    | selectedFiles.ToFileInput(upload).Placeholder(settings.Placeholder!)
                    | selectedFiles.Value.ToTable()
                        .Width(Size.Full())
                        .Builder(e => e.Length, e => e.Func((long x) => Ivy.Utils.FormatBytes(x)))
                        .Builder(e => e.Progress, e => e.Func((float x) => x.ToString("P0")))
                        .Remove(e => e.Id);
    }
}

public record FormFileUploadModel
{
    [Required]
    public FileUpload<byte[]>? Attachment1 { get; set; }

    public FileUpload<byte[]>? Attachment2 { get; set; }
}


