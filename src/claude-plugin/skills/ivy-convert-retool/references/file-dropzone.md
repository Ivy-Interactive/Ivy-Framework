# File Dropzone

A drag-and-drop area to select single or multiple files with built-in validation, progress tracking, and file parsing support.

## Retool

```js
// Access uploaded file data
fileDropzone.value        // array of uploaded files (base64)
fileDropzone.files        // array of file metadata (name, size, type, lastModified)
fileDropzone.parsedValue  // parsed plain-text content

// Methods
fileDropzone.clearValue();
fileDropzone.resetValue();
fileDropzone.setDisabled(true);
fileDropzone.setHidden(false);
fileDropzone.focus();
fileDropzone.clearValidation();
fileDropzone.scrollIntoView({ behavior: 'smooth', block: 'center' });
```

## Ivy

```csharp
// Single file upload (in-memory)
var fileState = UseState<FileUpload<byte[]>?>();
var upload = UseUpload(MemoryStreamUploadHandler.Create(fileState));
return fileState.ToFileInput(upload)
    .Placeholder("Drop a file here")
    .Accept("image/*,.pdf")
    .MaxFileSize(10_000_000);

// Multiple file upload
var filesState = UseState(ImmutableArray<FileUpload<byte[]>>.Empty);
var upload = UseUpload(MemoryStreamUploadHandler.Create(filesState));
return filesState.ToFileInput(upload)
    .Placeholder("Drop files here")
    .MaxFiles(5);

// Access uploaded file data
fileState.Value.FileName    // original file name
fileState.Value.ContentType // MIME type
fileState.Value.Length      // file size in bytes
fileState.Value.Progress    // upload progress 0.0–1.0
fileState.Value.Content     // file data as byte[]
fileState.Value.Status      // Pending, Uploading, Completed, Failed, Aborted
```

## Parameters

| Parameter | Retool | Ivy |
|-----------|--------|-----|
| `accept` | Array of file extensions or MIME types to accept. `["image/*", "pdf", "doc"]` | `Accept` — string of MIME types or extensions. `"image/*,.pdf,.doc"` |
| `disabled` | `boolean` — prevents user interaction. | `Disabled` — `bool`, disables the file input. |
| `placeholder` | `string` — text displayed when no value exists. | `Placeholder` — `string`, placeholder text. |
| `selectionType` | `"single"`, `"multiple"`, or `"directory"`. | `Multiple` — `bool`, auto-enabled when using `ImmutableArray<FileUpload<T>>`. Directory mode not supported. |
| `maxCount` | `number` — maximum number of files allowed. | `MaxFiles` — `int?`, maximum number of files. |
| `maxSize` | `number` — maximum file size in bytes. | `MaxFileSize` — `long?`, maximum file size in bytes. |
| `minCount` | `number` — minimum number of files required. | Not supported |
| `minSize` | `number` — minimum file size in bytes. | Not supported |
| `value` | Read-only array of uploaded file data (base64). | `Value` — `TValue`, current uploaded file(s) with typed content (`byte[]` or `string`). |
| `files` | Read-only array of file metadata (name, size, type, lastModified, uploading). | File metadata available via `FileUpload<T>` record properties: `FileName`, `ContentType`, `Length`, `Progress`, `Status`. |
| `parseFiles` | `boolean` — parse plain-text file content. | Not supported (handle content parsing in application code) |
| `parsedValue` | Read-only parsed plain-text content. | Not supported |
| `parsing` | Read-only `boolean` — whether content is being parsed. | Not supported (use `FileUpload.Status` and `Progress` for upload state) |
| `appendNewSelection` | `boolean` — append additional files to existing selection. | Not supported (state management handles accumulation) |
| `loading` | `boolean` — display a loading indicator. | Not supported |
| `iconBefore` | `string` — prefix icon to display. | Not supported |
| `labelPosition` | `"top"` or `"left"`. | Not supported |
| `variant` | N/A | `Variant` — `FileInputs` enum (e.g., `Drop`). Controls display style. |
| `invalid` | Set via `clearValidation()` method. | `Invalid` — `string`, validation error message. |
| `uploadToRetoolStorage` | `boolean` — upload to Retool Storage. | Not supported (use custom `UploadHandler` for server-side storage) |
| `isPublic` | `boolean` — make file public in Retool Storage. | Not supported |
| `shouldOverwriteOnNameCollision` | `boolean` — replace same-name file in Retool Storage. | Not supported |

### Events

| Event | Retool | Ivy |
|-------|--------|-----|
| Change | Triggered when file selection changes. | `OnChange` — `Func<Event<IInput<TValue>, TValue>, ValueTask>` |
| Parse | Triggered when data parsing completes. | Not supported |
| Blur | N/A | `OnBlur` — `Func<Event<IAnyInput>, ValueTask>` |
| Cancel | N/A | `OnCancel` — `Func<Event<IAnyInput, Guid>, ValueTask>`, fires when upload is cancelled. |

### Methods

| Method | Retool | Ivy |
|--------|--------|-----|
| Clear value | `fileDropzone.clearValue()` | Set state to `null` or empty array |
| Reset value | `fileDropzone.resetValue()` | Manage via state reset |
| Set disabled | `fileDropzone.setDisabled(bool)` | Set `Disabled` property |
| Set hidden | `fileDropzone.setHidden(bool)` | Control visibility via conditional rendering |
| Focus | `fileDropzone.focus()` | Not supported |
| Clear validation | `fileDropzone.clearValidation()` | Set `Invalid` to `null` |
| Scroll into view | `fileDropzone.scrollIntoView(options)` | Not supported |
