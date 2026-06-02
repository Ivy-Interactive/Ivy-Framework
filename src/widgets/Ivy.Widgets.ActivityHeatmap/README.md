# Ivy.Widgets.ActivityHeatmap

GitHub-style activity heatmap widget for Ivy Framework. Renders an activity graph grid, either day-by-day (52-week × 7-day) or hour-by-hour.

## Usage

The recommended way to build a heatmap is the `ToActivityHeatmap()` builder, which projects any
`IEnumerable<T>` / `IQueryable<T>` into the grid using a **dimension** (the date/time axis) and a
**measure** (the aggregated value per cell).

```csharp
using Ivy.Widgets.ActivityHeatmap;

public record RepoStats(DateOnly Date, int Stars, int Downloads);

public class ActivityHeatmapDemo : ViewBase
{
    public override object Build()
    {
        var repoService = UseService<IMyRepoService>();
        var dailyStats = repoService.GetDailyStats().ToList();

        return dailyStats.ToActivityHeatmap()
            .Dimension(ActivityDimension.Days, d => d.Date)
            .Measure("Stars", e => e.Sum(d => d.Stars));
    }
}
```

### Simplified overload with aggregation

When the measure is a single column, use the overload that takes a dimension selector, a measure
selector, and an `ActivityAggregation`. The widget builds the aggregator for you:

```csharp
hourlyStats.ToActivityHeatmap(
    dimension: e => e.Timestamp,
    measure: e => e.Downloads,
    aggregation: ActivityAggregation.Average)
    .ColorScheme(Colors.Emerald)
    .Height(Size.Units(40));
```

`ActivityAggregation` supports `Sum` (default), `Count`, `Average`, `Min`, and `Max`. Numeric
aggregations support `int`, `long`, `float`, `double`, `decimal`, and their nullable variants.

### Daily vs. hourly intervals

The `Interval` controls whether each cell represents a day or an hour:

- `ActivityInterval.Daily` — a 52-week × 7-day grid (GitHub contributions style).
- `ActivityInterval.Hourly` — an hour-by-hour grid.

If you don't set `Interval` explicitly, the builder auto-detects it: when the dimension values carry
a non-zero time component, it uses `Hourly`; otherwise `Daily`. Set it explicitly with `.Interval(...)`
to override.

### Low-level widget

You can also construct the widget directly and supply pre-aggregated data. Each `Activity` should be
unique per cell (one entry per day, or per day+hour for hourly):

```csharp
var data = new[]
{
    new Activity { Date = DateOnly.FromDateTime(DateTime.Today), Count = 5 },
};

new ActivityHeatmap()
    .Data(data)
    .ColorScheme(Colors.Green)
    .OnDayClick(day => Console.WriteLine($"Clicked {day.Date}: {day.Count}"));
```

## Builder API

The builder (`ToActivityHeatmap()`) exposes the configuration below in addition to the widget props.

| Method | Description |
|--------|-------------|
| `Dimension(ActivityDimension, selector)` | Sets the date/time axis. Required (or supply via the `ToActivityHeatmap` overload). |
| `Measure(name, aggregator)` | Sets the aggregated value per cell. Required (or supply via the overload). `name` is also used as the value label. |
| `Interval(ActivityInterval)` | Forces `Daily` or `Hourly`; otherwise auto-detected from the data. |
| `ColorScheme(Colors)` | See props below. |
| `ShowTooltip(bool)` / `ShowMonthLabels(bool)` / `ShowDayLabels(bool)` | See props below. |
| `StartDate(DateOnly?)` / `EndDate(DateOnly?)` | See props below. |
| `OnDayClick(...)` | See events below. |

## Props

These props apply to the `ActivityHeatmap` widget (the builder forwards most of them).

| Prop | Type | Default | Description |
|------|------|---------|-------------|
| `Data` | `Activity[]` | `[]` | Daily/hourly activity data (one entry per cell) |
| `ColorScheme` | `Ivy.Colors` | `Colors.Primary` | Color scheme; supports semantic, chromatic, and neutral color tokens |
| `ShowTooltip` | `bool` | `true` | Show date/count tooltip on hover |
| `ShowMonthLabels` | `bool` | `true` | Show month labels along the top |
| `ShowDayLabels` | `bool` | `true` | Show Mon/Wed/Fri labels on the left |
| `Interval` | `ActivityInterval` | `Daily` | `Daily` or `Hourly` cell granularity |
| `ValueLabel` | `string?` | `null` | Label used for the value in the tooltip; when unset the tooltip falls back to `"Count"` (the builder sets this to the measure name) |
| `StartDate` | `DateOnly?` | `null` | Pins the start of the visible range; when set, overrides the minimum date derived from `Data` |
| `EndDate` | `DateOnly?` | `null` | Pins the end of the visible range; when set, overrides the maximum date derived from `Data` |

If both `StartDate` and `EndDate` are set and `EndDate` is before `StartDate`, the widget treats the range in chronological order (same as swapping the two values), so the grid still renders instead of collapsing to zero weeks.

## Activity

`Activity` is the per-cell data record:

| Field | Type | Description |
|-------|------|-------------|
| `Date` | `DateOnly` | The day this cell represents |
| `Hour` | `int?` | The hour (0–23) for hourly intervals; `null` for daily |
| `Count` | `int` | The aggregated value for the cell |

## Data Constraints

- Duplicate cells in `Data` are not supported — provide at most one `Activity` per `DateOnly` (daily) or per `DateOnly` + `Hour` (hourly).
- If your source can produce duplicates, aggregate them first (for example by summing counts per cell) before passing data to `ActivityHeatmap`. The `ToActivityHeatmap()` builder handles this aggregation for you.

## Events

| Event | Args | Description |
|-------|------|-------------|
| `OnDayClick` | `Activity` | Fired when user clicks a cell |

## Development

### Building

1. Install frontend dependencies:

   ```bash
   cd frontend
   pnpm install
   ```

2. Build the frontend:

   ```bash
   pnpm build
   ```

3. Build the widget (repository root, or any path that builds `Ivy.Widgets.ActivityHeatmap.csproj`):

   ```bash
   dotnet build
   ```

   The widget project uses Ivy’s external-widget MSBuild targets: `dotnet build` runs `vp install` / `vp build` in `frontend` when needed, so after dependencies exist you can rely on that instead of repeating steps 1–2.

### Frontend watch

While changing React/TypeScript under `frontend/src`, run a watch build in another terminal:

```bash
cd frontend
pnpm exec vp build --watch
```

### Sample app

```bash
cd .samples
dotnet run
```
