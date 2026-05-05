# st.divider

Displays a horizontal rule in the app, providing a visual separator between content sections. An alternative to `st.write("---")` or using `---` via magic commands.

## Streamlit

```python
import streamlit as st

st.write("This is some text.")
st.divider()
st.write("This text is between the horizontal rules.")
st.divider()
```

## Ivy

```csharp
| Text("This is some text.")
| new Separator()
| Text("This text is between the horizontal rules.")
| new Separator()
```

## Parameters

| Parameter   | Documentation                                      | Ivy                               |
|-------------|----------------------------------------------------|------------------------------------|
| -           | -                                                  | text (string) - text to display with the separator |
| -           | -                                                  | orientation (Orientation) - direction of the separator, defaults to Horizontal |
