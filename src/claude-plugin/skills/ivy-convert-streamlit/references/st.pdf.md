# st.pdf

Displays an embedded PDF viewer in the app. Supports URLs, file paths, file-like objects, and raw bytes. Requires the `streamlit[pdf]` extra package.

## Streamlit

```python
st.pdf("https://example.com/sample.pdf")
```

## Ivy

Ivy has no dedicated PDF widget. The closest equivalent is `Iframe`, which can display a hosted PDF via the browser's built-in PDF renderer.

```csharp
new Iframe("https://example.com/sample.pdf")
    .Width(Size.Full())
    .Height(Size.Units(120))
```

## Parameters

| Parameter | Documentation                                                                                               | Ivy                                                                     |
|-----------|-------------------------------------------------------------------------------------------------------------|-------------------------------------------------------------------------|
| data      | The PDF to display. Accepts a URL string, file path, file-like object (e.g. UploadedFile), or raw bytes.    | `Iframe.src` supports URLs only. File paths and raw bytes not supported. |
