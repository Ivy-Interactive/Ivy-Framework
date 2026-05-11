# st.datetime_input

Display a date and time input widget, allowing users to select both a date and time through an interactive interface.

## Streamlit

```python
import datetime
import streamlit as st

event_time = st.datetime_input(
    "Schedule your event",
    datetime.datetime(2025, 11, 19, 16, 45),
)
st.write("Event scheduled for", event_time)
```

## Ivy

```csharp
var dateTimeState = UseState(new DateTime(2025, 11, 19, 16, 45, 0));

return dateTimeState.ToDateTimeInput()
    .Format("yyyy/MM/dd HH:mm")
    .WithField()
    .Label("Schedule your event");
```

## Parameters

| Parameter        | Documentation                                                                                          | Ivy                                                        |
|------------------|--------------------------------------------------------------------------------------------------------|------------------------------------------------------------|
| label            | Brief label explaining the widget's purpose. Supports GitHub-flavored Markdown.                        | `.WithField().Label("...")`                                |
| value            | Initial value. `"now"`, `datetime`, `date`, `time`, `str`, or `None`. Default: `"now"`.                | `UseState(DateTime.Now)` / `UseState<DateTime?>(null)`     |
| min_value        | Minimum selectable datetime. Defaults to ten years before initial value.                               | Not supported                                              |
| max_value        | Maximum selectable datetime. Defaults to ten years after initial value.                                | Not supported                                              |
| help             | Tooltip displayed next to the label.                                                                   | Not supported                                              |
| on_change        | Callback invoked when the value changes.                                                               | `.OnChange(e => ...)`                                      |
| format           | Date display format (e.g. `"YYYY/MM/DD"`, `"DD/MM/YYYY"`). Default: `"YYYY/MM/DD"`.                   | `.Format("yyyy/MM/dd")` using .NET format strings          |
| step             | Time stepping interval (60s–23h). Default: 900 seconds (15 min).                                      | Not supported                                              |
| disabled         | Disables the widget when `True`. Default: `False`.                                                     | `.Disabled(true)`                                          |
| label_visibility | Controls label visibility: `"visible"`, `"hidden"`, or `"collapsed"`. Default: `"visible"`.            | Not supported                                              |
