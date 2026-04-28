# st.status

Insert a status container to display output from long-running tasks. The container is expandable/collapsible (similar to `st.expander`) and shows a status icon (spinner, checkmark, or error) alongside a label. Content inside remains computed even when collapsed.

## Streamlit

```python
import time
import streamlit as st

with st.status("Downloading data...", expanded=True) as status:
    st.write("Searching for data...")
    time.sleep(2)
    st.write("Found URL.")
    time.sleep(1)
    st.write("Downloading data...")
    time.sleep(1)
    status.update(label="Download complete!", state="complete", expanded=False)
```

## Ivy

The closest equivalent is the `Expandable` widget, which provides a collapsible container with a header and content. It does not have built-in status states (running/complete/error) or icon indicators.

```csharp
new Expandable("Downloading data...",
    Layout.Vertical()
        | "Searching for data..."
        | "Found URL."
        | "Downloading data..."
).Open()
```

## Parameters

| Parameter | Documentation                                                                                                              | Ivy                                                  |
|-----------|-----------------------------------------------------------------------------------------------------------------------------|------------------------------------------------------|
| label     | Initial label for the status container. Supports GitHub-flavored Markdown (bold, italics, strikethrough, inline code, links) | `header` constructor parameter                       |
| expanded  | If `True`, the container initializes in an expanded state. Default `False`                                                   | `.Open()` fluent method                              |
| state     | Initial state: `"running"` (spinner), `"complete"` (checkmark), or `"error"` (error icon). Default `"running"`               | Not supported                                        |
