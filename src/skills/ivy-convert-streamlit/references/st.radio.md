# st.radio

Display a radio button widget. Allows the user to select a single option from a list of mutually exclusive choices.

## Streamlit

```python
import streamlit as st

genre = st.radio(
    "What's your favorite movie genre",
    ["Comedy", "Drama", "Documentary"],
    captions=["Laugh out loud.", "Get the popcorn.", "Never stop learning."],
)

if genre == "Comedy":
    st.write("You selected comedy.")
else:
    st.write("You didn't select comedy.")
```

## Ivy

Ivy has no dedicated radio widget. The closest equivalent is `SelectInput` with the `Toggle` variant for inline button selection, or the default `Select` variant for a dropdown.

```csharp
var genre = UseState("Comedy");

return new[]
{
    genre.ToSelectInput(
            new[] { "Comedy", "Drama", "Documentary" }.ToOptions())
        .Variant(SelectInputs.Toggle)
        .WithField()
        .Label("What's your favorite movie genre"),

    genre.Value == "Comedy"
        ? new TextBlock("You selected comedy.")
        : new TextBlock("You didn't select comedy.")
};
```

## Parameters

| Parameter        | Documentation                                                                                          | Ivy                                                                                  |
|------------------|--------------------------------------------------------------------------------------------------------|--------------------------------------------------------------------------------------|
| label            | A short label explaining the radio group. Supports GitHub-flavored markdown.                           | `.WithField().Label("...")`                                                          |
| options          | Labels for the selectable options. Can be a list, set, or dataframe column.                            | `IEnumerable<IAnyOption>` passed via constructor or `.ToSelectInput(options)`         |
| index            | Index of the preselected option on first render. `None` for no selection. Default `0`.                 | Initial value set on the state, e.g. `UseState("Comedy")`                            |
| format_func      | Function to modify display labels without affecting the return value.                                  | Custom `IAnyOption` instances with separate display text and value                   |
| help             | Tooltip shown next to the radio group label.                                                           | Not supported                                                                        |
| on_change        | Callback invoked when the selection changes.                                                           | `onChange` parameter or `OnChange` event                                             |
| disabled         | If `True`, the radio group is greyed out. Default `False`.                                             | `.Disabled(true)`                                                                    |
| horizontal       | If `True`, options are displayed horizontally. Default `False`.                                        | `.Variant(SelectInputs.Toggle)` renders options as inline toggle buttons             |
| captions         | Captions displayed below each radio option.                                                            | Not supported                                                                        |
| label_visibility | Controls label visibility: `"visible"`, `"hidden"`, or `"collapsed"`. Default `"visible"`.            | Not supported                                                                        |
