# st.container

Insert a multi-element container that can hold multiple elements. Elements can be added out of order using the returned container object, or inline using `with` notation. Supports vertical and horizontal layouts, fixed-height scrolling, and alignment options.

## Streamlit

```python
import streamlit as st

with st.container(border=True, gap="medium"):
    st.write("Inside the container")
    st.bar_chart({"data": [1, 5, 2, 6]})

st.write("Outside the container")

# Horizontal layout
flex = st.container(horizontal=True, horizontal_alignment="center", gap="small")
flex.button("Button 1")
flex.button("Button 2")
flex.button("Button 3")
```

## Ivy

```csharp
using Ivy;

// Vertical container with border
return new Box(
    new StackLayout([
        Text.Label("Inside the container"),
        new BarChart(data)
    ], gap: 4)
).BorderStyle(BorderStyle.Solid).BorderThickness(1).Padding(4);

// Horizontal layout
return new StackLayout([
    new Button("Button 1"),
    new Button("Button 2"),
    new Button("Button 3")
], Orientation.Horizontal, gap: 2, align: Align.Center);
```

## Parameters

| Parameter                | Streamlit Documentation                                                                                     | Ivy                                                                          |
|--------------------------|-------------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------|
| border                   | `bool or None` - Controls border display. Default shows border only when height is fixed.                   | Use `Box` with `.BorderStyle()` and `.BorderThickness()` to add a border     |
| horizontal               | `bool` - `False` (default) stacks vertically; `True` stacks horizontally.                                  | `StackLayout` `Orientation.Horizontal` or `Layout.Horizontal()`              |
| horizontal_alignment     | `"left"`, `"center"`, `"right"`, `"distribute"` - Horizontal alignment of child elements.                  | `Align` parameter (e.g. `Align.Center`, `Align.Left`, `Align.Right`)         |
| vertical_alignment       | `"top"`, `"center"`, `"bottom"`, `"distribute"` - Vertical alignment of child elements.                    | `Align` parameter (e.g. `Align.Center`)                                      |
| gap                      | `"xxsmall"` to `"xxlarge"` or `None` - Minimum gap between elements.                                       | `Gap` property (`int` in spacing units)                                      |
