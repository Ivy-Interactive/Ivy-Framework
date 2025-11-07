using Ivy.Hooks;
using Ivy.Services;
using Ivy.Shared;
using Ivy.Views.Builders;
using Ivy.Views.Forms;

namespace Ivy.Samples.Shared.Apps.Widgets.Inputs;

[App(icon: Icons.Upload, path: ["Widgets", "Inputs"], searchHints: ["upload", "file", "attachment", "drag-drop", "browse", "files"])]
public class FileInputApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Vertical()
               | Text.H1("File Inputs")
               | Layout.Tabs(
                   new Tab("Variants", new FileInputVariants()),
                   new Tab("Size Variants", new FileInputSizeVariants()),
                   new Tab("Data Binding", new FileInputDataBinding()),
                   new Tab("File Type Restrictions", new FileInputTypeRestrictions()),
                   new Tab("File Count Limits", new FileInputCountLimits()),
                   new Tab("File Content Display", new FileInputContentDisplay()),
                   new Tab("Form Example", new FileInputFormExample())
               ).Variant(TabsVariant.Content);
    }
}

public class FileInputVariants : ViewBase
{
    public override object? Build()
    {
        var singleFile = UseState<FileUpload<byte[]>?>(() => null);
        var multipleFiles = UseState(ImmutableArray.Create<FileUpload<byte[]>>());
        var placeholderFile = UseState<FileUpload<byte[]>?>(() => null);

        var singleFileUpload = this.UseUpload(MemoryStreamUploadHandler.Create(singleFile));
        var multipleFilesUpload = this.UseUpload(MemoryStreamUploadHandler.Create(multipleFiles));
        var placeholderFileUpload = this.UseUpload(MemoryStreamUploadHandler.Create(placeholderFile));

        return Layout.Vertical()
               | Text.H2("Variants")
               | Text.P("Demonstrate different visual states of file inputs including empty, with value, disabled, invalid, and with placeholder.")
               | (Layout.Grid().Columns(6)
                  | null!
                  | Text.InlineCode("Empty")
                  | Text.InlineCode("With Value")
                  | Text.InlineCode("Disabled")
                  | Text.InlineCode("Invalid")
                  | Text.InlineCode("With Placeholder")

                  | Text.InlineCode("Single File")
                  | singleFile.ToFileInput(singleFileUpload)
                  | singleFile.ToFileInput(singleFileUpload).Placeholder("File input with value (upload a file)")
                  | singleFile.ToFileInput(singleFileUpload).Disabled()
                  | singleFile.ToFileInput(singleFileUpload).Invalid("Please select a valid file")
                  | placeholderFile.ToFileInput(placeholderFileUpload).Placeholder("Click to select a file")

                  | Text.InlineCode("Multiple Files")
                  | multipleFiles.ToFileInput(multipleFilesUpload)
                  | multipleFiles.ToFileInput(multipleFilesUpload).Placeholder("Multiple files with value (upload files)")
                  | multipleFiles.ToFileInput(multipleFilesUpload).Disabled()
                  | multipleFiles.ToFileInput(multipleFilesUpload).Invalid("Please select valid files")
                  | multipleFiles.ToFileInput(multipleFilesUpload).Placeholder("Click to select files")
               );
    }
}

