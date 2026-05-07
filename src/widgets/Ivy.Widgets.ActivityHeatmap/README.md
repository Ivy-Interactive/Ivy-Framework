# Ivy.Widgets.ActivityHeatmap

GitHub-style activity heatmap widget for Ivy Framework. Renders a 52-week × 7-day contribution graph grid.

## Usage

```csharp
using Ivy.Widgets.ActivityHeatmap;

var data = new[]
{
    new ContributionDay { Date = DateOnly.FromDateTime(DateTime.Today), Count = 5 },
};

new ActivityHeatmap()
    .Data(data)
    .ColorScheme("green")
    .OnDayClick(day => Console.WriteLine($"Clicked {day.Date}: {day.Count}"));
```

## Props

| Prop | Type | Default | Description |
|------|------|---------|-------------|
| `Data` | `ContributionDay[]` | `[]` | Daily contribution data |
| `ColorScheme` | `string` | `"green"` | Color scheme: green, blue, purple, orange, pink |
| `ShowTooltip` | `bool` | `true` | Show date/count tooltip on hover |
| `ShowMonthLabels` | `bool` | `true` | Show month labels along the top |
| `ShowDayLabels` | `bool` | `true` | Show Mon/Wed/Fri labels on the left |

## Events

| Event | Args | Description |
|-------|------|-------------|
| `OnDayClick` | `ContributionDay` | Fired when user clicks a day cell |
