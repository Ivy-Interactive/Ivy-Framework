# st.multiselect

Display a multiselect widget. The widget allows users to select multiple options from a provided list and returns a list of the selected values.

## Streamlit

```python
import streamlit as st

options = st.multiselect(
    "What are your favorite colors?",
    ["Green", "Yellow", "Red", "Blue"],
    default=["Yellow", "Red"],
)

st.write("You selected:", options)
```

## Ivy

```csharp
var favorites = UseState(new[] { "Yellow", "Red" });

return favorites.ToSelectInput(
        new[] { "Green", "Yellow", "Red", "Blue" }.ToOptions()
    )
    .Placeholder("Choose your favorite colors")
    .WithField()
    .Label("What are your favorite colors?");
```

## Parameters

| Parameter          | Documentation                                                                                     | Ivy                                                                                       |
|--------------------|---------------------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------|
| label              | Brief label explaining the widget's purpose. Supports Markdown.                                   | `.WithField().Label()` sets the field label                                               |
| options            | The list of selectable options. Can be a list, set, or dataframe column.                          | `IEnumerable<IAnyOption>` passed to constructor; use `.ToOptions()` on a collection        |
| default            | Initial selected values. Accepts a single value or a list. Defaults to `None` (empty).            | Initial value of the state, e.g. `UseState(new[] { "Yellow", "Red" })`                    |
| format_func        | Function to modify option display text without affecting the return value.                         | Custom option rendering via `Option<T>` objects                                            |
| help               | Tooltip text shown next to the label.                                                             | `.WithField().Description()` for helper text                                              |
| on_change          | Callback invoked when the selection changes.                                                      | `OnChange` event: `Func<Event<IInput<TValue>, TValue>, ValueTask>`                       |
| max_selections     | Maximum number of items that can be selected simultaneously.                                      | Not supported                                                                              |
| placeholder        | Text displayed when no option is selected.                                                        | `.Placeholder()` property                                                                  |
| disabled           | If `True`, the widget is greyed out and non-interactive. Defaults to `False`.                     | `.Disabled()` property                                                                     |
| label_visibility   | Controls label display: `"visible"`, `"hidden"` (reserves space), or `"collapsed"` (no space).    | Not supported                                                                              |
| accept_new_options | If `True`, users can type and add values not in the original options list. Defaults to `False`.    | Not supported                                                                              |
