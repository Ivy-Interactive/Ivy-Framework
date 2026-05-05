# st.column_config

Column configuration for `st.dataframe` and `st.data_editor`. Provides typed column definitions that control how data is displayed, formatted, and edited. Supports 17 column types including text, numbers, checkboxes, select boxes, dates, links, images, charts, and progress bars.

The closest Ivy equivalent is the **DataTable** widget (`ToDataTable()` extension method), which configures columns via a fluent API. Ivy derives column types from C# property types rather than explicit column type classes. DataTable is primarily display-focused with sorting/filtering, while Streamlit's `st.data_editor` supports inline editing.

## Streamlit

```python
import pandas as pd
import streamlit as st
from datetime import date, datetime

data_df = pd.DataFrame({
    "name": ["Widget A", "Widget B", "Widget C"],
    "price": [20, 950, 250],
    "is_active": [True, False, True],
    "category": ["Tools", "Widgets", "Tools"],
    "website": [
        "https://example.com/a",
        "https://example.com/b",
        "https://example.com/c",
    ],
    "birthday": [date(1980, 1, 1), date(1990, 5, 3), date(2001, 8, 17)],
    "sales": [[10, 20, 30], [40, 50, 60], [70, 80, 90]],
})

st.data_editor(
    data_df,
    column_config={
        "name": st.column_config.TextColumn(
            "Product Name", help="The product name", max_chars=50
        ),
        "price": st.column_config.NumberColumn(
            "Price (USD)", format="$%d", min_value=0, max_value=1000
        ),
        "is_active": st.column_config.CheckboxColumn("Active?"),
        "category": st.column_config.SelectboxColumn(
            "Category", options=["Tools", "Widgets", "Other"]
        ),
        "website": st.column_config.LinkColumn(
            "Website", display_text="Open"
        ),
        "birthday": st.column_config.DateColumn(
            "Birthday", format="DD.MM.YYYY"
        ),
        "sales": st.column_config.BarChartColumn(
            "Sales (last 3 months)", y_min=0, y_max=100
        ),
    },
    hide_index=True,
)
```

## Ivy

```csharp
products.ToDataTable(e => e.Id)
    .Header(e => e.Name, "Product Name")
    .Help(e => e.Name, "The product name")
    .Align(e => e.Price, Align.Right)
    .Renderer(e => e.Website, new LinkDisplayRenderer { Type = LinkDisplayType.Url })
    .Hidden(e => e.InternalId, true)
    .Sortable(e => e.Price, true)
    .Filterable(e => e.Category, true)
    .Config(c =>
    {
        c.FreezeColumns = 1;
        c.AllowSorting = true;
        c.AllowFiltering = true;
        c.ShowIndexColumn = false;
    });
```

---

## Column

Generic column configuration applied to any data type.

```python
st.column_config.Column(
    "Streamlit Widgets",
    help="Streamlit **widget** commands",
    disabled=True,
    required=True,
    pinned=True,
)
```

| Parameter  | Documentation                                                                 | Ivy                                          |
|------------|-------------------------------------------------------------------------------|----------------------------------------------|
| `label`    | The label shown at the top of the column. Defaults to the column name.        | `Header(e => e.Prop, "Label")`               |
| `help`     | Tooltip on column label hover. Supports GitHub-flavored Markdown.             | `Help(e => e.Prop, "Tooltip text")`          |
| `disabled` | Whether editing should be disabled for this column.                           | Not supported                                |
| `required` | Whether edited cells must have a value.                                       | Not supported                                |
| `pinned`   | Whether the column stays visible on the left side when scrolling.             | `Config(c => c.FreezeColumns = N)` (freezes N leftmost columns) |

---

## TextColumn

Configured for string data. Supports character limits and regex validation.

```python
st.column_config.TextColumn(
    "Widgets",
    default="st.",
    max_chars=50,
    validate=r"^st\.[a-z_]+$",
)
```

