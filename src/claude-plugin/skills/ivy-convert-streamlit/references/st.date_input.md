# st.date_input

Display a date input widget. Supports single date selection and date range selection. The first day of the week is determined by the user's browser locale.

## Streamlit

```python
import datetime
import streamlit as st

d = st.date_input("When's your birthday", datetime.date(2019, 7, 6))
st.write("Your birthday is:", d)

# Date range
today = datetime.datetime.now()
next_year = today.year + 1
jan_1 = datetime.date(next_year, 1, 1)
dec_31 = datetime.date(next_year, 12, 31)
d = st.date_input(
    "Select your vacation for next year",
    (jan_1, datetime.date(next_year, 1, 7)),
    jan_1, dec_31,
    format="MM.DD.YYYY",
)
```

## Ivy

Ivy splits this into two widgets: `DateTimeInput` (with `ToDateInput()`) for single dates and `DateRangeInput` (with `ToDateRangeInput()`) for date ranges.

```csharp
// Single date
var dateState = this.UseState(() => new DateOnly(2019, 7, 6));

return Layout.Vertical()
    | dateState.ToDateInput()
        .Placeholder("When's your birthday")
        .Format("yyyy/MM/dd");

// Date range
var rangeState = this.UseState(() =>
    (from: new DateOnly(2025, 1, 1), to: new DateOnly(2025, 1, 7)));

return Layout.Vertical()
    | rangeState.ToDateRangeInput()
        .Placeholder("Select your vacation for next year")
        .Format("MM.dd.yyyy");
```

## Parameters

| Parameter        | Documentation                                                                                              | Ivy                                                                 |
|------------------|------------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------|
| label            | Brief label shown above the widget. Supports markdown.                                                     | `Placeholder` (hint text inside the input, no external label)       |
| value            | Initial value. Accepts `"today"`, `datetime.date`, a tuple of dates for ranges, or `None`.                 | `Value` / state initializer; use `Nullable` to allow empty state    |
| min_value        | Earliest selectable date. Defaults to 10 years before the initial value.                                   | Not supported                                                       |
| max_value        | Latest selectable date. Defaults to 10 years after the initial value.                                      | Not supported                                                       |
| help             | Tooltip shown next to the label.                                                                           | Not supported                                                       |
| on_change        | Callback invoked when the value changes.                                                                   | `OnChange`                                                          |
| format           | Display format string (`"YYYY/MM/DD"`, `"DD/MM/YYYY"`, `"MM/DD/YYYY"` with `/`, `.`, or `-` separators).  | `Format` (.NET format strings, e.g. `"yyyy-MM-dd"`)                |
| disabled         | Disables the widget when `True`.                                                                           | `Disabled`                                                          |
| label_visibility | Controls whether the label is `"visible"`, `"hidden"`, or `"collapsed"`.                                   | `Visible` (controls entire widget visibility, not just the label)   |
