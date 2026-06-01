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

    [Prop] public ActivityInterval Interval { get; init; } = ActivityInterval.Daily;

    [Prop] public string ValueLabel { get; init; } = "Count";

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

    public static ActivityHeatmap Interval(this ActivityHeatmap w, ActivityInterval interval) =>
        w with { Interval = interval };

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

public record Activity
{
    public DateOnly Date { get; init; }
    public int? Hour { get; init; }
    public int Count { get; init; }
}

public enum ActivityDimension
{
    Days
}

public enum ActivityInterval
{
    Daily,
    Hourly
}