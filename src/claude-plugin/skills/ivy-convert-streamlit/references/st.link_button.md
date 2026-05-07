# st.link_button

Displays a link button that opens a specified URL in a new tab when clicked.

## Streamlit

```python
import streamlit as st

st.link_button("Go to gallery", "https://streamlit.io/gallery")
```

## Ivy

```csharp
new Button("Go to gallery")
    .Url("https://streamlit.io/gallery")
    .OpenInNewTab()
```

## Parameters

| Parameter       | Documentation                                                                                          | Ivy                                                          |
|-----------------|--------------------------------------------------------------------------------------------------------|--------------------------------------------------------------|
| `label`         | `str` - Button text; supports GitHub-flavored Markdown formatting.                                     | `Title` via constructor: `new Button("label")`               |
| `url`           | `str` - The URL to open when clicked.                                                                  | `.Url("https://...")` property                               |
| `help`          | `str or None` - Tooltip text displayed on hover; supports Markdown. Default `None`.                    | `.Tooltip("text")` property                                  |
| `type`          | `str` - `"primary"`, `"secondary"` (default), or `"tertiary"`.                                         | `ButtonVariant` enum: `Primary`, `Secondary`, `Ghost`, etc.  |
| `icon`          | `str or None` - Emoji, Material Symbols icon, or `"spinner"`. Default `None`.                          | `Icons` enum via constructor or `.Icon()` property           |
| `icon_position` | `str` - `"left"` (default) or `"right"`. Placement of icon relative to label.                          | `.IconPosition(Align)` property                              |
| `disabled`      | `bool` - Disables the button when `True`. Default `False`.                                             | `.Disabled(bool)` property                                   |
| `shortcut`      | `str or None` - Keyboard shortcut to trigger the button (e.g. `"Ctrl+K"`). Default `None`.            | Not supported                                                |
