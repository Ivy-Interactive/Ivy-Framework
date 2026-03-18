using System.Collections.Immutable;

namespace Ivy.Samples.Shared.Apps.Widgets.Inputs;

[App(icon: Icons.Upload, path: ["Widgets", "Inputs"], searchHints: ["upload", "file", "attachment", "drag-drop", "browse", "files"])]
public class FileInputApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Vertical()
               | Text.H1("File Inputs")
               | Layout.Tabs(
                   new Tab("Variants", new FileInputVariantTests()),
                   new Tab("Size Variants", new FileInputSizeVariants()),
                   new Tab("Data Binding", new FileInputDataBinding()),
                   new Tab("Type Restrictions", new FileInputTypeRestrictions()),
                   new Tab("File Size Limits", new FileInputFileSizeLimits()),
                   new Tab("Count Limits", new FileInputCountLimits()),
                   new Tab("Content Display", new FileInputContentDisplay()),
                   new Tab("Event Handlers", new FileInputEventHandlersExample()),
                   new Tab("Integration", new FileInputIntegrationExample()),
                   new Tab("Validation", new FileInputValidationExample())
               ).Variant(TabsVariant.Content);
    }
}

public class FileInputVariantTests : ViewBase
{
    public override object? Build()
    {
        var singleFile = UseState<FileUpload<byte[]>?>(() => null);
        var multipleFiles = UseState(ImmutableArray.Create<FileUpload<byte[]>>());
        var placeholderFile = UseState<FileUpload<byte[]>?>(() => null);

        var singleFileUpload = UseUpload(MemoryStreamUploadHandler.Create(singleFile));
        var multipleFilesUpload = UseUpload(MemoryStreamUploadHandler.Create(multipleFiles));
        var placeholderFileUpload = UseUpload(MemoryStreamUploadHandler.Create(placeholderFile));

        return Layout.Vertical()
               | Text.H2("Variants")
               | Text.P("Demonstrate different visual states of file inputs including empty, with value, disabled, invalid, and with placeholder.")
               | (Layout.Grid().Columns(6)
                  | null!
                  | Text.Monospaced("Empty")
                  | Text.Monospaced("With Value")
                  | Text.Monospaced("Disabled")
                  | Text.Monospaced("Invalid")
                  | Text.Monospaced("With Placeholder")

                  | Text.Monospaced("Single File")
                  | singleFile.ToFileInput(singleFileUpload)
                  | singleFile.ToFileInput(singleFileUpload).Placeholder("File input with value (upload a file)")
                  | singleFile.ToFileInput(singleFileUpload).Disabled()
                  | singleFile.ToFileInput(singleFileUpload).Invalid("Please select a valid file")
                  | placeholderFile.ToFileInput(placeholderFileUpload).Placeholder("Click to select a file")

                  | Text.Monospaced("Multiple Files")
                  | multipleFiles.ToFileInput(multipleFilesUpload)
                  | multipleFiles.ToFileInput(multipleFilesUpload).Placeholder("Multiple files with value (upload files)")
                  | multipleFiles.ToFileInput(multipleFilesUpload).Disabled()
                  | multipleFiles.ToFileInput(multipleFilesUpload).Invalid("Please select valid files")
                  | multipleFiles.ToFileInput(multipleFilesUpload).Placeholder("Click to select files")

                  | Text.Monospaced("Default Variant")
                  | singleFile.ToFileInput(singleFileUpload).Variant(FileInputVariant.Default).Placeholder("Select one file...")
                  | placeholderFile.ToFileInput(placeholderFileUpload).Variant(FileInputVariant.Default).Placeholder("Click to select a file")
                  | singleFile.ToFileInput(singleFileUpload).Variant(FileInputVariant.Default).Disabled()
                  | multipleFiles.ToFileInput(multipleFilesUpload).Variant(FileInputVariant.Default).Placeholder("Select multiple...")
                  | multipleFiles.ToFileInput(multipleFilesUpload).Variant(FileInputVariant.Default).MaxFiles(3).Placeholder("Max 3 files...")
               );
    }
}

