# st.toast

Display a short message, known as a notification "toast". The notification appears in the app's top-right corner and disappears after a set duration.

## Streamlit

```python
import streamlit as st

st.toast("Your edited image was saved!", icon="😍")
```

## Ivy

```csharp
var client = UseService<IClientProvider>();
client.Toast("Your edited image was saved!", "Success");
```

## Parameters

| Parameter | Documentation | Ivy |
|-----------|---------------|-----|
| body | The string to display as GitHub-flavored Markdown | First argument of `client.Toast(message)` |
| icon | Optional emoji or icon displayed next to the message. Supports emojis, Material Symbols (`:material/icon_name:`), or `"spinner"` | Not supported |
| duration | Controls display duration: `"short"` (4s), `"long"` (10s), `"infinite"`, or custom seconds as `int`. Default is `"short"` | Not supported |
