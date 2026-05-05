# st.selectbox

Displays a dropdown selection widget that allows users to choose a single option from a list.

## Streamlit

```python
import streamlit as st

option = st.selectbox(
    "How would you like to be contacted?",
    ("Email", "Home phone", "Mobile phone"),
    index=None,
    placeholder="Select contact method...",
)

st.write("You selected:", option)
```

## Ivy

```csharp
var contact = UseState("Email");

return contact.ToSelectInput(
        new[] { "Email", "Home phone", "Mobile phone" }.ToOptions())
    .Placeholder("Select contact method...")
    .WithField()
    .Label("How would you like to be contacted?");
```

## Parameters

| Parameter          | Streamlit                                                                 | Ivy                                                        |
|--------------------|---------------------------------------------------------------------------|------------------------------------------------------------|
| label              | Short label explaining the widget's purpose                               | Use `.WithField().Label(...)` to attach a label            |
| options            | Iterable of options (list, tuple, set, dataframe column)                  | `IEnumerable<IAnyOption>` via `.ToOptions()`               |
| index              | Index of the preselected option (default `0`); `None` for empty          | Set via initial `UseState` value                           |
| format_func        | Function to modify display labels without affecting return value          | Custom option rendering via `IAnyOption` configuration     |
| help               | Tooltip text shown next to the label                                      | Not supported                                              |
| on_change          | Callback invoked when selection changes                                   | `OnChange` event handler                                   |
| placeholder        | Text shown when no option is selected                                     | `.Placeholder(...)` method                                 |
| disabled           | Disables the widget when `True`                                           | `.Disabled(bool)` method                                   |
| label_visibility   | Controls label display: `"visible"`, `"hidden"`, or `"collapsed"`         | Not supported                                              |
| accept_new_options | Allows the user to type and add new options not in the original list      | Not supported                                              |
