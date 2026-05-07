# st.time_input

Display a time input widget that allows users to select a time value.

## Streamlit

```python
import datetime
import streamlit as st

t = st.time_input("Set an alarm for", datetime.time(8, 45))
st.write("Alarm is set for", t)
```

## Ivy

```csharp
var timeState = UseState(DateTime.Now);
return timeState.ToTimeInput()
    .Format("HH:mm:ss")
    .WithField()
    .Label("Select Time");
```

## Parameters

| Parameter        | Documentation                                                                                          | Ivy                                                             |
|------------------|--------------------------------------------------------------------------------------------------------|-----------------------------------------------------------------|
| label            | A short label explaining the widget's purpose. Supports Markdown.                                      | `.WithField().Label("...")`                                     |
| value            | Initial value. Accepts `"now"`, `datetime.time`, `datetime.datetime`, ISO string, or `None`. Default: `"now"` | State initializer, e.g. `UseState(DateTime.Now)`                |
| help             | Tooltip text displayed next to the label. Default: `None`                                              | Not supported                                                   |
| on_change        | Callback triggered when the value changes.                                                             | `.OnChange(handler)`                                            |
| disabled         | Disables the widget when `True`. Default: `False`                                                      | `.Disabled(true)`                                               |
| label_visibility | Controls label display: `"visible"`, `"hidden"`, or `"collapsed"`. Default: `"visible"`                | Not supported                                                   |
| step             | Stepping interval in seconds (60s to 23h). Default: `900` (15 minutes)                                | Not supported                                                   |
