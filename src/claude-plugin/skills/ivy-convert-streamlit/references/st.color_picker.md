# st.color_picker

Displays a color picker widget that allows users to select a color, returned as a hex string.

## Streamlit

```python
import streamlit as st

color = st.color_picker("Pick A Color", "#00f900")
st.write("The current color is", color)
```

## Ivy

```csharp
public class ColorDemo : ViewBase
{
    public override object? Build()
    {
        var color = UseState("#00f900");
        return color.ToColorInput();
    }
}
```

## Parameters

| Parameter        | Streamlit                                                                                      | Ivy                                                          |
|------------------|------------------------------------------------------------------------------------------------|--------------------------------------------------------------|
| label            | `label` (str) - A short label explaining the input purpose. Supports markdown.                 | `Placeholder` (string) - Placeholder text for the input.     |
| value            | `value` (str) - Initial hex color value. Defaults to black.                                    | `Value` (string) - Bound via state or constructor parameter.  |
| help             | `help` (str) - Tooltip displayed next to the label.                                            | Not supported                                                |
| on_change        | `on_change` (callable) - Callback invoked when color changes.                                  | `OnChange` - Event handler for color changes.                |
| disabled         | `disabled` (bool) - Disables the color picker.                                                 | `Disabled` (bool) - Disables the color input.                |
| label_visibility | `label_visibility` (str) - Controls label display: "visible", "hidden", or "collapsed".        | `Visible` (bool) - Controls widget visibility.               |
| variant          | Not supported                                                                                  | `Variant` (ColorInputs) - Text, Picker, TextAndPicker, Swatch. |
| nullable         | Not supported                                                                                  | `Nullable` (bool) - Whether the value can be null.           |
| invalid          | Not supported                                                                                  | `Invalid` (string) - Validation message for invalid input.   |
| foreground       | Not supported                                                                                  | `Foreground` (bool?) - Foreground color mode.                |
