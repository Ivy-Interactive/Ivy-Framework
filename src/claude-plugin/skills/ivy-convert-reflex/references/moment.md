# Moment

Displays dates and relative timestamps with automatic formatting, humanized intervals (e.g. "5 minutes ago"), timezone support, and periodic client-side refresh. Wraps the `react-moment` library.

## Reflex

```python
import reflex as rx
from datetime import datetime

class MomentState(rx.State):
    date: str = datetime.now().isoformat()

def index():
    return rx.vstack(
        # Display as relative time ("5 minutes ago")
        rx.moment(date=MomentState.date, from_now=True),
        # Custom format
        rx.moment(date=MomentState.date, format="YYYY-MM-DD HH:mm"),
        # Auto-refresh every second
        rx.moment(date=MomentState.date, from_now=True, interval=1000),
        # With timezone
        rx.moment(date=MomentState.date, tz="America/New_York"),
    )
```

## Ivy

Ivy has no dedicated Moment widget. Date display is done via C#'s built-in `DateTime` formatting rendered in a `TextBlock`. For periodic refresh, combine with `UseEffect` and a timer.

```csharp
using Ivy;

// Simple formatted date display
var date = UseState(DateTime.Now);

return Layout.Vertical()
    | Text.P(date.Value.ToString("MMMM dd, yyyy HH:mm"))
    | Text.Muted(GetRelativeTime(date.Value));

// For periodic refresh, use UseEffect with a timer
UseEffect(() =>
{
    var timer = new Timer(_ => date.Set(DateTime.Now), null, 0, 1000);
    return timer; // IDisposable, cleaned up automatically
}, Trigger.OnMount);
```

## Parameters

| Parameter        | Description                                                        | Ivy                                                                 |
|------------------|--------------------------------------------------------------------|---------------------------------------------------------------------|
| `date`           | The date to display (str, datetime, date, time, timedelta)         | `UseState<DateTime>` bound to `Text.P(state.Value.ToString(...))` |
| `format`         | Date format string (e.g. `"YYYY-MM-DD"`)                          | `DateTime.ToString("yyyy-MM-dd")` (standard .NET format strings)    |
| `from_now`       | Display as humanized relative time (e.g. "5 minutes ago")         | Not built-in; requires custom helper method                         |
| `from_now_during`| Duration in ms before switching from relative to formatted display | Not supported                                                       |
| `to_now`         | Display time remaining until the date                              | Not built-in; requires custom helper method                         |
| `interval`       | Auto-refresh interval in milliseconds                              | `UseEffect` with `Timer` (disposable pattern)                       |
| `add`            | Add a time offset via `MomentDelta`                                | `DateTime.Add(TimeSpan)` before rendering                           |
| `subtract`       | Subtract a time offset via `MomentDelta`                           | `DateTime.Subtract(TimeSpan)` before rendering                      |
| `tz`             | Display in a specific timezone                                     | `TimeZoneInfo.ConvertTime(...)` before rendering                    |
| `locale`         | Locale for date formatting                                         | `CultureInfo` passed to `DateTime.ToString(..., culture)`           |
| `trim`           | Remove extra spacing from output                                   | Not applicable                                                      |
| `unix`           | Parse input as Unix timestamp                                      | `DateTimeOffset.FromUnixTimeSeconds(...)` before rendering          |
| `on_change`      | Event fired when the displayed date updates                        | Not supported; use `UseEffect` side-effects instead                 |