| Parameter   | Documentation                                                             | Ivy                                |
|-------------|---------------------------------------------------------------------------|-------------------------------------|
| `label`     | Column header label. Defaults to column name.                             | `Header(e => e.Prop, "Label")`     |
| `help`      | Tooltip with Markdown support.                                            | `Help(e => e.Prop, "text")`        |
| `disabled`  | Disable editing for this column.                                          | Not supported                      |
| `required`  | Require non-empty values.                                                 | Not supported                      |
| `pinned`    | Pin column to left side.                                                  | `Config(c => c.FreezeColumns = N)` |
| `default`   | Default value when a new row is added.                                    | Not supported                      |
| `max_chars` | Maximum number of characters allowed.                                     | Not supported                      |
| `validate`  | JS-flavored regex pattern for input validation.                           | Not supported                      |

---

## NumberColumn

Configured for numeric data with formatting and range constraints.

```python
st.column_config.NumberColumn(
    "Price (in USD)",
    min_value=0,
    max_value=1000,
    step=1,
    format="$%d",
)
```

| Parameter   | Documentation                                                          | Ivy                                |
|-------------|------------------------------------------------------------------------|-------------------------------------|
| `label`     | Column header label.                                                   | `Header(e => e.Prop, "Label")`     |
| `help`      | Tooltip with Markdown support.                                         | `Help(e => e.Prop, "text")`        |
| `disabled`  | Disable editing.                                                       | Not supported                      |
| `required`  | Require non-empty values.                                              | Not supported                      |
| `pinned`    | Pin column to left side.                                               | `Config(c => c.FreezeColumns = N)` |
| `default`   | Default value for new rows.                                            | Not supported                      |
| `format`    | Display format (printf-style, localized, percent, dollar, etc.).       | `Renderer()` with custom formatting |
| `min_value` | Minimum value that can be entered.                                     | Not supported                      |
| `max_value` | Maximum value that can be entered.                                     | Not supported                      |
| `step`      | Stepping interval / precision for input.                               | Not supported                      |

---

## CheckboxColumn

Configured for boolean data displayed as checkboxes.

```python
st.column_config.CheckboxColumn(
    "Your favorite?",
    help="Select your **favorite** widgets",
    default=False,
)
```

| Parameter  | Documentation                                            | Ivy                                |
|------------|----------------------------------------------------------|-------------------------------------|
| `label`    | Column header label.                                     | `Header(e => e.Prop, "Label")`     |
| `help`     | Tooltip with Markdown support.                           | `Help(e => e.Prop, "text")`        |
| `disabled` | Disable editing.                                         | Not supported                      |
| `required` | Require non-empty values.                                | Not supported                      |
| `pinned`   | Pin column to left side.                                 | `Config(c => c.FreezeColumns = N)` |
| `default`  | Default value for new rows.                              | Not supported                      |

---

## SelectboxColumn

Single-selection dropdown within a cell.

```python
st.column_config.SelectboxColumn(
    "App Category",
    options=["Data Exploration", "Data Visualization", "LLM"],
    required=True,
)
```

| Parameter     | Documentation                                                   | Ivy           |
|---------------|-----------------------------------------------------------------|---------------|
| `label`       | Column header label.                                            | `Header()`    |
| `help`        | Tooltip with Markdown support.                                  | `Help()`      |
| `disabled`    | Disable editing.                                                | Not supported |
| `required`    | Require non-empty values.                                       | Not supported |
| `pinned`      | Pin column to left side.                                        | `Config(c => c.FreezeColumns = N)` |
| `default`     | Default value for new rows.                                     | Not supported |
| `options`     | Available choices for selection. Inferred from category dtype.  | Not supported |
| `format_func` | Function to modify option display labels.                      | Not supported |

---

## MultiselectColumn

Multiple-selection tags within a cell.

```python
st.column_config.MultiselectColumn(
    "App Categories",
    options=["exploration", "visualization", "llm"],
    color=["#ffa421", "#803df5", "#00c0f2"],
    format_func=lambda x: x.capitalize(),
)
```

