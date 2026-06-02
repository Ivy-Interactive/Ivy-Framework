using System.Linq.Expressions;
using Ivy.Core.Helpers;

namespace Ivy.Widgets.ActivityHeatmap;

public class ActivityHeatmapBuilder<TSource>(
    IQueryable<TSource> data,
    Dimension<TSource> dimension,
    Measure<TSource> measure,
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
    private Measure<TSource> _measure = measure;

    // ToDo: remove this
    private ActivityInterval? _interval;

    public override object Build()
    {
        if (dimension is null)
            throw new InvalidOperationException("A dimension is required.");
        if (_measure is null)
            throw new InvalidOperationException("A measure is required.");

        var activityData = UseState(Array.Empty<Activity>());
        var resolvedInterval = UseState(ActivityInterval.Daily);
        var loading = UseState(true);
        var error = UseState<Exception?>();

        UseEffect(async () =>
        {
            try
            {
                // Resolve the interval from the *raw* dimension values before we truncate.
                var effectiveInterval = await ResolveIntervalAsync();

                // Truncate the dimension to the cell granularity (day or hour) so the pivot
                // groups all rows belonging to a cell together. Without this, a dimension that
                // carries minutes/seconds (e.g. a raw timestamp) lands every row in its own
                // single-element group, which would make Average/Min/Max/Count degenerate into Sum.
                var pivotDimension = BuildTruncatedDimension(dimension, effectiveInterval);

                var results = await data
                    .ToPivotTable()
                    .Dimension(pivotDimension)
                    .Measure(_measure)
                    .ExecuteAsync();

                // Each result row is now exactly one cell, so the aggregated measure value is used
                // directly — no re-grouping (which would re-sum) is required.
                var activities = results
                    .Select(row => new Activity
                    {
                        Date = ToDateOnly(row[pivotDimension.Name]),
                        Hour = effectiveInterval == ActivityInterval.Hourly ? ToHour(row[pivotDimension.Name]) : null,
                        Count = Convert.ToInt32(row[_measure.Name])
                    })
                    .ToArray();

                resolvedInterval.Set(effectiveInterval);
                activityData.Set(activities);
            }
            catch (Exception ex)
            {
                error.Set(ex);
            }
            finally
            {
                loading.Set(false);
            }
        }, [EffectTrigger.OnMount()]);

        if (loading.Value)
            return new ChatLoading();

        if (error.Value is { } loadError)
            return Callout.Error(loadError.Message);

        var widget = new ActivityHeatmap()
            .Data(activityData.Value)
            .ColorScheme(_colorScheme)
            .ShowTooltip(_showTooltip)
            .ShowMonthLabels(_showMonthLabels)
            .ShowDayLabels(_showDayLabels)
            .Interval(resolvedInterval.Value)
            .ValueLabel(_measure.Name)
            .StartDate(_startDate)
            .EndDate(_endDate);

        if (_onDayClick is not null)
            widget = widget with { OnDayClick = _onDayClick };

        return polish?.Invoke(widget) ?? widget;
    }

    public ActivityHeatmapBuilder<TSource> Measure(string name, Expression<Func<IQueryable<TSource>, object>> aggregator)
    {
        _measure = new Measure<TSource>(name, aggregator);
        return this;
    }

    public ActivityHeatmapBuilder<TSource> Interval(ActivityInterval interval) { _interval = interval; return this; }
    public ActivityHeatmapBuilder<TSource> ColorScheme(Colors scheme) { _colorScheme = scheme; return this; }
    public ActivityHeatmapBuilder<TSource> ShowTooltip(bool show = true) { _showTooltip = show; return this; }
    public ActivityHeatmapBuilder<TSource> ShowMonthLabels(bool show = true) { _showMonthLabels = show; return this; }
    public ActivityHeatmapBuilder<TSource> ShowDayLabels(bool show = true) { _showDayLabels = show; return this; }
    public ActivityHeatmapBuilder<TSource> StartDate(DateOnly? date) { _startDate = date; return this; }
    public ActivityHeatmapBuilder<TSource> EndDate(DateOnly? date) { _endDate = date; return this; }

    public ActivityHeatmapBuilder<TSource> OnDayClick(Func<Event<ActivityHeatmap, Activity>, ValueTask> handler)
    {
        _onDayClick = new EventHandler<Event<ActivityHeatmap, Activity>>(handler);
        return this;
    }

    public ActivityHeatmapBuilder<TSource> OnDayClick(Action<Activity> handler)
    {
        _onDayClick = new(e => { handler(e.Value); return ValueTask.CompletedTask; });
        return this;
    }

    private async Task<ActivityInterval> ResolveIntervalAsync()
    {
        if (_interval is { } explicitInterval)
            return explicitInterval;

        var dimensionValues = await data.Select(dimension.Selector).ToListAsync2();
        var hasIntraDay = dimensionValues.Any(HasTimeComponent);
        return hasIntraDay ? ActivityInterval.Hourly : ActivityInterval.Daily;
    }

    private static Dimension<TSource> BuildTruncatedDimension(Dimension<TSource> dimension, ActivityInterval interval)
    {
        var selector = dimension.Selector;
        var truncated = Expression.Call(
            TruncateMethod,
            selector.Body,
            Expression.Constant(interval));
        var lambda = Expression.Lambda<Func<TSource, object>>(truncated, selector.Parameters);
        return dimension with { Selector = lambda };
    }

    private static readonly System.Reflection.MethodInfo TruncateMethod =
        typeof(ActivityHeatmapBuilder<TSource>).GetMethod(
            nameof(TruncateDimensionValue),
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;

    private static bool HasTimeComponent(object value) => value switch
    {
        DateTime dt => dt.TimeOfDay != TimeSpan.Zero,
        DateTimeOffset dto => dto.TimeOfDay != TimeSpan.Zero,
        _ => false
    };

    /// <summary>
    /// Truncates a dimension value to the cell granularity so that all rows belonging to the same
    /// day (daily) or hour (hourly) collapse into a single pivot group. This lets the chosen
    /// aggregation (Sum, Average, Min, Max, Count) see every row for the cell.
    /// </summary>
    internal static object TruncateDimensionValue(object value, ActivityInterval interval) => value switch
    {
        DateTime dt => interval == ActivityInterval.Hourly
            ? new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0, dt.Kind)
            : dt.Date,
        DateTimeOffset dto => interval == ActivityInterval.Hourly
            ? new DateTimeOffset(dto.Year, dto.Month, dto.Day, dto.Hour, 0, 0, dto.Offset)
            : new DateTimeOffset(dto.Date, dto.Offset),
        string s when DateTime.TryParse(s, out var parsed) => interval == ActivityInterval.Hourly
            ? new DateTime(parsed.Year, parsed.Month, parsed.Day, parsed.Hour, 0, 0)
            : parsed.Date,
        _ => value
    };

    internal static DateOnly ToDateOnly(object value) => value switch
    {
        DateOnly d => d,
        DateTime dt => DateOnly.FromDateTime(dt),
        DateTimeOffset dto => DateOnly.FromDateTime(dto.DateTime),
        string s => DateOnly.Parse(s),
        _ => DateOnly.Parse(value.ToString()!)
    };

    internal static int? ToHour(object value) => value switch
    {
        DateTime dt => dt.Hour,
        DateTimeOffset dto => dto.Hour,
        string s when DateTime.TryParse(s, out var dt) => dt.Hour,
        _ => null
    };
}

