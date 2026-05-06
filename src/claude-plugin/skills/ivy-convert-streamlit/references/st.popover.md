# st.popover

Insert a multi-element popover container that opens when a button is clicked. The popover can contain any content including interactive widgets. Clicking outside the popover closes it.

## Streamlit

```python
import streamlit as st

with st.popover("Open popover"):
    st.markdown("Hello World")
    name = st.text_input("What's your name?")

st.write("Your name:", name)
```

## Ivy

The closest equivalent in Ivy is the `DropDownMenu` widget, which attaches a floating menu to a trigger button. Unlike Streamlit's popover which can contain arbitrary widgets, Ivy's DropDownMenu is designed for structured menu items (default, checkbox, separator, nested submenus).

```csharp
new DropDownMenu(@evt => client.Toast("Selected: " + @evt.Value),
    new Button("Open menu"),
    MenuItem.Default("Option A").Tag("a"),
    MenuItem.Separator(),
    MenuItem.Checkbox("Dark Mode").Checked())
    .Bottom()
    .Header(Text.Muted("Settings"))
```

## Parameters

| Parameter | Documentation | Ivy |
|-----------|---------------|-----|
| label | The label of the button that opens the popover. Supports GitHub-flavored Markdown. | Set on the trigger Button: `new Button("label")` |
| type | Button style: `"primary"`, `"secondary"` (default), or `"tertiary"`. | Set on the trigger Button: `new Button("label").Primary()` / `.Tertiary()` |
| help | Tooltip text shown on hover. Supports GitHub-flavored Markdown. | `.WithTooltip("text")` on the trigger Button |
| icon | An emoji, Material Symbols icon (`:material/icon_name:`), or `"spinner"`. | `.Icon(Icons.Name)` on the trigger Button |
| disabled | Whether the popover button is disabled. Defaults to `False`. | `.Disabled()` on the trigger Button |
