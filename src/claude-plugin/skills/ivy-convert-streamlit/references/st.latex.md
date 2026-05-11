# st.latex

Display mathematical expressions formatted as LaTeX. Supports LaTeX functions from KaTeX.

## Streamlit

```python
import streamlit as st

st.latex(r'''
    a + ar + a r^2 + a r^3 + \cdots + a r^{n-1} =
    \sum_{k=0}^{n-1} ar^k =
    a \left(\frac{1-r^{n}}{1-r}\right)
    ''')
```

## Ivy

Ivy has no dedicated LaTeX widget. The `Markdown` widget supports KaTeX math expressions using `$$...$$` for block math and `$...$` for inline math.

```csharp
new Markdown("""
$$
a + ar + a r^2 + a r^3 + \cdots + a r^{n-1} =
\sum_{k=0}^{n-1} ar^k =
a \left(\frac{1-r^{n}}{1-r}\right)
$$
""")
```

## Parameters

| Parameter | Documentation                                                                                          | Ivy                                                    |
|-----------|--------------------------------------------------------------------------------------------------------|--------------------------------------------------------|
| body      | (str or SymPy expression) The string or SymPy expression to render as LaTeX.                           | `Content` on `Markdown`, wrapped in `$$...$$` delimiters |
| help      | (str or None) Optional tooltip shown next to the expression. Supports GitHub-flavored Markdown.        | Not supported                                          |
