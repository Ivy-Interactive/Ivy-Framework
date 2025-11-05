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
        // Single File Upload
        var singleFileUploadState = UseState<FileUpload<byte[]>?>();
        var singleFileUpload = this.UseUpload(MemoryStreamUploadHandler.Create(singleFileUploadState))
            .Accept("*/*").MaxFileSize(10 * 1024 * 1024);

        // Multiple Files Upload
        var multipleFilesUploadState = UseState(ImmutableArray.Create<FileUpload<byte[]>>());
        var multipleFilesUpload = this.UseUpload(MemoryStreamUploadHandler.Create(multipleFilesUploadState))
            .Accept("*/*").MaxFileSize(10 * 1024 * 1024);

        // Dialog Upload
        var dialogSelectedFile = UseState<FileUpload<byte[]>?>();
        var dialogFile = UseState<FileUpload<byte[]>?>();
        var dialogUploadContext = this.UseUpload(MemoryStreamUploadHandler.Create(dialogFile))
            .Accept("*/*").MaxFileSize(10 * 1024 * 1024);
        var dialogIsOpen = UseState(false);

        ValueTask OnDialogClose(Event<Dialog> _)
        {
            dialogIsOpen.Value = false;
            dialogFile.Reset();
            return ValueTask.CompletedTask;
        }

        var openDialogButton = new Button("Open Dialog", _ =>
        {
            dialogFile.Reset();
            dialogIsOpen.Value = true;
        });

        var dialog = dialogIsOpen.Value
            ? new Dialog(
                OnDialogClose,
                new DialogHeader("Select File"),
                new DialogBody(
                    Layout.Vertical()
                        | dialogFile.ToFileInput(dialogUploadContext)
                            .Accept("*/*")
                            .Placeholder("Choose a file to upload")
                ),
                new DialogFooter(
                    new Button("Cancel", _ =>
                    {
                        dialogIsOpen.Value = false;
                        dialogFile.Reset();
                    }, variant: ButtonVariant.Outline),
                    new Button("Ok", _ =>
                    {
                        if (dialogFile.Value != null)
                        {
                            dialogSelectedFile.Set(dialogFile.Value);
                        }
                        dialogIsOpen.Value = false;
                        dialogFile.Reset();
                    })
                )
            )
            : null;

        // Form File Upload
        var formModel = UseState(() => new FormFileUploadModel());

        // Validation
        var validationSettings = UseState(new FileUploadValidationSettings());

        return Layout.Vertical()
               | Text.H1("Uploads")

               | Text.H2("Single File Upload")
               | Layout.Vertical()
                   | singleFileUploadState.ToFileInput(singleFileUpload).Placeholder("Choose a file to upload")
                   | singleFileUploadState.ToDetails()

               | Text.H2("Multiple Files Upload")
               | Layout.Vertical()
                   | multipleFilesUploadState.ToFileInput(multipleFilesUpload).Placeholder("Choose files to upload")
                   | multipleFilesUploadState.Value.ToTable()
                       .Width(Size.Full())
                       .Builder(e => e.Length, e => e.Func((long x) => Utils.FormatBytes(x)))
                       .Builder(e => e.Progress, e => e.Func((float x) => x.ToString("P0")))
                       .Remove(e => e.Id)

               | Text.H2("Dialog Upload")
               | Layout.Vertical()
                   | openDialogButton
                   | (dialogSelectedFile.Value != null
                        ? dialogSelectedFile.ToDetails()
                        : Text.P("No file selected"))
                   | dialog

               | Text.H2("Form File Upload")
               | new FormFileUploadContent(formModel)

               | Text.H2("Upload Validation")
               | new FileUploadValidationContent(validationSettings)
            ;
    }
}

public class FormFileUploadContent : ViewBase
{
    private readonly IState<FormFileUploadModel> _model;

    public FormFileUploadContent(IState<FormFileUploadModel> model)
    {
        _model = model;
    }

    public override object? Build()
    {
        var form = _model.ToForm()
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
               | form
               | _model.Value.Attachment1?.ToDetails()
               | _model.Value.Attachment2?.ToDetails()
            ;
    }
}

public class FileUploadValidationContent : ViewBase
{
    private readonly IState<FileUploadValidationSettings> _settings;

    public FileUploadValidationContent(IState<FileUploadValidationSettings> settings)
    {
        _settings = settings;
    }

    public override object? Build()
    {
        return Layout.Horizontal()
               | new FileUploadValidationUploader(_settings.Value).Key(_settings)
               | new Separator()
               | _settings.ToForm(submitTitle: "Update").WithLayout().Width(120);
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

        var layout = Layout.Vertical()
                     | selectedFiles.ToFileInput(upload).Placeholder(settings.Placeholder!)
                     | selectedFiles.Value.ToTable()
                         .Width(Size.Full())
                         .Builder(e => e.Length, e => e.Func((long x) => Utils.FormatBytes(x)))
                         .Builder(e => e.Progress, e => e.Func((float x) => x.ToString("P0")))
                         .Remove(e => e.Id);

        return layout;
    }
}

public record FormFileUploadModel
{
    [Required]
    public FileUpload<byte[]>? Attachment1 { get; set; }

    public FileUpload<byte[]>? Attachment2 { get; set; }
}