| Parameter            | Documentation                                               | Ivy           |
|----------------------|--------------------------------------------------------------|---------------|
| `label`              | Column header label.                                        | `Header()`    |
| `help`               | Tooltip with Markdown support.                              | `Help()`      |
| `disabled`           | Disable editing.                                            | Not supported |
| `required`           | Require non-empty values.                                   | Not supported |
| `pinned`             | Pin column to left side.                                    | `Config(c => c.FreezeColumns = N)` |
| `default`            | Default value for new rows.                                 | Not supported |
| `options`            | Available selections during editing.                        | Not supported |
| `accept_new_options` | Whether user can add selections not in options.             | Not supported |
| `color`              | Color styling for tags (CSS color, hex, "auto").            | Not supported |
| `format_func`        | Function to modify option display labels.                   | Not supported |

---

## DatetimeColumn

Configured for datetime data with format and range constraints.

```python
st.column_config.DatetimeColumn(
    "Appointment",
    min_value=datetime(2023, 6, 1),
    max_value=datetime(2025, 1, 1),
    format="D MMM YYYY, h:mm a",
    step=60,
)
```

| Parameter   | Documentation                                                                 | Ivy                                |
|-------------|-------------------------------------------------------------------------------|-------------------------------------|
| `label`     | Column header label.                                                          | `Header(e => e.Prop, "Label")`     |
| `help`      | Tooltip with Markdown support.                                                | `Help(e => e.Prop, "text")`        |
| `disabled`  | Disable editing.                                                              | Not supported                      |
| `required`  | Require non-empty values.                                                     | Not supported                      |
| `pinned`    | Pin column to left side.                                                      | `Config(c => c.FreezeColumns = N)` |
| `default`   | Default datetime value for new rows.                                          | Not supported                      |
| `format`    | Display format ("localized", "distance", "calendar", "iso8601", or momentJS). | `Renderer()` with custom formatting |
| `min_value` | Minimum allowable datetime.                                                   | Not supported                      |
| `max_value` | Maximum allowable datetime.                                                   | Not supported                      |
| `step`      | Stepping interval in seconds.                                                 | Not supported                      |
| `timezone`  | Timezone for display.                                                         | Not supported                      |

---

## DateColumn

Configured for date-only data.

```python
st.column_config.DateColumn(
    "Birthday",
    min_value=date(1900, 1, 1),
    max_value=date(2005, 1, 1),
    format="DD.MM.YYYY",
    step=1,
)
```

| Parameter   | Documentation                                                           | Ivy                                |
|-------------|-------------------------------------------------------------------------|-------------------------------------|
| `label`     | Column header label.                                                    | `Header(e => e.Prop, "Label")`     |
| `help`      | Tooltip with Markdown support.                                          | `Help(e => e.Prop, "text")`        |
| `disabled`  | Disable editing.                                                        | Not supported                      |
| `required`  | Require non-empty values.                                               | Not supported                      |
| `pinned`    | Pin column to left side.                                                | `Config(c => c.FreezeColumns = N)` |
| `default`   | Default date value for new rows.                                        | Not supported                      |
| `format`    | Display format ("localized", "distance", "iso8601", or momentJS).       | `Renderer()` with custom formatting |
| `min_value` | Minimum date allowed.                                                   | Not supported                      |
| `max_value` | Maximum date allowed.                                                   | Not supported                      |
| `step`      | Stepping interval in days.                                              | Not supported                      |

---

## TimeColumn

Configured for time-only data.

```python
st.column_config.TimeColumn(
    "Appointment",
    min_value=time(8, 0, 0),
    max_value=time(19, 0, 0),
    format="hh:mm a",
    step=60,
)
```