public enum ActivityAggregation
{
    Sum,
    Count,
    Average,
    Min,
    Max
}

public static class ActivityHeatmapBuilderExtensions
{
    public static ActivityHeatmapBuilder<TSource> ToActivityHeatmap<TSource, TDimension, TMeasure>(
        this IEnumerable<TSource> data,
        Expression<Func<TSource, TDimension>> dimension,
        Expression<Func<TSource, TMeasure>> measure,
        ActivityAggregation aggregation = ActivityAggregation.Sum)
    {
        return data.AsQueryable().ToActivityHeatmap(dimension, measure, aggregation);
    }

    [System.Runtime.CompilerServices.OverloadResolutionPriority(1)]
    public static ActivityHeatmapBuilder<TSource> ToActivityHeatmap<TSource, TDimension, TMeasure>(
        this IQueryable<TSource> data,
        Expression<Func<TSource, TDimension>> dimension,
        Expression<Func<TSource, TMeasure>> measure,
        ActivityAggregation aggregation = ActivityAggregation.Sum)
    {
        var boxedDimension = BoxSelector(dimension);
        var aggregator = BuildAggregator(measure, aggregation);

        return new ActivityHeatmapBuilder<TSource>(
            data,
            new Dimension<TSource>(ExpressionNameHelper.SuggestName(boxedDimension) ?? "Dimension", boxedDimension),
            new Measure<TSource>(MeasureName(measure, aggregation), aggregator)
        );
    }

