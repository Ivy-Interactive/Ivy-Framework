# st.text_area

Display a multi-line text input widget. The user can type in text spanning multiple lines.

## Streamlit

```python
import streamlit as st

txt = st.text_area(
    "Text to analyze",
    "It was the best of times, it was the worst of times...",
)
st.write(f"You wrote {len(txt)} characters.")
```

## Ivy

```csharp
address.ToTextInput()
    .Placeholder("Enter text here...")
    .Variant(TextInputVariants.Textarea)
```

## Parameters

| Parameter | Documentation | Ivy |
|-----------|---------------|-----|
| label | A short label explaining to the user what this input is for. Supports GitHub-flavored Markdown. | Not supported (use a separate label widget) |
| value | The text value of this widget when it first renders. Will be cast to str internally. | Bound via `IAnyState` or a value + `OnChange` callback |
| max_chars | Max number of characters allowed in the text input. | `.MaxLength(int)` |
| help | An optional tooltip that gets displayed next to the label. | Not supported |
| on_change | An optional callback invoked when this text area's value changes. | `.OnChange(Action)` or `.OnBlur(Action)` |
| placeholder | An optional string displayed when the text area is empty. Provides a hint to the user. | `.Placeholder(string)` |
| disabled | An optional boolean which disables the text area when set to True. Defaults to False. | `.Disabled(bool)` |
| label_visibility | Visibility of the label. If "hidden", the label is not shown but still used for accessibility. If "collapsed", the label and spacing are removed. | Not supported |
