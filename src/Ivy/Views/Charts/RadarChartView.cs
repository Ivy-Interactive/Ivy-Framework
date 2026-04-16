using System.Collections.Immutable;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Ivy.Core.Helpers;

// ReSharper disable once CheckNamespace
namespace Ivy;

public enum RadarChartStyles
{
    Default,
    Circle,
    Dashboard
}

public interface IRadarChartStyle<TSource>
{
    RadarChart Design(ExpandoObject[] data, Dimension<TSource> category, Measure<TSource>[] measures);
}

public static class RadarChartStyleHelpers
{
    public static IRadarChartStyle<TSource> GetStyle<TSource>(RadarChartStyles style)
    {
        return style switch
        {
            RadarChartStyles.Default => new DefaultRadarChartStyle<TSource>(),
            RadarChartStyles.Circle => new CircleRadarChartStyle<TSource>(),
            RadarChartStyles.Dashboard => new DashboardRadarChartStyle<TSource>(),
            _ => throw new InvalidOperationException($"Style {style} not found.")
        };
    }
}

public class DefaultRadarChartStyle<TSource> : IRadarChartStyle<TSource>
{
    public RadarChart Design(ExpandoObject[] data, Dimension<TSource> category, Measure<TSource>[] measures)
    {
        var chart = new RadarChart(data)
            .ColorScheme(ColorScheme.Default)
            .Shape(RadarShape.Polygon)
            .Tooltip()
            .Legend(new Legend()
                .Layout(Legend.Layouts.Horizontal)
                .Align(Legend.Alignments.Center)
                .VerticalAlign(Legend.VerticalAlignments.Bottom)
            )
            .SplitLine(true);

        foreach (var measure in measures)
        {
            chart = chart.Indicator(measure.Name);
        }

        chart = chart.Radar("values");

        return chart;
    }
}

public class CircleRadarChartStyle<TSource> : IRadarChartStyle<TSource>
{
    public RadarChart Design(ExpandoObject[] data, Dimension<TSource> category, Measure<TSource>[] measures)
    {
        var chart = new RadarChart(data)
            .ColorScheme(ColorScheme.Default)
            .Shape(RadarShape.Circle)
            .Tooltip()
            .Legend(new Legend()
                .Layout(Legend.Layouts.Horizontal)
                .Align(Legend.Alignments.Center)
                .VerticalAlign(Legend.VerticalAlignments.Bottom)
            )
            .SplitLine(true);

        foreach (var measure in measures)
        {
            chart = chart.Indicator(measure.Name);
        }

        chart = chart.Radar(new Radar("values").Filled());

        return chart;
    }
}

public class DashboardRadarChartStyle<TSource> : IRadarChartStyle<TSource>
{
    public RadarChart Design(ExpandoObject[] data, Dimension<TSource> category, Measure<TSource>[] measures)
    {
        var chart = new RadarChart(data)
            .ColorScheme(ColorScheme.Default)
            .Shape(RadarShape.Polygon)
            .Tooltip()
            .SplitArea(true)
            .SplitLine(true)
            .AxisLine(false);

        foreach (var measure in measures)
        {
            chart = chart.Indicator(measure.Name);
        }

        chart = chart.Radar(new Radar("values").Filled());

        return chart;
    }
}

