# st.columns

Inserts containers laid out side-by-side and returns a list of column objects. Elements can be added to each column using the `with` statement or by calling methods directly on the returned objects.

## Streamlit

```python
import streamlit as st

col1, col2, col3 = st.columns(3)

with col1:
    st.header("A cat")
    st.image("https://static.streamlit.io/examples/cat.jpg")

with col2:
    st.header("A dog")
    st.image("https://static.streamlit.io/examples/dog.jpg")

with col3:
    st.header("An owl")
    st.image("https://static.streamlit.io/examples/owl.jpg")
```

## Ivy

```csharp
Layout.Grid()
    .Columns(3)
    .Rows(1)
    | new Card("A cat")
    | new Card("A dog")
    | new Card("An owl")
```

## Parameters

| Parameter           | Documentation                                                                                                                                                                  | Ivy                                                                          |
|---------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------|
| spec                | Number of columns (int) or relative widths (iterable of numbers, e.g. `[0.7, 0.3]`). Integer creates equal-width columns; iterable sets proportional widths.                  | `Columns(int)` sets the number of equal columns. Relative widths via iterable not supported. |
| gap                 | Size of the gap between columns. One of `"xxsmall"`, `"xsmall"`, `"small"` (default), `"medium"`, `"large"`, `"xlarge"`, `"xxlarge"`, or `None`.                               | `Gap(int)` sets spacing in grid units (default 4).                           |
| vertical_alignment  | Vertical alignment of the columns. One of `"top"` (default), `"center"`, or `"bottom"`.                                                                                       | Not supported                                                                |
| border              | Whether to show a border around each column container. Default `False`.                                                                                                        | Not supported                                                                |
