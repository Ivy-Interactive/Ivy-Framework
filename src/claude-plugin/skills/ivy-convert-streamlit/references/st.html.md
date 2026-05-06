# st.html

Display custom HTML content in your app. The HTML is sanitized by default and JavaScript is ignored unless explicitly allowed.

## Streamlit

```python
import streamlit as st

st.html(
    "<p><span style='text-decoration: line-through double red;'>Oops</span>!</p>"
)
```

## Ivy

```csharp
public class HtmlExample : ViewBase
{
    public override object? Build()
    {
        return new Html("<p><span style='text-decoration: line-through double red;'>Oops</span>!</p>");
    }
}
```

## Parameters

| Parameter                  | Documentation                                                                                                            | Ivy                                                        |
|----------------------------|--------------------------------------------------------------------------------------------------------------------------|------------------------------------------------------------|
| `body`                     | The HTML content to render. Accepts a string, a path to a local HTML file, or any object convertible to string.          | `content` (string only)                                    |
| `unsafe_allow_javascript`  | When `True`, JavaScript in the HTML will be executed. Defaults to `False`. Never use with untrusted input.               | Not supported (all `<script>` tags are always removed)     |