public class FileInputSizeVariants : ViewBase
{
    public override object? Build()
    {
        var singleSizeFile = UseState<FileUpload<byte[]>?>(() => null);
        var multipleSizeFiles = UseState(ImmutableArray.Create<FileUpload<byte[]>>());

        var singleSizeFileUpload = this.UseUpload(MemoryStreamUploadHandler.Create(singleSizeFile));
        var multipleSizeFilesUpload = this.UseUpload(MemoryStreamUploadHandler.Create(multipleSizeFiles));

        return Layout.Vertical()
               | Text.H2("Size Variants")
               | Text.P("File inputs support different sizes: Small, Medium (default), and Large for both single and multiple file selection.")
               | (Layout.Grid().Columns(4)
                  | null!
                  | Text.InlineCode("Small")
                  | Text.InlineCode("Medium")
                  | Text.InlineCode("Large")

                  | Text.InlineCode("Single File")
                  | singleSizeFile.ToFileInput(singleSizeFileUpload).Small().Placeholder("Small file input")
                  | singleSizeFile.ToFileInput(singleSizeFileUpload).Placeholder("Medium file input")
                  | singleSizeFile.ToFileInput(singleSizeFileUpload).Large().Placeholder("Large file input")

                  | Text.InlineCode("Multiple Files")
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
        var dataBindingFileUpload = this.UseUpload(MemoryStreamUploadHandler.Create(dataBindingFile));

        var dataBindingNullableFile = UseState<FileUpload<byte[]>?>(() => null);
        var dataBindingNullableFileUpload = this.UseUpload(MemoryStreamUploadHandler.Create(dataBindingNullableFile));

        var dataBindingMultipleFiles = UseState(ImmutableArray.Create<FileUpload<byte[]>>());
        var dataBindingMultipleFilesUpload = this.UseUpload(MemoryStreamUploadHandler.Create(dataBindingMultipleFiles));

        var dataBinding = Layout.Grid().Columns(3)
                          | Text.InlineCode("FileInput")
                          | dataBindingFile.ToFileInput(dataBindingFileUpload)
                              .Placeholder("Single file: FileUpload<byte[]>? (nullable single file binding)")
                          | dataBindingFile

                          | Text.InlineCode("FileInput?")
                          | dataBindingNullableFile.ToFileInput(dataBindingNullableFileUpload)
                              .Placeholder("Single file: FileUpload<byte[]>? (explicitly nullable binding)")
                          | dataBindingNullableFile

                          | Text.InlineCode("IEnumerable<FileInput>")
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

        var textFilesUpload = this.UseUpload(MemoryStreamUploadHandler.Create(textFiles));
        var pdfFilesUpload = this.UseUpload(MemoryStreamUploadHandler.Create(pdfFiles));
        var imageFilesUpload = this.UseUpload(MemoryStreamUploadHandler.Create(imageFiles));
        var singleLimitFileUpload = this.UseUpload(MemoryStreamUploadHandler.Create(singleLimitFile));

        return Layout.Vertical()
               | Text.H2("File Type Restrictions")
               | Text.P("Restrict file selection to specific types using the Accept property with file extensions or MIME types.")
               | (Layout.Grid().Columns(3)
                  | Text.InlineCode("File Type")
                  | Text.InlineCode("Accept Property")
                  | Text.InlineCode("File Input")

                  | Text.Block("Text Files")
                  | Text.InlineCode(".txt,.md,.csv")
                  | textFiles.ToFileInput(textFilesUpload).Accept(".txt,.md,.csv").Placeholder("Select text files")

                  | Text.Block("PDF Files")
                  | Text.InlineCode(".pdf")
                  | pdfFiles.ToFileInput(pdfFilesUpload).Accept(".pdf").Placeholder("Select PDF files")

                  | Text.Block("Images")
                  | Text.InlineCode(".jpg,.jpeg,.png,.gif,.webp")
                  | imageFiles.ToFileInput(imageFilesUpload).Accept(".jpg,.jpeg,.png,.gif,.webp").Placeholder("Select image files")

                  | Text.Block("All Files")
                  | Text.InlineCode("(default)")
                  | singleLimitFile.ToFileInput(singleLimitFileUpload).Placeholder("Select any file")
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

        var limitedFilesUpload = this.UseUpload(MemoryStreamUploadHandler.Create(limitedFiles));
        var singleLimitFileUpload = this.UseUpload(MemoryStreamUploadHandler.Create(singleLimitFile));
        var multipleLimitFilesUpload = this.UseUpload(MemoryStreamUploadHandler.Create(multipleLimitFiles));

        return Layout.Vertical()
               | Text.H2("File Count Limits")
               | Text.P("Control the maximum number of files that can be selected using the MaxFiles property.")
               | (Layout.Grid().Columns(3)
                  | Text.InlineCode("Max Files")
                  | Text.InlineCode("Description")
                  | Text.InlineCode("File Input")

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
        var singleLimitFileUpload = this.UseUpload(MemoryStreamUploadHandler.Create(singleLimitFile));

        return Layout.Vertical()
               | Text.H2("File Content Display")
               | Text.P("Display file details and metadata after selection, showing information like file name, size, content type, and upload progress.")
               | (Layout.Grid().Columns(2)
                  | Text.InlineCode("File Input")
                  | Text.InlineCode("File Details")

                  | singleLimitFile.ToFileInput(singleLimitFileUpload).Placeholder("Select a text file to view content")
                  | (singleLimitFile.Value != null ? (object)singleLimitFile.Value.ToDetails() : Text.Block("No file selected"))

               // | singleFile.ToFileInput(singleFileUpload).Placeholder("Select a file to view as plain text")
               // | (singleFile.Value?.ToPlainText() ?? (object)Text.Block("No file selected"))
               );
    }
}

public class FileInputFormExample : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical()
               | Text.H2("Form Example")
               | Text.P("Example of integrating file inputs into a form with different sizes, file type restrictions, and size limits.")
               | new SizingExample();
    }
}

public class SizingExample : ViewBase
{
    public record FileModel(FileUpload<byte[]>? ProfilePhoto, FileUpload<byte[]>? Document, FileUpload<byte[]>? Certificate);

