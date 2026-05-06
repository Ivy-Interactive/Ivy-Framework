# st.markdown

Display string formatted as Markdown with support for GitHub-flavored Markdown, emoji shortcodes, LaTeX expressions, colored text, and Material Symbols.

## Streamlit

```python
import streamlit as st

st.markdown("*Streamlit* is **really** ***cool***.")
st.markdown(':red[Streamlit] :orange[can] :green[write] :blue[text]')
st.markdown("Here's a bouquet — :tulip: :rose: :sunflower:")

multi = '''If you end a line with two spaces,
a soft return is used for the next line.'''
st.markdown(multi)
```

## Ivy

```csharp
new Markdown("""
    *Ivy* is **really** ***cool***.

    Here's a bouquet — :tulip: :rose: :sunflower:
    """)
    .HandleLinkClick(e => Console.WriteLine($"Navigate to: {e.Data}"));
```

## Parameters

| Parameter | Streamlit | Ivy |
|-----------|-----------|-----|
| `body` / `Content` | The string to display as GitHub-flavored Markdown. Also supports emoji shortcodes, LaTeX, colored text, and Material Symbols. | `string content` — Markdown content to render. Supports text formatting, lists, code blocks, KaTeX math, Mermaid diagrams, and emoji. |
| `unsafe_allow_html` | `bool` (default `False`) — Whether to render HTML within the body. When `False`, HTML tags are escaped. | Not supported (HTML tags are supported by default) |
| `help` | `str` (default `None`) — A tooltip displayed next to the markdown, supports GitHub-flavored Markdown. | Not supported |
| `text_alignment` | `"left"`, `"center"`, `"right"`, or `"justify"` (default `"left"`) — Horizontal text alignment. | Not supported |
| `OnLinkClick` | Not supported | `Func<Event<Markdown, string>, ValueTask>` — Event handler called when a link is clicked, receiving the URL as data. |
