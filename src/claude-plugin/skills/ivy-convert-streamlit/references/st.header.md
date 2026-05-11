# st.header

Display text in header formatting. Streamlit renders `st.header` as an `<h2>` element with optional divider, tooltip, and anchor support.

## Streamlit

```python
import streamlit as st

st.header("_Streamlit_ is :blue[cool] :sunglasses:")
st.header("This is a header with a divider", divider="gray")
st.header("Helpful header", help="This is a tooltip")
```

## Ivy

```csharp
Text.H2("Ivy is cool")
    | new Separator()
    | Text.H2("Helpful header").WithTooltip("This is a tooltip")
```

## Parameters

| Parameter      | Streamlit                                                                                              | Ivy                                                                                  |
|----------------|--------------------------------------------------------------------------------------------------------|--------------------------------------------------------------------------------------|
| body           | The text to display, supports GitHub-flavored Markdown.                                                | First argument of `Text.H2("...")`. Markdown via `Text.Markdown()` instead.          |
| anchor         | An optional anchor name for URL linking (e.g. `#anchor`). Set to `False` to hide.                     | Not supported                                                                        |
| help           | Tooltip text displayed next to the header. Supports GitHub-flavored Markdown.                          | `.WithTooltip("...")` extension method on any widget                                 |
| divider        | Colored divider line below the header. `True` cycles colors, or pass a specific color like `"gray"`.  | Separate `new Separator()` widget placed after the header                            |
| text_alignment | Horizontal text alignment: `"left"`, `"center"`, `"right"`, or `"justify"`. Defaults to `"left"`.     | Layout-level alignment via `.Align()` on a parent layout                             |
