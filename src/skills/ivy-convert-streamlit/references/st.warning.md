# st.warning

Display a warning message. Renders styled alert box with an optional icon, supporting GitHub-flavored Markdown in the body text.

## Streamlit

```python
import streamlit as st

st.warning("This is a warning", icon="\u26a0\ufe0f")
```

## Ivy

```csharp
Callout.Warning("This is a warning");
```

## Parameters

| Parameter | Documentation                                                                                                                          | Ivy                                                            |
|-----------|----------------------------------------------------------------------------------------------------------------------------------------|----------------------------------------------------------------|
| body      | The warning message text. Supports GitHub-flavored Markdown including bold, italics, links, code blocks, etc.                          | `description` parameter in Callout                             |
| icon      | An optional emoji (e.g. `"\u26a0\ufe0f"`) or Material Symbol icon (e.g. `":material/warning:"`) displayed next to the warning message. | `icon` parameter using the `Icons` enum (e.g. `Icons.TriangleAlert`) |
