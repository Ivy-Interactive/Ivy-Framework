---
searchHints:
  - content-input
  - text-with-files
  - attachment
  - drag-drop
  - paste
  - compose
  - message-input
  - file-attachment
---

# ContentInput

<Ingress>
A textarea with integrated file attachment support via drag-and-drop, clipboard paste, and file picker — similar to GitHub issue editors or chat message inputs.
</Ingress>

The `ContentInput` widget combines a text area with file attachment capabilities. Users can type text while also attaching files by dragging them onto the input, pasting from the clipboard (e.g., screenshots via Ctrl+V/Cmd+V), or using the built-in file picker button.

## Basic Usage

```csharp
var text = UseState("");
var files = UseState(ImmutableArray<FileUpload<byte[]>>.Empty);
var upload = UseUpload(MemoryStreamUploadHandler.Create(files));

text.ToContentInput(upload)
    .Files(files.Value)
    .Placeholder("Type a message... (paste or drag files here)");
```

## File Restrictions

Control which files can be attached using `Accept`, `MaxFileSize`, and `MaxFiles`:

```csharp
text.ToContentInput(upload)
    .Files(files.Value)
    .Accept("image/*,.pdf")
    .MaxFileSize(10 * 1024 * 1024)  // 10 MB
    .MaxFiles(5)
    .Placeholder("Describe the issue... (paste screenshots or drag files)");
```

## Configuration

| Method | Description |
|--------|-------------|
| `.Placeholder(string)` | Placeholder text for the textarea |
| `.Rows(int)` | Number of visible text rows |
| `.MaxLength(int)` | Maximum character count |
| `.Accept(string)` | Accepted file types (e.g., `"image/*,.pdf"`) |
| `.MaxFileSize(long)` | Maximum file size in bytes |
| `.MaxFiles(int)` | Maximum number of attachments |
| `.Files(IEnumerable<FileUpload>)` | Display current file attachments |
| `.Disabled(bool)` | Disable the input |
| `.Invalid(string)` | Show validation error |

## Events

| Method | Description |
|--------|-------------|
| `.OnBlur(...)` | Fires when the component loses focus |
| `.OnFocus(...)` | Fires when the component gains focus |
| `.OnSubmit(...)` | Fires on Ctrl+Enter / Cmd+Enter |
| `.OnCancel(...)` | Fires when a file attachment is removed |

<Samples type="ContentInputApp" />