| Parameter   | Documentation                                                     | Ivy                                |
|-------------|-------------------------------------------------------------------|-------------------------------------|
| `label`     | Column header label.                                              | `Header(e => e.Prop, "Label")`     |
| `help`      | Tooltip with Markdown support.                                    | `Help(e => e.Prop, "text")`        |
| `disabled`  | Disable editing.                                                  | Not supported                      |
| `required`  | Require non-empty values.                                         | Not supported                      |
| `pinned`    | Pin column to left side.                                          | `Config(c => c.FreezeColumns = N)` |
| `default`   | Default time value for new rows.                                  | Not supported                      |
| `format`    | Display format ("localized", "iso8601", or momentJS string).      | `Renderer()` with custom formatting |
| `min_value` | Minimum time allowed.                                             | Not supported                      |
| `max_value` | Maximum time allowed.                                             | Not supported                      |
| `step`      | Stepping interval in seconds.                                     | Not supported                      |

---

## LinkColumn

Configured for URL/hyperlink data with validation and custom display text.

```python
st.column_config.LinkColumn(
    "Trending apps",
    validate=r"^https://[a-z]+\.streamlit\.app$",
    max_chars=100,
    display_text=r"https://(.*?)\.streamlit\.app",
)
```

| Parameter      | Documentation                                                                | Ivy                                                                       |
|----------------|------------------------------------------------------------------------------|---------------------------------------------------------------------------|
| `label`        | Column header label.                                                         | `Header(e => e.Prop, "Label")`                                           |
| `help`         | Tooltip with Markdown support.                                               | `Help(e => e.Prop, "text")`                                              |
| `disabled`     | Disable editing.                                                             | Not supported                                                            |
| `required`     | Require non-empty values.                                                    | Not supported                                                            |
| `pinned`       | Pin column to left side.                                                     | `Config(c => c.FreezeColumns = N)`                                       |
| `default`      | Default URL for new rows.                                                    | Not supported                                                            |
| `max_chars`    | Maximum character limit.                                                     | Not supported                                                            |
| `validate`     | JS regex for URL validation.                                                 | Not supported                                                            |
| `display_text` | Custom display text, or regex to extract from URL. None shows full URL.      | `Renderer(e => e.Prop, new LinkDisplayRenderer { Type = LinkDisplayType.Url })` |

---

## ImageColumn

Displays images from URLs or base64 data. Read-only.

```python
st.column_config.ImageColumn(
    "Preview Image",
    help="Streamlit app preview screenshots",
)
```

| Parameter | Documentation                           | Ivy                                |
|-----------|-----------------------------------------|-------------------------------------|
| `label`   | Column header label.                    | `Header(e => e.Prop, "Label")`     |
| `help`    | Tooltip with Markdown support.          | `Help(e => e.Prop, "text")`        |
| `pinned`  | Pin column to left side.                | `Config(c => c.FreezeColumns = N)` |

> Ivy DataTable does not have a built-in image column renderer. Use `Renderer()` with a custom builder for image display.

---

## LineChartColumn

Displays an inline line chart within each cell. Read-only.

```python
st.column_config.LineChartColumn(
    "Sales (last 6 months)",
    y_min=0,
    y_max=100,
)
```

| Parameter | Documentation                                              | Ivy           |
|-----------|-------------------------------------------------------------|---------------|
| `label`   | Column header label.                                        | `Header()`    |
| `help`    | Tooltip with Markdown support.                              | `Help()`      |
| `pinned`  | Pin column to left side.                                    | `Config(c => c.FreezeColumns = N)` |
| `y_min`   | Minimum value on the y-axis for all cells.                  | Not supported |
| `y_max`   | Maximum value on the y-axis for all cells.                  | Not supported |
| `color`   | Chart color: "auto" (green/red), "auto-inverse", or hex.   | Not supported |

> Ivy does not support inline charts in DataTable columns. Use a separate `LineChart` widget.

---

## BarChartColumn

Displays an inline bar chart within each cell. Read-only.

```python
st.column_config.BarChartColumn(
    "Sales (last 6 months)",
    y_min=0,
    y_max=100,
)
```

