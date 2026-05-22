namespace Ivy.Widgets.ActivityHeatmap;

[ExternalWidget(
    "frontend/dist/Ivy_Widgets_ActivityHeatmap.js",
    StylePath = "frontend/dist/ivy-widgets-activityheatmap.css",
    ExportName = "ActivityHeatmap",
    GlobalName = "Ivy_Widgets_ActivityHeatmap"
)]
public record ActivityHeatmap : WidgetBase<ActivityHeatmap>
{
    /// <summary>Daily activity data. One entry per active day — missing days are rendered as zero.</summary>
    [Prop] public Activity[] Data { get; init; } = [];

    [Prop] public Colors ColorScheme { get; init; } = Colors.Primary;

    [Prop] public bool ShowTooltip { get; init; } = true;

    [Prop] public bool ShowMonthLabels { get; init; } = true;

    [Prop] public bool ShowDayLabels { get; init; } = true;

    [Prop] public string ValueLabel { get; init; } = string.Empty;

    [Prop] public DateOnly? StartDate { get; init; }

    [Prop] public DateOnly? EndDate { get; init; }

    [Event] public EventHandler<Event<ActivityHeatmap, Activity>>? OnDayClick { get; init; }
}

public static class ActivityHeatmapExtensions
{
    public static ActivityHeatmap Data(this ActivityHeatmap w, Activity[] data) =>
        w with { Data = data };

    public static ActivityHeatmap ColorScheme(this ActivityHeatmap w, Colors scheme) =>
        w with { ColorScheme = scheme };

    public static ActivityHeatmap ShowTooltip(this ActivityHeatmap w, bool show = true) =>
        w with { ShowTooltip = show };

    public static ActivityHeatmap ShowMonthLabels(this ActivityHeatmap w, bool show = true) =>
        w with { ShowMonthLabels = show };

    public static ActivityHeatmap ValueLabel(this ActivityHeatmap w, string valueLabel) =>
        w with { ValueLabel = valueLabel };

    public static ActivityHeatmap ShowDayLabels(this ActivityHeatmap w, bool show = true) =>
        w with { ShowDayLabels = show };

    public static ActivityHeatmap StartDate(this ActivityHeatmap w, DateOnly? date) =>
        w with { StartDate = date };

    public static ActivityHeatmap EndDate(this ActivityHeatmap w, DateOnly? date) =>
        w with { EndDate = date };

    public static ActivityHeatmap OnDayClick(
        this ActivityHeatmap w,
        Func<Event<ActivityHeatmap, Activity>, ValueTask> handler
    ) => w with { OnDayClick = new(handler) };

    public static ActivityHeatmap OnDayClick(
        this ActivityHeatmap w,
        Action<Activity> handler
    ) => w with
    {
        OnDayClick = new(e =>
            {
                handler(e.Value);
                return ValueTask.CompletedTask;
            })
    };
}

public static class ActivityHeatmapGrid
{
    public static Activity[][] BuildGrid(Activity[] data, DateOnly? startDate = null, DateOnly? endDate = null)
    {
        if (data.Length == 0 && startDate == null && endDate == null)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            return BuildGridFromRange(data, today.AddDays(-364), today);
        }

        var sorted = data.OrderBy(d => d.Date).ToArray();
        var first = startDate ?? (sorted.Length > 0 ? sorted[0].Date : DateOnly.FromDateTime(DateTime.Today).AddDays(-364));
        var last = endDate ?? (sorted.Length > 0 ? sorted[^1].Date : DateOnly.FromDateTime(DateTime.Today));
        return BuildGridFromRange(data, first, last);
    }

    public static Activity[][] BuildGridFromRange(Activity[] data, DateOnly first, DateOnly last)
    {
        if (first > last)
            (first, last) = (last, first);

        // Pad left to Sunday (DayOfWeek.Sunday == 0)
        var start = first.AddDays(-(int)first.DayOfWeek);
        // Pad right to Saturday (DayOfWeek.Saturday == 6)
        var end = last.AddDays(6 - (int)last.DayOfWeek);

        var dataMap = data.ToDictionary(d => d.Date);

        var weeks = new List<Activity[]>();
        var current = start;
        while (current <= end)
        {
            var week = new Activity[7];
            for (int d = 0; d < 7; d++)
            {
                week[d] = dataMap.TryGetValue(current, out var day)
                    ? day
                    : new Activity { Date = current, Count = 0 };
                current = current.AddDays(1);
            }
            weeks.Add(week);
        }

        return [.. weeks];
    }

    public static int GetLevel(int count, int maxCount)
    {
        if (count == 0 || maxCount == 0) return 0;
        if (count <= maxCount * 0.25) return 1;
        if (count <= maxCount * 0.50) return 2;
        if (count <= maxCount * 0.75) return 3;
        return 4;
    }
}

public record Activity
{
    public DateOnly Date { get; init; }
    public int Count { get; init; }
}
