# st.file_uploader

Displays a file uploader widget that enables users to upload files. Supports single file, multiple files, and directory uploads with configurable file type filtering and size limits.

## Streamlit

```python
import streamlit as st
import pandas as pd

# Single file upload
uploaded_file = st.file_uploader("Choose a file", type=["csv", "xlsx"])
if uploaded_file is not None:
    dataframe = pd.read_csv(uploaded_file)
    st.write(dataframe)

# Multiple file upload
uploaded_files = st.file_uploader(
    "Upload data",
    accept_multiple_files=True,
    type="csv"
)
for uploaded_file in uploaded_files:
    df = pd.read_csv(uploaded_file)
    st.write(df)
```

## Ivy

```csharp
public class FileUploadDemo : ViewBase
{
    public override object? Build()
    {
        // Single file upload
        var fileState = UseState<FileUpload<byte[]>?>();
        var upload = UseUpload(MemoryStreamUploadHandler.Create(fileState))
            .Accept(".csv,.xlsx")
            .MaxFileSize(FileSize.FromMegabytes(200));

        return fileState.ToFileInput(upload)
            .Placeholder("Choose a file");
    }
}

public class MultiFileUploadDemo : ViewBase
{
    public override object? Build()
    {
        // Multiple file upload
        var files = UseState(ImmutableArray.Create<FileUpload<byte[]>>());
        var upload = UseUpload(MemoryStreamUploadHandler.Create(files))
            .Accept(".csv")
            .MaxFiles(10);

        return files.ToFileInput(upload)
            .Placeholder("Upload data");
    }
}
```

## Parameters

| Parameter | Streamlit | Ivy |
|-----------|-----------|-----|
| label | `label` (str) — A short label explaining what the uploader is for. Supports markdown. | `.Placeholder(string)` — Sets the prompt text displayed in the drop zone. |
| type | `type` (str or list) — Allowed file extensions, e.g. `"csv"` or `["jpg", "png"]`. `None` allows all. | `.Accept(string)` — MIME types or extensions, e.g. `"image/*"` or `".pdf,.doc"`. |
| accept_multiple_files | `accept_multiple_files` (bool or `"directory"`) — `False` for single file, `True` for multiple, `"directory"` for folder upload. | Determined by state type: `UseState<FileUpload<byte[]>?>()` for single, `UseState(ImmutableArray.Create<FileUpload<byte[]>>())` for multiple. |
| help | `help` (str) — Tooltip shown next to the label. Supports markdown. | Not supported |
| on_change | `on_change` (callable) — Callback when value changes. | `.HandleChange(Func<Event, ValueTask>)` — Event handler when files are selected. |
| max_upload_size | `max_upload_size` (int) — Max file size in MB per upload. Defaults to `server.maxUploadSize`. | `.MaxFileSize(FileSize)` — Per-file size limit, e.g. `FileSize.FromMegabytes(5)`. |
| disabled | `disabled` (bool) — Disables the uploader when `True`. | `.Disabled` (bool) — Disables the file input. |
| label_visibility | `label_visibility` ("visible", "hidden", "collapsed") — Controls whether the label is shown, hidden with spacing, or fully collapsed. | Not supported |
