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
        // Mock file for 'With Value' example
        var mockFile = new FileUpload { FileName = "example.txt", ContentType = "text/plain", Length = 12345 };

        var singleFile = UseState<FileUpload?>(() => null);
        var singleFileWithValue = UseState<FileUpload?>(() => mockFile);
        var multipleFiles = UseState<IEnumerable<FileUpload>?>(() => null);
        var multipleFilesWithValue = UseState<IEnumerable<FileUpload>?>(() => new[] { mockFile });
        var disabledFile = UseState<FileUpload?>(() => null);
        var invalidFile = UseState<FileUpload?>(() => null);
        var placeholderFile = UseState<FileUpload?>(() => null);
        var limitedFiles = UseState<IEnumerable<FileUpload>?>(() => null);
        var textFiles = UseState<IEnumerable<FileUpload>?>(() => null);
        var pdfFiles = UseState<IEnumerable<FileUpload>?>(() => null);
        var imageFiles = UseState<IEnumerable<FileUpload>?>(() => null);
        var singleSizeFile = UseState<FileUpload?>(() => null);
        var multipleSizeFiles = UseState<IEnumerable<FileUpload>?>(() => null);

        var onBlurState = UseState<FileUpload?>(() => null);
        var onBlurLabel = UseState("");

        // Validation examples
        var validationError = UseState<string?>(() => null);
        var validatedFiles = UseState<IEnumerable<FileUpload>?>(() => null);
        var singleFileWithValidation = UseState<FileUpload?>(() => null);

        // Upload contexts (using simple lambda handlers for demo purposes)
        var singleFileUpload = this.UseUpload((fileUpload, stream, cancellationToken) => System.Threading.Tasks.Task.CompletedTask);
        var singleFileWithValueUpload = this.UseUpload((fileUpload, stream, cancellationToken) => System.Threading.Tasks.Task.CompletedTask);
        var multipleFilesUpload = this.UseUpload((fileUpload, stream, cancellationToken) => System.Threading.Tasks.Task.CompletedTask);
        var multipleFilesWithValueUpload = this.UseUpload((fileUpload, stream, cancellationToken) => System.Threading.Tasks.Task.CompletedTask);
        var placeholderFileUpload = this.UseUpload((fileUpload, stream, cancellationToken) => System.Threading.Tasks.Task.CompletedTask);
        var textFilesUpload = this.UseUpload((fileUpload, stream, cancellationToken) => System.Threading.Tasks.Task.CompletedTask);
        var pdfFilesUpload = this.UseUpload((fileUpload, stream, cancellationToken) => System.Threading.Tasks.Task.CompletedTask);
        var imageFilesUpload = this.UseUpload((fileUpload, stream, cancellationToken) => System.Threading.Tasks.Task.CompletedTask);
        var singleSizeFileUpload = this.UseUpload((fileUpload, stream, cancellationToken) => System.Threading.Tasks.Task.CompletedTask);
        var multipleSizeFilesUpload = this.UseUpload((fileUpload, stream, cancellationToken) => System.Threading.Tasks.Task.CompletedTask);
        var onBlurUpload = this.UseUpload((fileUpload, stream, cancellationToken) => System.Threading.Tasks.Task.CompletedTask);
        var validatedFilesUpload = this.UseUpload((fileUpload, stream, cancellationToken) => System.Threading.Tasks.Task.CompletedTask);
        var singleFileWithValidationUpload = this.UseUpload((fileUpload, stream, cancellationToken) => System.Threading.Tasks.Task.CompletedTask);

        var dataBinding = Layout.Grid().Columns(3)
                          | Text.InlineCode("FileInput")
                          | (Layout.Vertical()
                             | singleFile.ToFileInput(singleFileUpload)
                             | singleFile.ToFileInput(singleFileUpload)
                          )
                          | singleFile

                          | Text.InlineCode("FileInput?")
                          | (Layout.Vertical()
                             | singleFile.ToFileInput(singleFileUpload)
                             | singleFile.ToFileInput(singleFileUpload)
                          )
                          | singleFile

                          | Text.InlineCode("IEnumerable<FileInput>")
                          | (Layout.Vertical()
                             | multipleFiles.ToFileInput(multipleFilesUpload)
                          )
                          | multipleFiles
            ;

        return Layout.Vertical()
               | Text.H1("File Inputs")

               // Size Variants:
               | Text.H2("Size Variants")
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
               )

               | Text.H2("Variants")
               | (Layout.Grid().Columns(6)
                  | null!
                  | Text.InlineCode("Empty")
                  | Text.InlineCode("With Value")
                  | Text.InlineCode("Disabled")
                  | Text.InlineCode("Invalid")
                  | Text.InlineCode("With Placeholder")

                  | Text.InlineCode("Single File")
                  | singleFile.ToFileInput(singleFileUpload)
                  | singleFileWithValue.ToFileInput(singleFileWithValueUpload)
                  | singleFile.ToFileInput(singleFileUpload).Disabled()
                  | singleFile.ToFileInput(singleFileUpload).Invalid("Please select a valid file")
                  | placeholderFile.ToFileInput(placeholderFileUpload).Placeholder("Click to select a file")

                  | Text.InlineCode("Multiple Files")
                  | multipleFiles.ToFileInput(multipleFilesUpload)
                  | multipleFilesWithValue.ToFileInput(multipleFilesWithValueUpload)
                  | multipleFiles.ToFileInput(multipleFilesUpload).Disabled()
                  | multipleFiles.ToFileInput(multipleFilesUpload).Invalid("Please select valid files")
                  | multipleFiles.ToFileInput(multipleFilesUpload).Placeholder("Click to select files")
               )

               // Data Binding:
               | Text.H2("Data Binding")
               | dataBinding

               // File Type Restrictions:
               | Text.H2("File Type Restrictions")
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
                  | singleFile.ToFileInput(singleFileUpload).Placeholder("Select any file")
               )

               // File Count Limits:
               | Text.H2("File Count Limits")
               | (Layout.Grid().Columns(3)
                  | Text.InlineCode("Max Files")
                  | Text.InlineCode("Description")
                  | Text.InlineCode("File Input")

                  | Text.Block("No Limit")
                  | Text.Block("Default behavior - no restriction on number of files")
                  | multipleFiles.ToFileInput(multipleFilesUpload).Placeholder("Select unlimited files")

                  | Text.Block("1 File")
                  | Text.Block("Single file selection only")
                  | singleFile.ToFileInput(singleFileUpload).Placeholder("Select one file")

                  | Text.Block("3 Files")
                  | Text.Block("Maximum of 3 files allowed")
                  | multipleFiles.ToFileInput(multipleFilesUpload).MaxFiles(3).Placeholder("Select up to 3 files")

                  | Text.Block("5 Files")
                  | Text.Block("Maximum of 5 files allowed")
                  | multipleFiles.ToFileInput(multipleFilesUpload).MaxFiles(5).Placeholder("Select up to 5 files")
               )

               // Events:
               | Text.H2("Events")
               | Text.H3("OnBlur")
               | Layout.Horizontal(
                   onBlurState.ToFileInput(onBlurUpload).HandleBlur(e => onBlurLabel.Set("Blur")),
                   onBlurLabel
               )

               // File Content Display:
               | Text.H2("File Content Display")
               | (Layout.Grid().Columns(2)
                  | Text.InlineCode("File Input")
                  | Text.InlineCode("File Details")

                  | singleFile.ToFileInput(singleFileUpload).Placeholder("Select a text file to view content")
                  | (singleFile.Value != null ? (object)singleFile.ToDetails() : Text.Block("No file selected"))

               // | singleFile.ToFileInput(singleFileUpload).Placeholder("Select a file to view as plain text")
               // | (singleFile.Value?.ToPlainText() ?? (object)Text.Block("No file selected"))
               )

               // Backend Validation:
               | Text.H2("Backend Validation")
               | Text.P("The backend provides validation methods that can be used to validate files against Accept patterns and MaxFiles limits:")
               | (Layout.Grid().Columns(2)
                  | Text.InlineCode("Validation Method")
                  | Text.InlineCode("Usage Example")

                  | Text.Block("Validate Single File")
                  | Text.Code("var validation = fileInput.ValidateFile(file);\nif (!validation.IsValid) {\n    // Handle error\n}")

                  | Text.Block("Validate Multiple Files")
                  | Text.Code("var validation = fileInput.ValidateFiles(files);\nif (!validation.IsValid) {\n    // Handle error\n}")

                  | Text.Block("Validate Any Value")
                  | Text.Code("var validation = fileInput.ValidateValue(value);\nif (!validation.IsValid) {\n    // Handle error\n}")

                  | Text.Block("Supported Patterns")
                  | Text.Code(".txt,.pdf          // File extensions\nimage/*           // MIME type wildcards\ntext/plain        // Exact MIME types")
               )

               // Automatic Validation Examples:
               | Text.H2("Automatic Validation Examples")
               | Text.P("FileInput automatically validates files when Accept or MaxFiles is set:")
               | (Layout.Grid().Columns(2)
                  | Text.InlineCode("Description")
                  | Text.InlineCode("File Input")

                  | Text.Block("Single file with type validation")
                  | singleFileWithValidation.ToFileInput(singleFileWithValidationUpload).Accept(".txt,.pdf").Placeholder("Select .txt or .pdf file")

                  | Text.Block("Multiple files with count and type validation")
                  | validatedFiles.ToFileInput(validatedFilesUpload).MaxFiles(3).Accept("image/*").Placeholder("Select up to 3 image files")
               )

               // File Upload Form with Different Sizes:
               | Text.H2("File Upload Form with Different Sizes")
               | new SizingExample()
            ;
    }
}

public class SizingExample : ViewBase
{
    public record FileModel(FileUpload? ProfilePhoto, FileUpload? Document, FileUpload? Certificate);

    public override object? Build()
    {
        var fileModel = UseState(() => new FileModel(null, null, null));

        var profilePhotoUpload = this.UseUpload((fileUpload, stream, cancellationToken) => System.Threading.Tasks.Task.CompletedTask);
        var documentUpload = this.UseUpload((fileUpload, stream, cancellationToken) => System.Threading.Tasks.Task.CompletedTask);
        var certificateUpload = this.UseUpload((fileUpload, stream, cancellationToken) => System.Threading.Tasks.Task.CompletedTask);

        return Layout.Vertical()
            | new Card(
                fileModel.ToForm()
                    .Builder(m => m.ProfilePhoto, s => s.ToFileInput(profilePhotoUpload).Large().Accept("image/*"))
                    .Builder(m => m.Document, s => s.ToFileInput(documentUpload).Accept(".pdf,.doc,.docx"))
                    .Builder(m => m.Certificate, s => s.ToFileInput(certificateUpload).Small().Accept(".pdf"))
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