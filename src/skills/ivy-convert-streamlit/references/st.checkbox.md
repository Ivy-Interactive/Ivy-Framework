# st.checkbox

Display a checkbox widget that returns a boolean value indicating whether the checkbox is checked.

## Streamlit

```python
import streamlit as st

agree = st.checkbox("I agree")

if agree:
    st.write("Great!")
```

## Ivy

```csharp
var state = UseState(false);
return state.ToBoolInput().Label("I agree");
```

## Parameters

| Parameter        | Documentation                                                                                                          | Ivy                                                              |
|------------------|------------------------------------------------------------------------------------------------------------------------|------------------------------------------------------------------|
| label            | A short label explaining to the user what this checkbox is for. Supports GitHub-flavored Markdown.                     | `Label` property on `BoolInput`                                  |
| value            | Initial state when checkbox first renders; cast to bool internally. Defaults to `False`.                               | Initial value passed via `IAnyState` or `bool value` constructor |
| help             | Tooltip displayed next to the checkbox. Supports GitHub-flavored Markdown.                                             | `Description` property on `BoolInput`                            |
| on_change        | Optional callback invoked when the checkbox value changes.                                                             | `OnChange` event                                                 |
| disabled         | If `True`, disables the checkbox. Defaults to `False`.                                                                 | `Disabled` property on `BoolInput`                               |
| label_visibility | Controls label display: `"visible"` (default), `"hidden"` (empty spacer), or `"collapsed"` (no label and no spacer).  | Not supported                                                    |
