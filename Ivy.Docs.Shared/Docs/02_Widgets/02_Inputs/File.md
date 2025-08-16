# FileInput

<Ingress>
Enable file uploads with a flexible interface supporting type filtering, size limits, and single or multiple file selection.
</Ingress>

The `FileInput` widget allows users to upload files. It provides a file selector interface with options for file type filtering, size limitations, and support for single or multiple file selections.

## Basic Usage

Here's a simple example of a `FileInput` that allows users to select files:

```csharp demo-below 
public class BasicFileInputDemo : ViewBase
{
    public override object? Build()
    {    
        var fileState = this.UseState((FileInput?)null);
        var selected = fileState.Value?.Name;
        return Layout.Vertical()
                | fileState.ToFileInput()
                           .Placeholder("Select a file")
                           .Accept(".txt,.pdf,.cs")
                | Text.Large(selected);                    
   }     
}    
```

To create a file upload input, `ToFileInput` is the recommended function.

## Variants

The `FileInput` widget supports different variants to suit various use cases. It has a variant
where users can select a single file or multiple files and drag and drop them in the file upload
section. The following demo showcases this.

```csharp demo-below 
public class FileDropDemo : ViewBase
{    
    public override object? Build()
    {    
        var fileState = this.UseState((FileInput?)null);
        var fileStates = this.UseState((FileInput?)null);
        return  Layout.Vertical()
                | fileState.ToFileInput().Variant(FileInputs.Drop)
                | fileStates.ToFileInput().Variant(FileInputs.Drop).Multiple;
    }
}    
         
```

## Styling

`FileInput` can be customized with various styling options:

### Disabled

To render a disabled `FileInput` control, the `Disabled` function should be used.

```csharp
public class FileInputDisabledDemo : ViewBase
{
    public override object? Build()
    {
        var fileState = this.UseState((FileInput?)null);
         return Layout.Vertical()
                |  fileState.ToFileInput()
                    .Placeholder("Select a file")
                    .Accept(".jpg,.png")
                    .Disabled();
    }
}    
```

<WidgetDocs Type="Ivy.FileInput" ExtensionTypes="Ivy.FileInputExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Inputs/FileInput.cs"/>

## Examples

### Multiple File Selection

```csharp demo-below 
public class MultiFileSelectionDemo : ViewBase
{
    public override object? Build()
    {    
        
        var filesState = UseState<IEnumerable<FileInput>>([]);
        var selected = UseState("");
        if(filesState.Value.Count() > 0)
        {
            selected.Set($"Files selected: {string.Join(", ", filesState.Value?.Select(f => f.Name) ?? new string[0])}");
        }   
        return Layout.Vertical()
                |  filesState.ToFileInput()
                |  Text.Large(selected);
    }
}

```