public class FileInputSizeVariants : ViewBase
{
    public override object? Build()
    {
        var singleSizeFile = UseState<FileUpload<byte[]>?>(() => null);
        var multipleSizeFiles = UseState(ImmutableArray.Create<FileUpload<byte[]>>());

        var singleSizeFileUpload = UseUpload(MemoryStreamUploadHandler.Create(singleSizeFile));
        var multipleSizeFilesUpload = UseUpload(MemoryStreamUploadHandler.Create(multipleSizeFiles));

        return Layout.Vertical()
               | Text.H2("Size Variants")
               | Text.P("File inputs support different sizes: Small, Medium (default), and Large for both single and multiple file selection.")
               | (Layout.Grid().Columns(4)
                  | null!
                  | Text.Monospaced("Small")
                  | Text.Monospaced("Medium")
                  | Text.Monospaced("Large")

                  | Text.Monospaced("Single File")
                  | singleSizeFile.ToFileInput(singleSizeFileUpload).Small().Placeholder("Small file input")
                  | singleSizeFile.ToFileInput(singleSizeFileUpload).Placeholder("Medium file input")
                  | singleSizeFile.ToFileInput(singleSizeFileUpload).Large().Placeholder("Large file input")

                  | Text.Monospaced("Multiple Files")
                  | multipleSizeFiles.ToFileInput(multipleSizeFilesUpload).Small()
                  | multipleSizeFiles.ToFileInput(multipleSizeFilesUpload)
                  | multipleSizeFiles.ToFileInput(multipleSizeFilesUpload).Large()
               );
    }
}

public class FileInputDataBinding : ViewBase
{
    public override object? Build()
    {
        var dataBindingFile = UseState<FileUpload<byte[]>?>(() => null);
        var dataBindingFileUpload = UseUpload(MemoryStreamUploadHandler.Create(dataBindingFile));

        var dataBindingNullableFile = UseState<FileUpload<byte[]>?>(() => null);
        var dataBindingNullableFileUpload = UseUpload(MemoryStreamUploadHandler.Create(dataBindingNullableFile));

        var dataBindingMultipleFiles = UseState(ImmutableArray.Create<FileUpload<byte[]>>());
        var dataBindingMultipleFilesUpload = UseUpload(MemoryStreamUploadHandler.Create(dataBindingMultipleFiles));

        var dataBinding = Layout.Grid().Columns(3)
                          | Text.Monospaced("FileInput")
                          | dataBindingFile.ToFileInput(dataBindingFileUpload)
                              .Placeholder("Single file: FileUpload<byte[]>? (nullable single file binding)")
                          | dataBindingFile

                          | Text.Monospaced("FileInput?")
                          | dataBindingNullableFile.ToFileInput(dataBindingNullableFileUpload)
                              .Placeholder("Single file: FileUpload<byte[]>? (explicitly nullable binding)")
                          | dataBindingNullableFile

                          | Text.Monospaced("IEnumerable<FileInput>")
                          | dataBindingMultipleFiles.ToFileInput(dataBindingMultipleFilesUpload)
                              .Placeholder("Multiple files: ImmutableArray<FileUpload<byte[]>> (collection binding)")
                          | dataBindingMultipleFiles
            ;

        return Layout.Vertical()
               | Text.H2("Data Binding")
               | Text.P("File inputs support different data binding types for single files (nullable and non-nullable) and multiple files (collections).")
               | dataBinding;
    }
}

public class FileInputTypeRestrictions : ViewBase
{
    public override object? Build()
    {
        var textFiles = UseState(ImmutableArray.Create<FileUpload<byte[]>>());
        var pdfFiles = UseState(ImmutableArray.Create<FileUpload<byte[]>>());
        var imageFiles = UseState(ImmutableArray.Create<FileUpload<byte[]>>());
        var singleLimitFile = UseState<FileUpload<byte[]>?>(() => null);

        var textFilesUpload = UseUpload(MemoryStreamUploadHandler.Create(textFiles));
        var pdfFilesUpload = UseUpload(MemoryStreamUploadHandler.Create(pdfFiles));
        var imageFilesUpload = UseUpload(MemoryStreamUploadHandler.Create(imageFiles));
        var singleLimitFileUpload = UseUpload(MemoryStreamUploadHandler.Create(singleLimitFile));

        return Layout.Vertical()
               | Text.H2("File Type Restrictions")
               | Text.P("Restrict file selection to specific types using the Accept property with file extensions or MIME types.")
               | (Layout.Grid().Columns(3)
                  | Text.Monospaced("File Type")
                  | Text.Monospaced("Accept Property")
                  | Text.Monospaced("File Input")

                  | Text.Block("Text Files")
                  | Text.Monospaced(".txt,.md,.csv")
                  | textFiles.ToFileInput(textFilesUpload).Accept(".txt,.md,.csv").Placeholder("Select text files")

                  | Text.Block("PDF Files")
                  | Text.Monospaced(".pdf")
                  | pdfFiles.ToFileInput(pdfFilesUpload).Accept(".pdf").Placeholder("Select PDF files")

                  | Text.Block("Images")
                  | Text.Monospaced(".jpg,.jpeg,.png,.gif,.webp")
                  | imageFiles.ToFileInput(imageFilesUpload).Accept(".jpg,.jpeg,.png,.gif,.webp").Placeholder("Select image files")

                  | Text.Block("All Files")
                  | Text.Monospaced("(default)")
                  | singleLimitFile.ToFileInput(singleLimitFileUpload).Placeholder("Select any file")
               );
    }
}

