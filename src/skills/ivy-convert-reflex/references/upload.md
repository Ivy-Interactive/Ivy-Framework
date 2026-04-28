# Upload

File upload component that allows users to select files via click or drag-and-drop, upload them to the server, and track upload progress. Supports file type filtering, size limits, and multiple file selection.

## Reflex

```python
class UploadExample(rx.State):
    files: list[rx.UploadFile]

    async def handle_upload(self, files: list[rx.UploadFile]):
        for file in files:
            upload_data = await file.read()
            outfile = rx.get_upload_dir() / file.filename
            with outfile.open("wb") as f:
                f.write(upload_data)

def index():
    return rx.upload(
        rx.text("Drag and drop files here or click to select files"),
        accept={
            "image/png": [".png"],
            "image/jpeg": [".jpg", ".jpeg"],
        },
        max_files=5,
        max_size=5_000_000,
        multiple=True,
        on_drop=UploadExample.handle_upload(rx.upload_files()),
    )
```

## Ivy

```csharp
public class UploadExample : ViewBase
{
    public override object? Build()
    {
        var fileState = UseState<FileUpload<byte[]>?>();
        var upload = UseUpload(MemoryStreamUploadHandler.Create(fileState));
        return fileState.ToFileInput(upload,
            accept: "image/png,.jpg,.jpeg",
            maxFiles: 5,
            maxFileSize: 5_000_000,
            multiple: true
        );
    }
}
```

## Parameters

| Parameter      | Documentation                                              | Ivy                                              |
|----------------|------------------------------------------------------------|--------------------------------------------------|
| `accept`       | File types to accept (dict of MIME types to extensions)    | `Accept` (string of MIME types/extensions)        |
| `disabled`     | Disable upload functionality                               | `Disabled`                                        |
| `max_files`    | Maximum number of files allowed                            | `MaxFiles`                                        |
| `max_size`     | Maximum file size in bytes                                 | `MaxFileSize`                                     |
| `min_size`     | Minimum file size in bytes                                 | Not supported                                     |
| `multiple`     | Allow multiple file selection                              | `Multiple` (auto-enabled with `ImmutableArray`)   |
| `no_click`     | Disable click-to-upload                                    | Not supported                                     |
| `no_drag`      | Disable drag-and-drop                                      | Not supported                                     |
| `no_keyboard`  | Disable keyboard shortcuts for upload                      | Not supported                                     |
| `on_drop`      | Fired when files are dropped                               | `OnChange`                                        |
| `on_drop_rejected` | Fired when files don't meet criteria                  | Not supported                                     |
| `on_upload_progress` | Fires during upload for progress tracking            | Built-in progress tracking via `UseUpload` hook   |
| N/A            | N/A                                                        | `Placeholder` (instructional text)                |
| N/A            | N/A                                                        | `Variant` (visual style: Drop, etc.)              |
| N/A            | N/A                                                        | `Invalid` (error message text)                    |
| N/A            | N/A                                                        | `UploadUrl` (custom endpoint URL)                 |
