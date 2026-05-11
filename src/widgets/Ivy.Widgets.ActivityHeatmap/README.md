# Ivy.Widgets.ActivityHeatmap

GitHub-style activity heatmap widget for Ivy Framework. Renders a 52-week × 7-day activity graph grid.

## Usage

```csharp
using Ivy.Widgets.ActivityHeatmap;

var data = new[]
{
    new Activity { Date = DateOnly.FromDateTime(DateTime.Today), Count = 5 },
};

new ActivityHeatmap()
    .Data(data)
    .ColorScheme("green")
    .OnDayClick(day => Console.WriteLine($"Clicked {day.Date}: {day.Count}"));
```

## Props

| Prop | Type | Default | Description |
|------|------|---------|-------------|
| `Data` | `Activity[]` | `[]` | Daily activity data |
| `ColorScheme` | `string` | `"green"` | Color scheme: green, blue, purple, orange, pink |
| `ShowTooltip` | `bool` | `true` | Show date/count tooltip on hover |
| `ShowMonthLabels` | `bool` | `true` | Show month labels along the top |
| `ShowDayLabels` | `bool` | `true` | Show Mon/Wed/Fri labels on the left |

## Data Constraints

- Duplicate dates in `Data` are not supported.
- Provide at most one `Activity` entry per `DateOnly` day.
- If your source can produce duplicates, aggregate them first (for example by summing counts per date) before passing data to `ActivityHeatmap`.

## Events

| Event | Args | Description |
|-------|------|-------------|
| `OnDayClick` | `Activity` | Fired when user clicks a day cell |
