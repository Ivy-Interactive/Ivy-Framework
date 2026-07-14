# Ivy Framework Weekly Notes - Week of 2026-06-03

> [!NOTE]
> We usually release on Fridays every week. Sign up on [https://ivy.app/](https://ivy.app/auth/sign-up) to get release notes directly to your inbox.

## New Features

### Activity Heatmap Builder API
We have introduced a powerful new `ActivityHeatmapBuilder<TSource>` to simplify binding and aggregating structured data sources into a GitHub-style activity calendar. Instead of manually constructing arrays of `Activity` records, you can now use fluent builder methods to perform server-side grouping and aggregation.

Key capabilities include:
- **Fluent Aggregation**: Support for standard aggregation metrics via `ToActivityHeatmap` (e.g., Sum, Count, Average, Min, Max).
- **Auto-detected Granularity**: Automatically groups timestamps into daily or hourly calendar cells.
- **Hourly Granularity**: Full support for hourly interval rendering (`ActivityInterval.Hourly`), displaying activity distributions across hours of the day.

#### Example Usage
```csharp
using Ivy;
using Ivy.Widgets.ActivityHeatmap;

public class MyHeatmapApp : ViewBase
{
    private IQueryable<UserAction> _actions;

    public override object Build() =>
        _actions.ToActivityHeatmap(
            dimension: action => action.Timestamp,
            measure: action => action.PayloadSize
        )
        .ColorScheme(Colors.Emerald)
        .ShowTooltip(true)
        .ValueLabel("Bytes Transferred")
        .OnDayClick(action => {
            Console.WriteLine($"Day clicked: {action.Date}, Value: {action.Value}");
        });
}
```

## Bug Fixes and Improvements

### Desktop Window Icon Extraction Console Noise
Removed redundant console debug logs generated when extracting embedded window icons in `DesktopWindow.cs` (#4559), resulting in cleaner startup logs for desktop-deployed applications.

### Activity Heatmap Pivot Aggregations
Fixed an issue where multiple timestamps falling on the same day/hour cell were improperly flattened. The builder now groups pivots by granular cell timestamps before performing the selected aggregation (Average/Min/Max/Count), and eliminates duplicate date entries in the data grid.

### Markdown Spacings
Resolved a spacing bug in `MarkdownRenderer.tsx` that caused incorrect layout gaps when rendering nested media elements (like images or videos inside list items or customized block layouts).
