# st.subheader

Display text in subheader formatting. Supports GitHub-flavored Markdown, optional tooltips, colored dividers, and text alignment.

## Streamlit

```python
import streamlit as st

st.subheader("_Streamlit_ is :blue[cool] :sunglasses:")
st.subheader("This is a subheader with a divider", divider="gray")
```

## Ivy

```csharp
using static Ivy.Views.Text;

H2("Ivy is cool");
```

## Parameters

| Parameter      | Documentation                                                                                                       | Ivy           |
|----------------|---------------------------------------------------------------------------------------------------------------------|---------------|
| body           | The text to display, rendered as GitHub-flavored Markdown.                                                          | `H2(content)` |
| anchor         | An optional anchor id. If omitted, generates one from body. If `False`, hides the anchor.                           | Not supported |
| help           | Tooltip text shown next to the subheader, supports GitHub-flavored Markdown.                                        | Not supported |
| divider        | Colored divider line below the subheader. `True` cycles colors; or specify `"blue"`, `"green"`, `"orange"`, `"red"`, `"violet"`, `"yellow"`, `"gray"`, `"rainbow"`. | Not supported |
| text_alignment | Horizontal text alignment: `"left"`, `"center"`, `"right"`, or `"justify"`. Default is `"left"`.                   | Not supported |
