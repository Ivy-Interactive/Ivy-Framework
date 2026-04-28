# st.title

Display text in title formatting. Each document should ideally contain a single `st.title()`.

## Streamlit

```python
import streamlit as st

st.title("This is a title")
st.title("_Streamlit_ is :blue[cool] :sunglasses:")
```

## Ivy

```csharp
Layout.Vertical()
    | Text.H1("This is a title")
```

## Parameters

| Parameter      | Documentation                                                                                              | Ivy           |
|----------------|------------------------------------------------------------------------------------------------------------|---------------|
| body           | The text to display as GitHub-flavored Markdown.                                                           | `Text.H1(string content)` |
| anchor         | Sets the header's anchor name for URL access via `#anchor`. Pass `False` to hide from UI.                 | Not supported |
| help           | Optional tooltip appearing next to the title. Accepts GitHub-flavored Markdown.                            | Not supported |
| text_alignment | Horizontal text alignment: `"left"` (default), `"center"`, `"right"`, or `"justify"`.                     | Not supported |