| Parameter | Documentation                                              | Ivy           |
|-----------|-------------------------------------------------------------|---------------|
| `label`   | Column header label.                                        | `Header()`    |
| `help`    | Tooltip with Markdown support.                              | `Help()`      |
| `pinned`  | Pin column to left side.                                    | `Config(c => c.FreezeColumns = N)` |
| `y_min`   | Minimum value on the y-axis for all cells.                  | Not supported |
| `y_max`   | Maximum value on the y-axis for all cells.                  | Not supported |
| `color`   | Chart color: "auto" (green/red), "auto-inverse", or hex.   | Not supported |

> Ivy does not support inline charts in DataTable columns. Use a separate `BarChart` widget.

---

## AreaChartColumn

Displays an inline area chart within each cell. Read-only.

```python
st.column_config.AreaChartColumn(
    "Sales (last 6 months)",
    y_min=0,
    y_max=100,
)
```

| Parameter | Documentation                                              | Ivy           |
|-----------|-------------------------------------------------------------|---------------|
| `label`   | Column header label.                                        | `Header()`    |
| `help`    | Tooltip with Markdown support.                              | `Help()`      |
| `pinned`  | Pin column to left side.                                    | `Config(c => c.FreezeColumns = N)` |
| `y_min`   | Minimum value on the y-axis for all cells.                  | Not supported |
| `y_max`   | Maximum value on the y-axis for all cells.                  | Not supported |
| `color`   | Chart color: "auto" (green/red), "auto-inverse", or hex.   | Not supported |

> Ivy does not support inline charts in DataTable columns. Use a separate `AreaChart` widget.

---

## ProgressColumn

Displays a progress bar based on numeric values. Read-only.

```python
st.column_config.ProgressColumn(
    "Sales volume",
    format="$%f",
    min_value=0,
    max_value=1000,
)
```

| Parameter   | Documentation                                                                   | Ivy           |
|-------------|---------------------------------------------------------------------------------|---------------|
| `label`     | Column header label.                                                            | `Header()`    |
| `help`      | Tooltip with Markdown support.                                                  | `Help()`      |
| `pinned`    | Pin column to left side.                                                        | `Config(c => c.FreezeColumns = N)` |
| `format`    | Number format (printf-style, localized, percent, dollar, etc.).                 | Not supported |
| `min_value` | Progress bar minimum (defaults to 0).                                           | Not supported |
| `max_value` | Progress bar maximum (defaults to 100 for ints, 1.0 for floats).               | Not supported |
| `step`      | Number precision.                                                               | Not supported |
| `color`     | Bar color: "auto" (green/red based on threshold), "auto-inverse", or hex/CSS.   | Not supported |

> Ivy does not support inline progress bars in DataTable columns. Use a separate `Progress` widget.

---

## JsonColumn

Displays JSON data in an expandable viewer. Read-only.

```python
st.column_config.JsonColumn(
    "JSON Data",
    help="JSON strings or objects",
)
```

| Parameter | Documentation                           | Ivy                                |
|-----------|-----------------------------------------|-------------------------------------|
| `label`   | Column header label.                    | `Header(e => e.Prop, "Label")`     |
| `help`    | Tooltip with Markdown support.          | `Help(e => e.Prop, "text")`        |
| `pinned`  | Pin column to left side.                | `Config(c => c.FreezeColumns = N)` |

> Ivy DataTable does not have a dedicated JSON column renderer. For JSON display, use a separate `Json` primitive widget.

---

## ListColumn

Displays list/array data as comma-separated values within a cell.

```python
st.column_config.ListColumn(
    "Sales (last 6 months)",
    help="The sales volume in the last 6 months",
)
```

| Parameter  | Documentation                           | Ivy                                |
|------------|-----------------------------------------|-------------------------------------|
| `label`    | Column header label.                    | `Header(e => e.Prop, "Label")`     |
| `help`     | Tooltip with Markdown support.          | `Help(e => e.Prop, "text")`        |
| `disabled` | Disable editing.                        | Not supported                      |
| `required` | Require non-empty values.               | Not supported                      |
| `pinned`   | Pin column to left side.                | `Config(c => c.FreezeColumns = N)` |
| `default`  | Default list value for new rows.        | Not supported                      |
