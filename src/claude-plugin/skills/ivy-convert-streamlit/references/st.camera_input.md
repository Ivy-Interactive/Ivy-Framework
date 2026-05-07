# st.camera_input

Displays a widget that captures pictures from the user's webcam. Returns an `UploadedFile` (a `BytesIO` subclass) or `None`.

## Streamlit

```python
import streamlit as st

enable = st.checkbox("Enable camera")
picture = st.camera_input("Take a picture", disabled=not enable)

if picture:
    st.image(picture)
```

## Ivy

Ivy does not have a dedicated camera input widget. The closest equivalent is `FileInput` configured to accept images.

```csharp
var fileState = UseState<FileUpload<byte[]>?>();
var upload = UseUpload(MemoryStreamUploadHandler.Create(fileState))
    .Accept("image/*")
    .MaxFileSize(FileSize.FromMegabytes(5));
return fileState.ToFileInput(upload).Placeholder("Choose an image");
```

## Parameters

| Parameter        | Documentation                                                                                          | Ivy                                                        |
|------------------|--------------------------------------------------------------------------------------------------------|------------------------------------------------------------|
| label            | A short label explaining the widget's purpose. Supports GitHub-flavored Markdown.                      | `Placeholder("...")`                                       |
| help             | Optional tooltip displayed next to the label.                                                          | Not supported                                              |
| on_change        | Optional callback invoked when the value changes.                                                      | `OnBlur`, `OnCancel` event handlers                        |
| disabled         | Disables the camera input when set to `True`. Default is `False`.                                      | `Disabled(true)`                                           |
| label_visibility | Controls label display: `"visible"`, `"hidden"` (spacer only), or `"collapsed"` (no label and spacer). | Not supported                                              |
