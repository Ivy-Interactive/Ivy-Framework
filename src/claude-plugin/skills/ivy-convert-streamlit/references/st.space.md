# st.space

Adds vertical or horizontal spacing to a layout. The direction of the space depends on its parent container — vertical layouts receive vertical space, while horizontal layouts receive horizontal space.

## Streamlit

```python
import streamlit as st

left, middle, right = st.columns(3)
left.space("medium")
left.button("Left button", width="stretch")
middle.space("small")
middle.text_input("Middle input")
right.audio_input("Right uploader")

# Horizontal spacing with stretch
with st.container(horizontal=True):
    st.button("Left")
    st.space("stretch")
    st.button("Right")
```

## Ivy

```csharp
// Vertical spacing with fixed size
Layout.Vertical()
    | new Card("Top")
    | new Spacer().Height(Size.Rem(2.5m))
    | new Card("Bottom")

// Horizontal spacing with grow (equivalent to "stretch")
Layout.Horizontal()
    | new Button("Left")
    | new Spacer().Width(Size.Grow())
    | new Button("Right")
```

## Parameters

| Parameter | Documentation                                                                                                                                                                                                                                                 | Ivy                                                                          |
|-----------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------|
| size      | The size of the space. Predefined values: `"xxsmall"` (0.25rem), `"xsmall"` (0.5rem), `"small"` (0.75rem, default), `"medium"` (2.5rem), `"large"` (4.25rem), `"xlarge"` (6rem), `"xxlarge"` (8rem). Also accepts `"stretch"` or an integer for pixel values. | `.Height()` / `.Width()` with `Size.Rem()`, `Size.Px()`, or `Size.Grow()` |