public class RadarChartBuilder<TSource>(
    IQueryable<TSource> data,
    Dimension<TSource>? category = null,
    Measure<TSource>[]? measures = null,
    IRadarChartStyle<TSource>? style = null,
    Func<RadarChart, RadarChart>? polish = null)
    : ViewBase
{
    private readonly List<Measure<TSource>> _measures = [.. measures ?? []];
    private Toolbox? _toolbox;
    private Func<Toolbox, Toolbox>? _toolboxFactory;
    private Size? _height;
    private Size? _width;

    public override object? Build()
    {
        if (category is null)
        {
            throw new InvalidOperationException("A category dimension is required.");
        }

        if (_measures.Count == 0)
        {
            throw new InvalidOperationException("At least one measure is required.");
        }

        var chartData = UseState(ImmutableArray.Create<Dictionary<string, object>>);
        var loading = UseState(true);
        var exception = UseState<Exception?>((Exception?)null);

        UseEffect(async () =>
        {
            try
            {
                // Use "name" as the dimension key so the frontend can identify series
                var nameDimension = new Dimension<TSource>("name", category.Selector);
                var results = await data
                    .ToPivotTable()
                    .Dimension(nameDimension).Measures(_measures)
                    .ExecuteAsync();
                chartData.Set([.. results]);
            }
            catch (Exception e)
            {
                exception.Set(e);
            }
            finally
            {
                loading.Set(false);
            }
        }, [EffectTrigger.OnMount()]);

        if (exception.Value is not null)
        {
            return new ErrorTeaserView(exception.Value);
        }

        if (loading.Value)
        {
            return new ChatLoading();
        }

        var resolvedDesigner = style ?? RadarChartStyleHelpers.GetStyle<TSource>(RadarChartStyles.Default);

        var scaffolded = resolvedDesigner.Design(
            chartData.Value.ToExpando(),
            category,
            _measures.ToArray()
        );

        var configuredChart = scaffolded;

        if (_toolbox is not null)
        {
            configuredChart = configuredChart.Toolbox(_toolbox);
        }
        else if (_toolboxFactory is not null)
        {
            var baseToolbox = configuredChart.Toolbox ?? new Toolbox();
            configuredChart = configuredChart.Toolbox(_toolboxFactory(baseToolbox));
        }

        var result = polish?.Invoke(configuredChart) ?? configuredChart;

        if (_height is not null)
            result = result.Height(_height);
        if (_width is not null)
            result = result.Width(_width);

        return result;
    }

    public RadarChartBuilder<TSource> Height(Size size)
    {
        _height = size;
        return this;
    }

    public RadarChartBuilder<TSource> Width(Size size)
    {
        _width = size;
        return this;
    }

    public RadarChartBuilder<TSource> Dimension(string name, Expression<Func<TSource, object>> selector)
    {
        category = new Dimension<TSource>(name, selector);
        return this;
    }

    public RadarChartBuilder<TSource> Measure(string name, Expression<Func<IQueryable<TSource>, object>> aggregator)
    {
        _measures.Add(new Measure<TSource>(name, aggregator));
        return this;
    }

    public RadarChartBuilder<TSource> Toolbox(Toolbox toolbox)
    {
        ArgumentNullException.ThrowIfNull(toolbox);
        _toolbox = toolbox;
        _toolboxFactory = null;
        return this;
    }

    public RadarChartBuilder<TSource> Toolbox(Func<Toolbox, Toolbox> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        _toolbox = null;
        _toolboxFactory = configure;
        return this;
    }

    public RadarChartBuilder<TSource> Toolbox()
    {
        return Toolbox(_ => new Toolbox());
    }
}

public static partial class RadarChartExtensions
{
    public static RadarChartBuilder<TSource> ToRadarChart<TSource>(
        this IEnumerable<TSource> data,
        Expression<Func<TSource, object>> category,
        Expression<Func<IQueryable<TSource>, object>>[] measures,
        RadarChartStyles style = RadarChartStyles.Default,
        Func<RadarChart, RadarChart>? polish = null)
    {
        return data.AsQueryable().ToRadarChart(category, measures, style, polish);
    }

    [OverloadResolutionPriority(1)]
    public static RadarChartBuilder<TSource> ToRadarChart<TSource>(
        this IQueryable<TSource> data,
        Expression<Func<TSource, object>> category,
        Expression<Func<IQueryable<TSource>, object>>[] measures,
        RadarChartStyles style = RadarChartStyles.Default,
        Func<RadarChart, RadarChart>? polish = null)
    {
        return new RadarChartBuilder<TSource>(data,
            new Dimension<TSource>(ExpressionNameHelper.SuggestName(category) ?? "Category", category),
            measures.Select(m => new Measure<TSource>(ExpressionNameHelper.SuggestName(m) ?? "Measure", m)).ToArray(),
            RadarChartStyleHelpers.GetStyle<TSource>(style),
            polish
        );
    }
}
