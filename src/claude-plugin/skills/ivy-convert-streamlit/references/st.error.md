# st.error

Display an error message in a styled alert box. Used to communicate error states to the user.

## Streamlit

```python
import streamlit as st

st.error('This is an error', icon="🚨")
```

## Ivy

```csharp
Callout.Error("This is an error");
```

## Parameters

| Parameter | Documentation                                                                                                          | Ivy                                                             |
|-----------|------------------------------------------------------------------------------------------------------------------------|-----------------------------------------------------------------|
| body      | The error message. Supports GitHub-flavored Markdown and additional Markdown directives (see `st.markdown`).           | `message` parameter in `Callout.Error(message, title)`          |
| icon      | An optional icon shown next to the alert. A single-character emoji, a `:material/icon_name:` Material Symbol, or `"spinner"`. | `.Icon(Icons.XXX)` fluent setter, e.g. `.Icon(Icons.CircleAlert)` |
