# st.button

Display a button widget that users can click to trigger actions.

## Streamlit

```python
import streamlit as st

if st.button("Say hello"):
    st.write("Why hello there")
else:
    st.write("Goodbye")

st.button("Reset", type="primary")
st.button("Aloha", type="tertiary")
st.button("Emoji button", icon="😃")
st.button("Material button", icon=":material/mood:")
```

## Ivy

```csharp
new Button("Say hello", onClick: e => Console.WriteLine("Why hello there"));

new Button("Reset").Primary();
new Button("Aloha").Ghost();
new Button("Emoji button").Icon(Icons.Mood);
```

## Parameters

| Parameter | Documentation | Ivy |
|-----------|---------------|-----|
| label | A short label explaining to the user what this button is for. Supports Markdown. | `Title` property (`string`) |
| help | Tooltip displayed on hover, supports Markdown. | `Tooltip` property (`string`) |
| on_click | Optional callback invoked when the button is clicked. | `OnClick` event (`Func<Event<Button>, ValueTask>`) |
| type | `"primary"`, `"secondary"`, or `"tertiary"`. Default `"secondary"`. | `Variant` property (`ButtonVariant`) — supports Primary, Secondary, Outline, Ghost, Destructive, Link, Success, Warning, Info |
| icon | Emoji or Material Symbols icon string. | `Icon` property (`Icons?`) |
| icon_position | `"left"` or `"right"`. Default `"left"`. | `IconPosition` property (`Align`) |
| disabled | Disables the button when `True`. Default `False`. | `Disabled` property (`bool`) |
| shortcut | Keyboard shortcut string (e.g. `"Ctrl+K"`). | Not supported |
| use_container_width | Whether the button stretches to fill its container. | Not supported |
