# st.expander

A container that can be expanded or collapsed. When collapsed, only the label is visible. All content inside is still computed and sent to the frontend even when closed.

## Streamlit

```python
import streamlit as st

st.bar_chart({"data": [1, 5, 2, 6, 2, 1]})

with st.expander("See explanation"):
    st.write("The chart above shows some numbers I picked for you.")
    st.image("https://static.streamlit.io/examples/dice.jpg")
```

## Ivy

```csharp
new Expandable("See explanation", new Layout(
    new Text("The chart above shows some numbers I picked for you."),
    new Image("https://static.streamlit.io/examples/dice.jpg")
))
```

## Parameters

| Parameter | Documentation                                                                                              | Ivy                          |
|-----------|------------------------------------------------------------------------------------------------------------|------------------------------|
| label     | Header text shown on the expander. Supports GitHub-flavored Markdown (bold, italics, strikethrough, etc.). | `header` constructor parameter |
| expanded  | If `True`, initializes the expander in the open state. Default: `False`.                                   | `.Open()` modifier           |
| icon      | Optional emoji or Material Symbol icon displayed next to the label. Default: `None`.                       | Not supported                |
