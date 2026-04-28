# st.caption

Display text in small font. Used for captions, asides, footnotes, sidenotes, and other explanatory text. Supports GitHub-flavored Markdown including italics, colors, and emojis.

## Streamlit

```python
import streamlit as st

st.caption("This is a string that explains something above.")
st.caption("A caption with _italics_ :blue[colors] and emojis :sunglasses:")
```

## Ivy

```csharp
using Ivy;
using static Ivy.Views.Text;

Muted("This is a string that explains something above.");
```

## Parameters

| Parameter          | Documentation                                                                                         | Ivy           |
|--------------------|-------------------------------------------------------------------------------------------------------|---------------|
| body               | The text to display. Supports GitHub-flavored Markdown.                                               | First argument of `Muted()` |
| unsafe_allow_html  | If True, renders HTML expressions instead of escaping them. Default is False.                         | Not supported |
| help               | Optional tooltip displayed next to the caption. Supports GitHub-flavored Markdown. Default is None.   | Not supported |
| text_alignment     | Horizontal text alignment: "left" (default), "center", "right", or "justify".                         | Not supported |
