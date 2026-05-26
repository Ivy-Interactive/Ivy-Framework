using System.Linq.Expressions;
using Ivy.Core.Helpers;

namespace Ivy.Widgets.ActivityHeatmap;

public class ActivityHeatmapBuilder<TSource>(
    IQueryable<TSource> data,
    Dimension<TSource>? dimension = null,
    Measure<TSource>? measure = null,
    Func<ActivityHeatmap, ActivityHeatmap>? polish = null
) : ViewBase
{
    private Colors _colorScheme = Colors.Primary;
    private bool _showTooltip = true;
    private bool _showMonthLabels = true;
    private bool _showDayLabels = true;
    private DateOnly? _startDate;
    private DateOnly? _endDate;
    private EventHandler<Event<ActivityHeatmap, Activity>>? _onDayClick;
    private Dimension<TSource>? _dimension = dimension;
    private Measure<TSource>? _measure = measure;

    public override object Build()
    {
        if (_dimension is null)
            throw new InvalidOperationException("A dimension is required.");
        if (_measure is null)
            throw new InvalidOperationException("A measure is required.");

        var activityData = UseState(Array.Empty<Activity>());
        var loading = UseState(true);

        UseEffect(async () =>
        {
            try
            {
                var results = await data
                    .ToPivotTable()
                    .Dimension(_dimension)
                    .Measure(_measure)
                    .ExecuteAsync();

                var activities = results
                    .Select(row => new Activity
                    {
                        Date = ToDateOnly(row[_dimension.Name]),
                        Count = Convert.ToInt32(row[_measure.Name])
                    })
                    .GroupBy(a => a.Date)
                    .Select(g => new Activity { Date = g.Key, Count = g.Sum(a => a.Count) })
                    .ToArray();

                activityData.Set(activities);
            }
            finally
            {
                loading.Set(false);
            }
        }, [EffectTrigger.OnMount()]);

        if (loading.Value)
            return new ChatLoading();

        var widget = new ActivityHeatmap()
            .Data(activityData.Value)
            .ColorScheme(_colorScheme)
            .ShowTooltip(_showTooltip)
            .ShowMonthLabels(_showMonthLabels)
            .ShowDayLabels(_showDayLabels)
            .ValueLabel(_measure.Name)
            .StartDate(_startDate)
            .EndDate(_endDate);

        if (_onDayClick is not null)
            widget = widget with { OnDayClick = _onDayClick };

        return polish?.Invoke(widget) ?? widget;
    }

    public ActivityHeatmapBuilder<TSource> Dimension(string name, Expression<Func<TSource, object>> selector)
    {
        _dimension = new Dimension<TSource>(name, selector);
        return this;
    }

    public ActivityHeatmapBuilder<TSource> Measure(string name, Expression<Func<IQueryable<TSource>, object>> aggregator)
    {
        _measure = new Measure<TSource>(name, aggregator);
        return this;
    }

    public ActivityHeatmapBuilder<TSource> ColorScheme(Colors scheme) { _colorScheme = scheme; return this; }
    public ActivityHeatmapBuilder<TSource> ShowTooltip(bool show = true) { _showTooltip = show; return this; }
    public ActivityHeatmapBuilder<TSource> ShowMonthLabels(bool show = true) { _showMonthLabels = show; return this; }
    public ActivityHeatmapBuilder<TSource> ShowDayLabels(bool show = true) { _showDayLabels = show; return this; }
    public ActivityHeatmapBuilder<TSource> StartDate(DateOnly? date) { _startDate = date; return this; }
    public ActivityHeatmapBuilder<TSource> EndDate(DateOnly? date) { _endDate = date; return this; }

    public ActivityHeatmapBuilder<TSource> OnDayClick(Func<Event<ActivityHeatmap, Activity>, ValueTask> handler)
    {
        _onDayClick = new(handler);
        return this;
    }

    public ActivityHeatmapBuilder<TSource> OnDayClick(Action<Activity> handler)
    {
        _onDayClick = new(e => { handler(e.Value); return ValueTask.CompletedTask; });
        return this;
    }

    internal static DateOnly ToDateOnly(object value) => value switch
    {
        DateOnly d => d,
        DateTime dt => DateOnly.FromDateTime(dt),
        DateTimeOffset dto => DateOnly.FromDateTime(dto.DateTime),
        string s => DateOnly.Parse(s),
        _ => DateOnly.Parse(value.ToString()!)
    };
}

public static class ActivityHeatmapBuilderExtensions
{
    public static ActivityHeatmapBuilder<TSource> ToActivityHeatmap<TSource>(
        this IEnumerable<TSource> data,
        Expression<Func<TSource, object>>? dimension = null,
        Expression<Func<IQueryable<TSource>, object>>? measure = null)
    {
        return data.AsQueryable().ToActivityHeatmap(dimension, measure);
    }

    [System.Runtime.CompilerServices.OverloadResolutionPriority(1)]
    public static ActivityHeatmapBuilder<TSource> ToActivityHeatmap<TSource>(
        this IQueryable<TSource> data,
        Expression<Func<TSource, object>>? dimension = null,
        Expression<Func<IQueryable<TSource>, object>>? measure = null)
    {
        return new ActivityHeatmapBuilder<TSource>(
            data,
            dimension != null ? new Dimension<TSource>(ExpressionNameHelper.SuggestName(dimension) ?? "Dimension", dimension) : null,
            measure != null ? new Measure<TSource>(ExpressionNameHelper.SuggestName(measure) ?? "Measure", measure) : null
        );
    }
}
