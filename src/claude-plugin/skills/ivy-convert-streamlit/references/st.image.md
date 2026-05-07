# st.image

Displays one or more images on the page, supporting URLs, file paths, numpy arrays, byte arrays, and SVG strings.

## Streamlit

```python
import streamlit as st

st.image("sunrise.jpg", caption="Sunrise by the mountains")
```

## Ivy

```csharp
new Image("sunrise.jpg")
```

## Parameters

| Parameter         | Documentation                                                                 | Ivy           |
|-------------------|-------------------------------------------------------------------------------|---------------|
| image             | The image source: a URL, file path, SVG string, numpy array, or list of these | `Src`         |
| caption           | Text label beneath the image; supports GitHub-flavored Markdown               | Not supported |
| clamp             | Whether to clamp byte array pixel values to the 0-255 range                   | Not supported |
| channels          | Color channel ordering for numpy arrays (`"RGB"` or `"BGR"`)                  | Not supported |
| output_format     | Output compression format: `"JPEG"`, `"PNG"`, or `"auto"`                     | Not supported |
| use_container_width | *Deprecated* — use `width` instead                                          | Not supported |
