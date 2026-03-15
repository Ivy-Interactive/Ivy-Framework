using System;
using System.Collections.Immutable;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;

// ReSharper disable once CheckNamespace
namespace Ivy;

public enum AreaChartStyles
{
    Default,
    Dashboard
}

public interface IAreaChartStyle<TSource>
{
    AreaChart Design(ExpandoObject[] data, Dimension<TSource> dimension, Measure<TSource>[] measures);
}

public static class AreaChartStyleHelpers
{
    public static IAreaChartStyle<TSource> GetStyle<TSource>(AreaChartStyles style)
    {
        return style switch
        {
            AreaChartStyles.Default => new DefaultAreaChartStyle<TSource>(),
            AreaChartStyles.Dashboard => new DashboardAreaChartStyle<TSource>(),
            _ => throw new InvalidOperationException($"Style {style} not found.")
        };
    }
}

public class DefaultAreaChartStyle<TSource> : IAreaChartStyle<TSource>
{
    public AreaChart Design(ExpandoObject[] data, Dimension<TSource> dimension, Measure<TSource>[] measures)
    {
        return new AreaChart(data)
            .Area(measures.Select(m => new Area(m.Name, 1)).ToArray())
            .YAxis(new YAxis())
            .XAxis(new XAxis(dimension.Name).TickLine(false).AxisLine(false).MinTickGap(10))
            .CartesianGrid(new CartesianGrid().Horizontal())
            .Tooltip(new ChartTooltip().Animated(true))
            .Legend();

    }
}

public class DashboardAreaChartStyle<TSource> : IAreaChartStyle<TSource>
{
    public AreaChart Design(ExpandoObject[] data, Dimension<TSource> dimension, Measure<TSource>[] measures)
    {
        return new AreaChart(data)
            .ColorScheme(ColorScheme.Default)
            .Area(measures.Select(m => new Area(m.Name, 1)).ToArray())
            .XAxis(new XAxis(dimension.Name).TickLine(false).AxisLine(false).MinTickGap(10))
            .CartesianGrid(new CartesianGrid().Horizontal())
            .Tooltip(new ChartTooltip().Animated(true))
        ;
    }
}

public class AreaChartBuilder<TSource>(
    IQueryable<TSource> data,
    Dimension<TSource>? dimension = null,
    Measure<TSource>[]? measures = null,
    IAreaChartStyle<TSource>? style = null,
    Func<AreaChart, AreaChart>? polish = null)
    : ViewBase
{
    private readonly List<Measure<TSource>> _measures = [.. measures ?? []];
    private Toolbox? _toolbox;
    private Func<Toolbox, Toolbox>? _toolboxFactory;
    private Expression<Func<TSource, object>>? _sortSelector;
    private SortOrder _sortOrder = SortOrder.None;
    private Size? _height;
    private Size? _width;

    public override object? Build()
    {
        if (dimension is null)
        {
            throw new InvalidOperationException("A dimension is required.");
        }

        if (_measures.Count == 0)
        {
            throw new InvalidOperationException("At least one measure is required.");
        }

        var lineChartData = UseState(ImmutableArray.Create<Dictionary<string, object>>);
        var loading = UseState(true);

        UseEffect(async () =>
        {
            try
            {
                var pivotBuilder = data
                    .ToPivotTable()
                    .Dimension(dimension).Measures(_measures);

                if (_sortOrder != SortOrder.None)
                {
                    pivotBuilder = _sortSelector != null
                        ? pivotBuilder.SortBy(_sortSelector, _sortOrder)
                        : pivotBuilder.SortBy(_sortOrder);
                }

                var results = await pivotBuilder.ExecuteAsync();
                lineChartData.Set([.. results]);
            }
            finally
            {
                loading.Set(false);
            }
        }, [EffectTrigger.OnMount()]);

        if (loading.Value)
        {
            return new ChatLoading();
        }

        var resolvedDesigner = style ?? AreaChartStyleHelpers.GetStyle<TSource>(AreaChartStyles.Default);

        var scaffolded = resolvedDesigner.Design(
            lineChartData.Value.ToExpando(),
            dimension,
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
            result = result with { Height = _height };
        if (_width is not null)
            result = result with { Width = _width };

        return result;
    }

    public AreaChartBuilder<TSource> Height(Size size)
    {
        _height = size;
        return this;
    }

    public AreaChartBuilder<TSource> Width(Size size)
    {
        _width = size;
        return this;
    }

    public AreaChartBuilder<TSource> Dimension(string name, Expression<Func<TSource, object>> selector)
    {
        dimension = new Dimension<TSource>(name, selector);
        return this;
    }

    public AreaChartBuilder<TSource> Measure(string name, Expression<Func<IQueryable<TSource>, object>> aggregator)
    {
        _measures.Add(new Measure<TSource>(name, aggregator));
        return this;
    }

    public AreaChartBuilder<TSource> Toolbox(Toolbox toolbox)
    {
        ArgumentNullException.ThrowIfNull(toolbox);
        _toolbox = toolbox;
        _toolboxFactory = null;
        return this;
    }

    public AreaChartBuilder<TSource> Toolbox(Func<Toolbox, Toolbox> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        _toolbox = null;
        _toolboxFactory = configure;
        return this;
    }

    public AreaChartBuilder<TSource> Toolbox()
    {
        return Toolbox(_ => new Toolbox());
    }

    public AreaChartBuilder<TSource> SortBy(Expression<Func<TSource, object>> selector, SortOrder order = SortOrder.Ascending)
    {
        _sortSelector = selector;
        _sortOrder = order;
        return this;
    }

    public AreaChartBuilder<TSource> SortBy(SortOrder order)
    {
        _sortOrder = order;
        _sortSelector = null;
        return this;
    }
}

public static partial class AreaChartExtensions
{
    public static AreaChartBuilder<TSource> ToAreaChart<TSource>(
    this IEnumerable<TSource> data,
    Expression<Func<TSource, object>>? dimension = null,
    Expression<Func<IQueryable<TSource>, object>>[]? measures = null,
    AreaChartStyles style = AreaChartStyles.Default,
    Func<AreaChart, AreaChart>? polish = null)
    {
        return data.AsQueryable().ToAreaChart(dimension, measures, style, polish);
    }

    [OverloadResolutionPriority(1)]
    public static AreaChartBuilder<TSource> ToAreaChart<TSource>(
    this IQueryable<TSource> data,
    Expression<Func<TSource, object>>? dimension = null,
    Expression<Func<IQueryable<TSource>, object>>[]? measures = null,
    AreaChartStyles style = AreaChartStyles.Default,
    Func<AreaChart, AreaChart>? polish = null)
    {
        return new AreaChartBuilder<TSource>(data,
            dimension != null ? new Dimension<TSource>(ExpressionNameHelper.SuggestName(dimension) ?? "Dimension", dimension) : null,
            measures?.Select(m => new Measure<TSource>(ExpressionNameHelper.SuggestName(m) ?? "Measure", m)).ToArray(),
            AreaChartStyleHelpers.GetStyle<TSource>(style),
            polish
        );
    }
}