public class FileInputFileSizeLimits : ViewBase
{
    public override object? Build()
    {
        var minSizeFile = UseState<FileUpload<byte[]>?>(() => null);
        var maxSizeFile = UseState<FileUpload<byte[]>?>(() => null);
        var rangeSizeFile = UseState<FileUpload<byte[]>?>(() => null);

        var minSizeUploadCtx = UseUpload(MemoryStreamUploadHandler.Create(minSizeFile));
        var maxSizeUploadCtx = UseUpload(MemoryStreamUploadHandler.Create(maxSizeFile));
        var rangeSizeUploadCtx = UseUpload(MemoryStreamUploadHandler.Create(rangeSizeFile));

        var minSizeUpload = minSizeUploadCtx.MinFileSize(1024); // 1 KB minimum
        var maxSizeUpload = maxSizeUploadCtx.MaxFileSize(5 * 1024 * 1024); // 5 MB maximum
        var rangeSizeUpload = rangeSizeUploadCtx
            .MinFileSize(1024)           // 1 KB minimum
            .MaxFileSize(10 * 1024 * 1024); // 10 MB maximum

        return Layout.Vertical()
               | Text.H2("File Size Limits")
               | Text.P("Control minimum and maximum file sizes using MinFileSize and MaxFileSize. Use MinFileSize to reject empty or trivially small files.")
               | (Layout.Grid().Columns(3)
                  | Text.Monospaced("Size Limit")
                  | Text.Monospaced("Description")
                  | Text.Monospaced("File Input")

                  | Text.Block("Min 1 KB")
                  | Text.Block("Reject files smaller than 1 KB (empty or trivial files)")
                  | minSizeFile.ToFileInput(minSizeUpload).Placeholder("Minimum 1 KB")

                  | Text.Block("Max 5 MB")
                  | Text.Block("Reject files larger than 5 MB")
                  | maxSizeFile.ToFileInput(maxSizeUpload).Placeholder("Maximum 5 MB")

                  | Text.Block("1 KB - 10 MB")
                  | Text.Block("Accept files between 1 KB and 10 MB")
                  | rangeSizeFile.ToFileInput(rangeSizeUpload).Placeholder("Between 1 KB and 10 MB")
               );
    }
}

public class FileInputCountLimits : ViewBase
{
    public override object? Build()
    {
        var limitedFiles = UseState(ImmutableArray.Create<FileUpload<byte[]>>());
        var singleLimitFile = UseState<FileUpload<byte[]>?>(() => null);
        var multipleLimitFiles = UseState(ImmutableArray.Create<FileUpload<byte[]>>());

        var limitedFilesUpload = UseUpload(MemoryStreamUploadHandler.Create(limitedFiles));
        var singleLimitFileUpload = UseUpload(MemoryStreamUploadHandler.Create(singleLimitFile));
        var multipleLimitFilesUpload = UseUpload(MemoryStreamUploadHandler.Create(multipleLimitFiles));

        return Layout.Vertical()
               | Text.H2("File Count Limits")
               | Text.P("Control the maximum number of files that can be selected using the MaxFiles property.")
               | (Layout.Grid().Columns(3)
                  | Text.Monospaced("Max Files")
                  | Text.Monospaced("Description")
                  | Text.Monospaced("File Input")

                  | Text.Block("No Limit")
                  | Text.Block("Default behavior - no restriction on number of files")
                  | limitedFiles.ToFileInput(limitedFilesUpload).Placeholder("Select unlimited files")

                  | Text.Block("1 File")
                  | Text.Block("Single file selection only")
                  | singleLimitFile.ToFileInput(singleLimitFileUpload).Placeholder("Select one file")

                  | Text.Block("3 Files")
                  | Text.Block("Maximum of 3 files allowed")
                  | multipleLimitFiles.ToFileInput(multipleLimitFilesUpload).MaxFiles(3).Placeholder("Select up to 3 files")
               );
    }
}

