namespace Ivy.Widgets.ActivityHeatmap;

/// <summary>Pure helper for computing grid layout and level assignments.</summary>
public static class ActivityHeatmapGrid
{
    /// <summary>
    /// Builds a weeks × 7-days matrix. Pads left to the preceding Sunday of the earliest date,
    /// pads right to the following Saturday of the latest date. Empty days get Count = 0.
    /// </summary>
    public static Activity[][] BuildGrid(Activity[] data)
    {
        if (data.Length == 0)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var end = today;
            var start = today.AddDays(-364);
            return BuildGridFromRange(data, start, end);
        }

        var sorted = data.OrderBy(d => d.Date).ToArray();
        return BuildGridFromRange(data, sorted[0].Date, sorted[^1].Date);
    }

    /// <summary>
    /// Builds a grid between two dates (inclusive), padded to week boundaries.
    /// </summary>
    public static Activity[][] BuildGridFromRange(Activity[] data, DateOnly first, DateOnly last)
    {
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

    /// <summary>Maps a count to a level 0–4 using quartile thresholds.</summary>
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

    /// <summary>Color scheme for activity levels. One of: "primary" (default), "red", "orange", "amber", "yellow", "lime", "green", "emerald", "teal", "cyan", "sky", "blue", "indigo", "violet", "purple", "fuchsia", "pink", "rose".</summary>
    [Prop] public string ColorScheme { get; init; } = "primary";

    /// <summary>Show date and count tooltip on hover.</summary>
    [Prop] public bool ShowTooltip { get; init; } = true;

    /// <summary>Show month labels along the top.</summary>
    [Prop] public bool ShowMonthLabels { get; init; } = true;

    /// <summary>Show weekday labels on the left (Mon, Wed, Fri).</summary>
    [Prop] public bool ShowDayLabels { get; init; } = true;

    /// <summary>Fired when the user clicks a day cell.</summary>
    [Event] public EventHandler<Event<ActivityHeatmap, Activity>>? OnDayClick { get; init; }
}

public static class ActivityHeatmapExtensions
{
    public static ActivityHeatmap Data(this ActivityHeatmap w, Activity[] data) =>
        w with { Data = data };

    public static ActivityHeatmap ColorScheme(this ActivityHeatmap w, Colors scheme) =>
        w with { ColorScheme = scheme.ToString().ToLower() };

    public static ActivityHeatmap ShowTooltip(this ActivityHeatmap w, bool show = true) =>
        w with { ShowTooltip = show };

    public static ActivityHeatmap ShowMonthLabels(this ActivityHeatmap w, bool show = true) =>
        w with { ShowMonthLabels = show };

    public static ActivityHeatmap ShowDayLabels(this ActivityHeatmap w, bool show = true) =>
        w with { ShowDayLabels = show };

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