    public override object? Build()
    {
        var fileModel = UseState(() => new FileModel(null, null, null));

        // Profile Photo: 5 MB max, Large size
        const long profilePhotoMaxSize = 5 * 1024 * 1024; // 5 MB

        // Document: 10 MB max, Medium size (default)
        const long documentMaxSize = 10 * 1024 * 1024; // 10 MB

        // Certificate: 2 MB max, Small size
        const long certificateMaxSize = 2 * 1024 * 1024; // 2 MB

        return Layout.Vertical()
            | new Card(
                fileModel.ToForm()
                    .Builder(m => m.ProfilePhoto, (state, view) =>
                    {
                        var uploadContext = view.UseUpload(MemoryStreamUploadHandler.Create(state))
                            .Accept("image/*")
                            .MaxFileSize(profilePhotoMaxSize);
                        return state.ToFileInput(uploadContext)
                            .Large()
                            .Placeholder($"Upload profile photo (max {Utils.FormatBytes(profilePhotoMaxSize)})");
                    })
                    .Builder(m => m.Document, (state, view) =>
                    {
                        var uploadContext = view.UseUpload(MemoryStreamUploadHandler.Create(state))
                            .Accept(".pdf,.doc,.docx")
                            .MaxFileSize(documentMaxSize);
                        return state.ToFileInput(uploadContext)
                            .Placeholder($"Upload document (max {Utils.FormatBytes(documentMaxSize)})");
                    })
                    .Builder(m => m.Certificate, (state, view) =>
                    {
                        var uploadContext = view.UseUpload(MemoryStreamUploadHandler.Create(state))
                            .Accept(".pdf")
                            .MaxFileSize(certificateMaxSize);
                        return state.ToFileInput(uploadContext)
                            .Small()
                            .Placeholder($"Upload certificate (max {Utils.FormatBytes(certificateMaxSize)})");
                    })
                    .Label(m => m.ProfilePhoto, "Profile Photo")
                    .Label(m => m.Document, "Document")
                    .Label(m => m.Certificate, "Certificate")
                    .Description(m => m.ProfilePhoto, "Upload your profile picture")
                    .Description(m => m.Document, "Upload your resume or document")
                    .Description(m => m.Certificate, "Upload certificate (optional)")
                    .Required(m => m.ProfilePhoto, m => m.Document)
            )
            .Width(Size.Full())
            .Title("File Upload Form with Different Sizes");
    }
}