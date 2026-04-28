# st.download_button

Displays a download button that allows users to download files directly from the app. Data can be passed directly or via a callable for deferred generation of large files.

## Streamlit

```python
import streamlit as st
import pandas as pd
import numpy as np

@st.cache_data
def get_data():
    return pd.DataFrame(np.random.randn(50, 20))

@st.cache_data
def convert_csv(df):
    return df.to_csv().encode("utf-8")

df = get_data()
csv = convert_csv(df)

st.download_button(
    label="Download CSV",
    data=csv,
    file_name="data.csv",
    mime="text/csv"
)
```

## Ivy

```csharp
var data = GetData(); // returns DataFrame equivalent
var csv = ConvertCsv(data); // returns byte[]

var downloadUrl = UseDownload(
    factory: () => csv,
    mimeType: "text/csv",
    fileName: "data.csv"
);

return downloadUrl.Value != null
    ? new Button("Download CSV").Url(downloadUrl.Value)
    : Text.Block("Preparing download...");
```

## Parameters

| Parameter | Documentation | Ivy |
|-----------|---------------|-----|
| label | Button display text. Supports GitHub-flavored Markdown (bold, italics, links, images). | `Button(title)` constructor parameter |
| data | File contents as `str`, `bytes`, file-like object, or callable that returns contents. | `UseDownload(factory)` — a `Func<byte[]>` that generates file content |
| file_name | Name of the downloaded file. Auto-generated if omitted. | `UseDownload(fileName)` — required parameter |
| mime | MIME type of the file. Defaults based on data type. | `UseDownload(mimeType)` — required parameter |
| help | Tooltip text shown on hover. | Not supported |
| on_click | Callback or behavior on click (`"rerun"`, `"ignore"`, callable, or `None`). | `Button(onClick)` constructor parameter |
| type | Button styling: `"primary"`, `"secondary"`, or `"tertiary"`. | `Button(variant)` — `ButtonVariant.Primary`, `.Secondary()`, `.Tertiary()` |
| icon | Emoji or Material icon displayed on the button. | `Button(icon)` or `.Icon(Icons.X)` |
| icon_position | Icon placement: `"left"` or `"right"`. | `.Icon(Icons.X, Align.Left)` or `Align.Right` |
| disabled | Disables the button when `True`. | `.Disabled(true)` property |
| shortcut | Keyboard shortcut to trigger the button. | Not supported |