public class FileInputContentDisplay : ViewBase
{
    public override object? Build()
    {
        var singleLimitFile = UseState<FileUpload<byte[]>?>(() => null);
        var singleLimitFileUpload = UseUpload(MemoryStreamUploadHandler.Create(singleLimitFile));

        return Layout.Vertical()
               | Text.H2("File Content Display")
               | Text.P("Display file details and metadata after selection, showing information like file name, size, content type, and upload progress.")
               | (Layout.Grid().Columns(2)
                  | Text.Monospaced("File Input")
                  | Text.Monospaced("File Details")

                  | singleLimitFile.ToFileInput(singleLimitFileUpload).Placeholder("Select a text file to view content")
                  | (singleLimitFile.Value != null ? (object)singleLimitFile.Value.ToDetails() : Text.Block("No file selected"))

               // | singleFile.ToFileInput(singleFileUpload).Placeholder("Select a file to view as plain text")
               // | (singleFile.Value?.ToPlainText() ?? (object)Text.Block("No file selected"))
               );
    }
}

public class FileInputEventHandlersExample : ViewBase
{
    public override object? Build()
    {
        var files = UseState(ImmutableArray.Create<FileUpload<byte[]>>());
        var blurMessage = UseState("");
        var cancelCount = UseState(0);
        var blurCount = UseState(0);
        var upload = UseUpload(MemoryStreamUploadHandler.Create(files));

        return Layout.Vertical()
               | Text.H2("Event Handlers")
               | Text.P("Demonstrate OnBlur and OnCancel event handlers. OnBlur fires when the file dialog closes or input loses focus. OnCancel fires when the cancel button is clicked on a file.")
               | files.ToFileInput(upload)
                   .Placeholder("Choose files - try selecting, canceling the dialog, or clicking the X button")
                   .OnBlur((Event<IAnyInput> e) =>
                   {
                       blurCount.Set(blurCount.Value + 1);
                       if (files.Value.Length > 0)
                       {
                           blurMessage.Set($"Blur Event #{blurCount.Value}: {files.Value.Length} file(s) selected");
                       }
                       else
                           blurMessage.Set($"Blur Event #{blurCount.Value}: No file selected (dialog cancelled)");
                   })
                   .OnCancel((Guid fileId) =>
                   {
                       upload.Value.Cancel(fileId);
                       files.Set(list => list.Where(f => f.Id != fileId).ToImmutableArray());
                       cancelCount.Set(cancelCount.Value + 1);
                   })
               | Layout.Vertical().Gap(4)
                   | new Card(
                       Layout.Vertical().Gap(2)
                           | Text.P("The blur event fires when you close the file dialog or click away from the input.").Small()
                           | (blurMessage.Value != ""
                               ? Callout.Success(blurMessage.Value)
                               : Callout.Info("Interact with the file input above to see blur events"))
                   ).Title("OnBlur Handler")
                   | new Card(
                       Layout.Vertical().Gap(2)
                           | Text.P("The cancel event fires when you click the X button next to a file in the list.").Small()
                           | (files.Value.Length > 0
                               ? files.Value.ToTable()
                                   .Width(Size.Full())
                                   .Builder(e => e.FileName, e => e.Func((string x) => x))
                                   .Builder(e => e.Progress, e => e.Func((float x) => x.ToString("P0")))
                                   .Remove(e => e.Id)
                               : Text.P("Upload files above to see the cancel button (X) next to each file").Color(Colors.Muted))
                           | (cancelCount.Value > 0
                               ? Callout.Success($"Cancelled {cancelCount.Value} file(s)")
                               : files.Value.Length > 0
                                   ? Callout.Info("Click the X button next to any file to trigger the cancel event")
                                   : null)
                   ).Title("OnCancel Handler");
    }
}

public class FileInputIntegrationExample : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical()
               | Text.H2("Integration")
               | Text.P("Examples of integrating file inputs with forms and dialogs.")
               | Layout.Vertical().Gap(4)
                   | Text.H3("Form Integration")
                   | new SizingExample()
                   | Text.H3("Dialog Integration")
                   | new DialogFileUploadExample();
    }
}

public class SizingExample : ViewBase
{
    public record FileModel(FileUpload<byte[]>? ProfilePhoto, FileUpload<byte[]>? Document, FileUpload<byte[]>? Certificate);

    public override object? Build()
    {
        var profilePhoto = UseState<FileUpload<byte[]>?>(() => null);
        var document = UseState<FileUpload<byte[]>?>(() => null);
        var certificate = UseState<FileUpload<byte[]>?>(() => null);

        return Layout.Vertical()
            | new Card(
                Layout.Vertical().Gap(4)
                | Layout.Vertical().Gap(2)
                    | Text.Label("Profile Photo *")
                    | new ProfilePhotoUpload(profilePhoto)
                    | Text.Muted("Upload your profile picture")
                | Layout.Vertical().Gap(2)
                    | Text.Label("Document *")
                    | new DocumentUpload(document)
                    | Text.Muted("Upload your resume or document")
                | Layout.Vertical().Gap(2)
                    | Text.Label("Certificate")
                    | new CertificateUpload(certificate)
                    | Text.Muted("Upload certificate (optional)")
            )
            .Width(Size.Full())
            .Title("File Upload Form with Different Sizes");
    }
}

