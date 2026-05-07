# st.text

Write fixed-width, pre-formatted text without Markdown or HTML parsing.

## Streamlit

```python
import streamlit as st

st.text("This is text\n[and more text](that's not a Markdown link).")
```

## Ivy

```csharp
Text.Literal("This is text\n[and more text](that's not a Markdown link).")
```

## Parameters

| Parameter      | Documentation                                                                 | Ivy                                              |
|----------------|-------------------------------------------------------------------------------|--------------------------------------------------|
| body           | The string content to display                                                 | First argument to `Text.Literal()`               |
| help           | Optional tooltip next to the text, supports GitHub-flavored Markdown          | Not supported                                    |
| text_alignment | Horizontal alignment: "left", "center", "right", or "justify" (default left) | Not supported                                    |
