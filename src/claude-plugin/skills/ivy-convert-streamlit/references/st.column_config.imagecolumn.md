# ImageColumn

Configures an image column in `st.dataframe` or `st.data_editor`. Displays images from URLs, SVG data URIs, or base64 data URIs within table cells. The column is not editable.

## Streamlit

```python
import pandas as pd
import streamlit as st

data_df = pd.DataFrame(
    {
        "apps": [
            "https://storage.googleapis.com/s4a-prod-share-preview/default/st_app_screenshot_image/5435030c-0571-4571-b1f0-5a46c7a7a0b2/Home_Page.png",
            "https://storage.googleapis.com/s4a-prod-share-preview/default/st_app_screenshot_image/ef9a7627-13f2-47e5-8f65-3f69bb38a5c2/Home_Page.png",
            "https://storage.googleapis.com/s4a-prod-share-preview/default/st_app_screenshot_image/31b99099-8c43-4b8c-842e-65b4f8e14571/Home_Page.png",
            "https://storage.googleapis.com/s4a-prod-share-preview/default/st_app_screenshot_image/6a399b09-241e-4ae7-a31f-7640dc1d181e/Home_Page.png",
        ],
    }
)

st.data_editor(
    data_df,
    column_config={
        "apps": st.column_config.ImageColumn(
            "Preview Image", help="Streamlit app preview screenshots"
        )
    },
    hide_index=True,
)
```

## Ivy

The Table widget does not have a dedicated image column type. The `Builder()` method supports `.Link()`, `.Text()`, `.CopyToClipboard()`, and `.Default()` renderers but not an `.Image()` renderer.

```csharp
// Not directly supported.
// The closest approach uses Builder with available renderers:
new Table(items)
    .Builder(p => p.ImageUrl, f => f.Link());
```

## Parameters

| Parameter | Documentation                                                                                                  | Ivy           |
|-----------|----------------------------------------------------------------------------------------------------------------|---------------|
| label     | The label shown at the top of the column. If `None`, the column name is used.                                  | `Header()`    |
| help      | A tooltip displayed when hovering over the column label. Supports GitHub-flavored Markdown.                    | Not supported |
| pinned    | Whether the column stays visible when scrolling. Index columns are pinned by default, data columns are not.    | Not supported |