public class ProfilePhotoUpload(IState<FileUpload<byte[]>?> state) : ViewBase
{
    public override object Build()
    {
        var uploadCtx = UseUpload(MemoryStreamUploadHandler.Create(state));
        var uploadContext = uploadCtx
            .Accept("image/*")
            .MaxFileSize(5 * 1024 * 1024); // 5 MB
        const long maxSize = 5 * 1024 * 1024;
        return state.ToFileInput(uploadContext)
            .Large()
            .Placeholder($"Upload profile photo (max {StringHelper.FormatBytes(maxSize)})");
    }
}

public class DocumentUpload(IState<FileUpload<byte[]>?> state) : ViewBase
{
    public override object Build()
    {
        var uploadCtx = UseUpload(MemoryStreamUploadHandler.Create(state));
        var uploadContext = uploadCtx
            .Accept(".pdf,.doc,.docx")
            .MaxFileSize(10 * 1024 * 1024); // 10 MB
        const long maxSize = 10 * 1024 * 1024;
        return state.ToFileInput(uploadContext)
            .Placeholder($"Upload document (max {StringHelper.FormatBytes(maxSize)})");
    }
}

public class CertificateUpload(IState<FileUpload<byte[]>?> state) : ViewBase
{
    public override object Build()
    {
        var uploadCtx = UseUpload(MemoryStreamUploadHandler.Create(state));
        var uploadContext = uploadCtx
            .Accept(".pdf")
            .MaxFileSize(2 * 1024 * 1024); // 2 MB
        const long maxSize = 2 * 1024 * 1024;
        return state.ToFileInput(uploadContext)
            .Small()
            .Placeholder($"Upload certificate (max {StringHelper.FormatBytes(maxSize)})");
    }

}

public class DialogFileUploadExample : ViewBase
{
    public override object? Build()
    {
        var selectedFile = UseState<FileUpload<byte[]>?>();

        // Ephemeral state used inside the dialog while picking a file
        var dialogFile = UseState<FileUpload<byte[]>?>();
        // Dialog visibility state
        var isOpen = UseState(false);

        var uploadCtx = UseUpload(MemoryStreamUploadHandler.Create(dialogFile));
        var uploadContext = uploadCtx.Accept("*/*").MaxFileSize(10 * 1024 * 1024);



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
               | Text.P("Upload files through a dialog interface, allowing users to select files in a modal window.")
               | openButton
               | (selectedFile.Value != null
                    ? selectedFile.ToDetails()
                    : Text.P("No file selected"))
               | dialog;
    }
}

public class FileInputValidationExample : ViewBase
{
    public override object? Build()
    {
        var settings = UseState(new FileUploadValidationSettings());
        return Layout.Vertical()
               | Text.H2("Validation")
               | Text.P("Configure and test file upload validation rules including file size limits, file count limits, and accepted file types.")
               | (Layout.Horizontal()
                   | new FileUploadValidationUploader(settings.Value).Key(settings)
                   | settings.ToForm(submitTitle: "Update").WithLayout().Width(Size.Units(120)));
    }
}

public record FileUploadValidationSettings
{
    public long MaxFileSize { get; init; } = 5 * 1024 * 1024; // 5 MB

    public int MaxFiles { get; init; } = 3;

    public string? Accept { get; init; }

    public string Placeholder { get; init; } = "Choose files to upload";
}

public class FileUploadValidationUploader(FileUploadValidationSettings settings) : ViewBase
{
    public override object? Build()
    {
        var selectedFiles = UseState(ImmutableArray.Create<FileUpload<byte[]>>());
        var uploadCtx = UseUpload(MemoryStreamUploadHandler.Create(selectedFiles));
        var upload = uploadCtx
            .Accept(settings.Accept!)
            .MaxFileSize(settings.MaxFileSize)
            .MaxFiles(settings.MaxFiles);

        return Layout.Vertical()
                    | selectedFiles.ToFileInput(upload).Placeholder(settings.Placeholder)
                    | selectedFiles.Value.ToTable()
                        .Width(Size.Full())
                        .Builder(e => e.Length, e => e.Func((long x) => Ivy.StringHelper.FormatBytes(x)))
                        .Builder(e => e.Progress, e => e.Func((float x) => x.ToString("P0")))
                        .Remove(e => e.Id);
    }
}