    private static Expression<Func<TSource, object>> BoxSelector<TSource, TValue>(
        Expression<Func<TSource, TValue>> selector)
    {
        var body = Expression.Convert(selector.Body, typeof(object));
        return Expression.Lambda<Func<TSource, object>>(body, selector.Parameters);
    }

    private static string MeasureName<TSource, TMeasure>(
        Expression<Func<TSource, TMeasure>> selector,
        ActivityAggregation aggregation)
    {
        return ExpressionNameHelper.SuggestName(BoxSelector(selector)) ?? aggregation.ToString();
    }

    private static Expression<Func<IQueryable<TSource>, object>> BuildAggregator<TSource, TMeasure>(
        Expression<Func<TSource, TMeasure>> selector,
        ActivityAggregation aggregation)
    {
        var source = Expression.Parameter(typeof(IQueryable<TSource>), "q");

        Expression call = aggregation switch
        {
            ActivityAggregation.Count => Expression.Call(
                CountMethod<TSource>(),
                source),
            ActivityAggregation.Min => Expression.Call(
                MinMaxMethod<TSource, TMeasure>(nameof(Queryable.Min)),
                source, selector),
            ActivityAggregation.Max => Expression.Call(
                MinMaxMethod<TSource, TMeasure>(nameof(Queryable.Max)),
                source, selector),
            ActivityAggregation.Sum => Expression.Call(
                NumericMethod<TSource, TMeasure>(nameof(Queryable.Sum)),
                source, selector),
            ActivityAggregation.Average => Expression.Call(
                NumericMethod<TSource, TMeasure>(nameof(Queryable.Average)),
                source, selector),
            _ => throw new ArgumentOutOfRangeException(nameof(aggregation), aggregation, null)
        };

        var body = Expression.Convert(call, typeof(object));
        return Expression.Lambda<Func<IQueryable<TSource>, object>>(body, source);
    }

    private static System.Reflection.MethodInfo CountMethod<TSource>()
    {
        return typeof(Queryable).GetMethods()
            .First(m => m.Name == nameof(Queryable.Count)
                        && m.IsGenericMethodDefinition
                        && m.GetGenericArguments().Length == 1
                        && m.GetParameters().Length == 1)
            .MakeGenericMethod(typeof(TSource));
    }

    private static System.Reflection.MethodInfo MinMaxMethod<TSource, TMeasure>(string name)
    {
        return typeof(Queryable).GetMethods()
            .First(m => m.Name == name
                        && m.IsGenericMethodDefinition
                        && m.GetGenericArguments().Length == 2
                        && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(TSource), typeof(TMeasure));
    }

    private static System.Reflection.MethodInfo NumericMethod<TSource, TMeasure>(string name)
    {
        var method = typeof(Queryable).GetMethods()
            .Where(m => m.Name == name
                        && m.IsGenericMethodDefinition
                        && m.GetGenericArguments().Length == 1
                        && m.GetParameters().Length == 2)
            .Select(m => m.MakeGenericMethod(typeof(TSource)))
            .FirstOrDefault(m =>
            {
                var selectorParam = m.GetParameters()[1].ParameterType; // Expression<Func<TSource, X>>
                var funcType = selectorParam.GetGenericArguments()[0];   // Func<TSource, X>
                var returnType = funcType.GetGenericArguments()[1];      // X
                return returnType == typeof(TMeasure);
            });

        if (method is null)
        {
            throw new NotSupportedException(
                $"Queryable.{name} does not support a measure of type '{typeof(TMeasure).Name}'. " +
                "Supported types are int, long, float, double, decimal and their nullable variants.");
        }

        return method;
    }
}
