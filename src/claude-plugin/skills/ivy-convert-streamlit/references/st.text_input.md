# st.text_input

Display a single-line text input widget. Returns the current value as a string.

## Streamlit

```python
import streamlit as st

title = st.text_input("Movie title", "Life of Brian")
st.write("The current movie title is", title)
```

## Ivy

```csharp
public class TextInputDemo : ViewBase
{
    public override object? Build()
    {
        var title = UseState("Life of Brian");

        return title.ToTextInput()
            .Placeholder("Movie title")
            .WithField()
            .Label("Movie title");
    }
}
```

## Parameters

| Parameter | Streamlit | Ivy |
|-----------|-----------|-----|
| label | `label` (str) — Short label explaining the input's purpose. Supports Markdown. | `.WithField().Label(string)` — Label is provided via the Field wrapper, not the input itself. |
| value | `value` (str, default `""`) — Initial text value. | First constructor parameter `string value` — the current text value. |
| max_chars | `max_chars` (int or None) — Maximum number of characters allowed. | `.MaxLength(int?)` — Character limit for the input. |
| type | `type` ("default" or "password") — Set to "password" to mask input. | `variant: TextInputVariants` enum — Supports `Text`, `Password`, `Textarea`, `Search`, `Email`, `Tel`, `Url`. |
| help | `help` (str or None) — Tooltip shown next to the label. | `.WithField().Help(string)` — Tooltip via info icon on the Field wrapper. |
| autocomplete | `autocomplete` (str) — HTML autocomplete attribute value. | Not supported |
| on_change | `on_change` (callable) — Callback invoked when the value changes. | `onChange` constructor parameter — `Action` or `Func<..., ValueTask>` handler. |
| placeholder | `placeholder` (str or None) — Text shown when the input is empty. | `.Placeholder(string)` — Hint text displayed in the empty input. |
| disabled | `disabled` (bool, default `False`) — Disables the input. | `.Disabled(bool)` or `disabled` constructor parameter. |
| label_visibility | `label_visibility` ("visible", "hidden", "collapsed") — Controls label display. | `.WithField().Visible(bool)` on the Field wrapper controls visibility, but no direct hidden/collapsed distinction. |
| icon | `icon` (str or None) — Emoji or Material Symbols icon in the input. | `.Prefix(Affix)` / `.Suffix(Affix)` — Static content around the input, can be used for icons. |
