# FileInput

The `FileInput` widget allows users to upload files. It provides a file selector interface with options for file type filtering, size limitations, and support for single or multiple file selections.

## Basic Usage

Here's a simple example of a FileInput that allows users to select files:

```csharp demo-below
public class BasicFileUsageDemo : ViewBase 
{
    public override object? Build()
    {    
        var fileState = UseState((FileInput?)null);
        var uploadedStatus = UseState(fileState.Value?.Name); 
        return Layout.Vertical()
                | Text.Large("Upload your resume")
                | fileState.ToFileInput()
                           .Placeholder("Select a file")
                           .Accept(".pdf,.txt")
                           
                | Text.Small(uploadedStatus);
    }
}
```

```csharp 
var fileState = this.UseState((FileInput?)null);
new FileInput(fileState, placeholder: "Select a file", accept: ".txt,.pdf")
    .OnChange(e => Console.WriteLine($"File selected: {e.Value?.Name}"));
```

## Variants

The FileInput widget supports different variants to suit various use cases:

```csharp
Layout.Horizontal()
    | new FileInput(fileState, variant: FileInputs.Drop)
    | new FileInput(fileState, variant: FileInputs.Drop).Multiple()
```

## Event Handling

FileInput can handle file selection events using the `OnChange` parameter:

```csharp
var fileState = this.UseState((FileInput?)null);
new FileInput(fileState)
    .OnChange(e => Console.WriteLine($"File selected: {e.Value?.Name}"));
```

## Styling

FileInput can be customized with various styling options:

```csharp
new FileInput(fileState)
    .Placeholder("Select a file")
    .Accept(".jpg,.png")
    .Disabled(false)
```

<WidgetDocs Type="Ivy.FileInput" ExtensionTypes="Ivy.FileInputExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Inputs/FileInput.cs"/>

## Examples

### Multiple File Selection

```csharp
var filesState = this.UseState<IEnumerable<FileInput>?>(null);
new FileInput(filesState, multiple: true)
    .OnChange(e => Console.WriteLine($"Files selected: {string.Join(", ", e.Value?.Select(f => f.Name) ?? new string[0])}"));
``` 