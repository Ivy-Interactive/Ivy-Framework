using Ivy.Shared;

namespace Ivy.Samples.Apps.Widgets.Inputs;

[App(icon: Icons.Upload, path: ["Widgets", "Inputs"])]
public class FileInputApp : SampleBase
{
    protected override object? BuildSample()
    {
        // Mock file for 'With Value' example
        var mockFile = new FileInput
        {
            Name = "example.txt",
            Type = "text/plain",
            Size = 1234,
            LastModified = DateTime.Now,
            Content = null
        };

        var singleFile = UseState<FileInput?>(() => null);
        var singleFileWithValue = UseState<FileInput?>(() => mockFile);
        var multipleFiles = UseState<IEnumerable<FileInput>?>(() => null);
        var multipleFilesWithValue = UseState<IEnumerable<FileInput>?>(() => new[] { mockFile });
        var disabledFile = UseState<FileInput?>(() => null);
        var invalidFile = UseState<FileInput?>(() => null);
        var placeholderFile = UseState<FileInput?>(() => null);
        var limitedFiles = UseState<IEnumerable<FileInput>?>(() => null);
        var textFiles = UseState<IEnumerable<FileInput>?>(() => null);
        var pdfFiles = UseState<IEnumerable<FileInput>?>(() => null);
        var imageFiles = UseState<IEnumerable<FileInput>?>(() => null);

        var onChangedState = UseState<FileInput?>(() => null);
        var onChangeLabel = UseState("");
        var onBlurState = UseState<FileInput?>(() => null);
        var onBlurLabel = UseState("");

        // Validation examples
        var validationError = UseState<string?>(() => null);
        var validatedFiles = UseState<IEnumerable<FileInput>?>(() => null);
        var singleFileWithValidation = UseState<FileInput?>(() => null);

        var dataBinding = Layout.Grid().Columns(3)
                          | Text.InlineCode("FileInput")
                          | (Layout.Vertical()
                             | singleFile.ToFileInput()
                             | singleFile.ToFileInput()
                          )
                          | singleFile

                          | Text.InlineCode("FileInput?")
                          | (Layout.Vertical()
                             | singleFile.ToFileInput()
                             | singleFile.ToFileInput()
                          )
                          | singleFile

                          | Text.InlineCode("IEnumerable<FileInput>")
                          | (Layout.Vertical()
                             | multipleFiles.ToFileInput()
                          )
                          | multipleFiles
            ;

        return Layout.Vertical()
               | Text.H1("File Inputs")
               | Text.H2("Variants")
               | (Layout.Grid().Columns(6)
                  | null!
                  | Text.InlineCode("Empty")
                  | Text.InlineCode("With Value")
                  | Text.InlineCode("Disabled")
                  | Text.InlineCode("Invalid")
                  | Text.InlineCode("With Placeholder")

                  | Text.InlineCode("Single File")
                  | singleFile.ToFileInput()
                  | singleFileWithValue.ToFileInput()
                  | singleFile.ToFileInput().Disabled()
                  | singleFile.ToFileInput().Invalid("Please select a valid file")
                  | placeholderFile.ToFileInput().Placeholder("Click to select a file")

                  | Text.InlineCode("Multiple Files")
                  | multipleFiles.ToFileInput()
                  | multipleFilesWithValue.ToFileInput()
                  | multipleFiles.ToFileInput().Disabled()
                  | multipleFiles.ToFileInput().Invalid("Please select valid files")
                  | multipleFiles.ToFileInput().Placeholder("Click to select files")
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
                  | textFiles.ToFileInput().Accept(".txt,.md,.csv").Placeholder("Select text files")

                  | Text.Block("PDF Files")
                  | Text.InlineCode(".pdf")
                  | pdfFiles.ToFileInput().Accept(".pdf").Placeholder("Select PDF files")

                  | Text.Block("Images")
                  | Text.InlineCode(".jpg,.jpeg,.png,.gif,.webp")
                  | imageFiles.ToFileInput().Accept(".jpg,.jpeg,.png,.gif,.webp").Placeholder("Select image files")

                  | Text.Block("All Files")
                  | Text.InlineCode("(default)")
                  | singleFile.ToFileInput().Placeholder("Select any file")
               )

               // File Count Limits:
               | Text.H2("File Count Limits")
               | (Layout.Grid().Columns(3)
                  | Text.InlineCode("Max Files")
                  | Text.InlineCode("Description")
                  | Text.InlineCode("File Input")

                  | Text.Block("No Limit")
                  | Text.Block("Default behavior - no restriction on number of files")
                  | multipleFiles.ToFileInput().Placeholder("Select unlimited files")

                  | Text.Block("1 File")
                  | Text.Block("Single file selection only")
                  | singleFile.ToFileInput().Placeholder("Select one file")

                  | Text.Block("3 Files")
                  | Text.Block("Maximum of 3 files allowed")
                  | multipleFiles.ToFileInput().MaxFiles(3).Placeholder("Select up to 3 files")

                  | Text.Block("5 Files")
                  | Text.Block("Maximum of 5 files allowed")
                  | multipleFiles.ToFileInput().MaxFiles(5).Placeholder("Select up to 5 files")
               )

               // Events: 
               | Text.H2("Events")
               | Text.H3("OnChange")
               | Layout.Horizontal(
                   new FileInput<FileInput?>(onChangedState.Value, e =>
                   {
                       onChangedState.Set(e.Value);
                       onChangeLabel.Set("Changed");
                   }),
                   onChangeLabel
               )
               | Text.H3("OnBlur")
               | Layout.Horizontal(
                   onBlurState.ToFileInput().HandleBlur(e => onBlurLabel.Set("Blur")),
                   onBlurLabel
               )

               // File Content Display:
               | Text.H2("File Content Display")
               | (Layout.Grid().Columns(2)
                  | Text.InlineCode("File Input")
                  | Text.InlineCode("File Details")

                  | singleFile.ToFileInput().Placeholder("Select a text file to view content")
                  | (singleFile.Value != null ? (object)singleFile.ToDetails().Remove(e => e!.Content) : Text.Block("No file selected"))

                  | singleFile.ToFileInput().Placeholder("Select a file to view as plain text")
                  | (singleFile.Value?.ToPlainText() ?? (object)Text.Block("No file selected"))
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
                  | singleFileWithValidation.ToFileInput().Accept(".txt,.pdf").Placeholder("Select .txt or .pdf file")

                  | Text.Block("Multiple files with count and type validation")
                  | validatedFiles.ToFileInput().MaxFiles(3).Accept("image/*").Placeholder("Select up to 3 image files")
               )
            ;
    }
}