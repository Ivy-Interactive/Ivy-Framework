using System.Collections.Immutable;
using Ivy.Hooks;
using Ivy.Services;
using Ivy.Shared;
using Ivy.Views.Builders;
using Ivy.Views.Tables;
using Microsoft.Extensions.Logging;
using System.Linq;
using Ivy.Views.Forms;

namespace Ivy.Samples.Shared.Apps.Concepts;

[App(icon: Icons.Upload, searchHints: ["file", "attachment", "upload", "stream", "progress", "multipart"])]
public class UploadApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Tabs(
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
        var upload = this.UseUpload(MemoryStreamUploadHandler.Create(uploadState)).Accept("*/*").MaxFileSize(10 * 1024 * 1024);

        return Layout.Vertical()
               | Text.H1("Single File Upload")
               | uploadState.ToFileInput(upload).Placeholder("Choose a file to upload")
               | uploadState.ToDetails()
                   .Builder(e => e!.Length, e => e.Func((long x) => Utils.FormatBytes(x)))
                   .Builder(e => e!.Progress, e => e.Func((float x) => x.ToString("P0")))
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
                     | Text.H1("Multiple Files Upload")
                     | selectedFiles.ToFileInput(upload).Placeholder("Choose files to upload")
                     | selectedFiles.Value.ToTable()
                         .Width(Size.Full())
                         .Builder(e => e.Length, e => e.Func((long x) => Utils.FormatBytes(x)))
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
               | Text.H1("Dialog Upload")
               | openButton
               | (selectedFile.Value != null
                    ? selectedFile.ToDetails()
                        .Builder(e => e!.Length, e => e.Func((long x) => Utils.FormatBytes(x)))
                        .Builder(e => e!.Progress, e => e.Func((float x) => x.ToString("P0")))
                    : Text.Block("No file selected"))
               | dialog;
    }
}

public record FileUploadValidationSettings
{
    public long MaxFileSize { get; init; } = 5 * 1024 * 1024; // 5 MB

    public int MaxFiles { get; init; } = 3;

    public string? Accept { get; init; }

    public string? Placeholder { get; init; } = null!;
}


public class FileUploadValidation : ViewBase
{
    public override object? Build()
    {
        var settings = UseState(new FileUploadValidationSettings());
        return new SidebarLayout(
            new FileUploadValidationUploader(settings.Value).Key(settings),
            settings.ToForm(submitTitle: "Update")
        );
    }
}

public class FileUploadValidationUploader(FileUploadValidationSettings settings) : ViewBase
{
    public override object? Build()
    {
        var selectedFiles = UseState(ImmutableArray.Create<FileUpload<byte[]>>());
        var upload = this.UseUpload(MemoryStreamUploadHandler.Create(selectedFiles)).Accept(settings.Accept!).MaxFileSize(settings.MaxFileSize).MaxFiles(settings.MaxFiles);

        var layout = Layout.Vertical()
                     | Text.H1("Multiple Files Upload")
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
    public string Subject { get; set; } = string.Empty;

    public FileUpload<byte[]>? Attachment { get; set; }
}

public class FormFileUpload : ViewBase
{
    public override object? Build()
    {
        return "";
        //var model = UseState(new FormFileUploadModel());
        //return model.ToForm();
    }
}