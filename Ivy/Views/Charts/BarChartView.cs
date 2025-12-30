using System;
using System.Collections.Immutable;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Ivy.Charts;
using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;
using Ivy.Shared;

namespace Ivy.Views.Charts;

public enum BarChartStyles
{
    Default,
    Dashboard
}

public interface IBarChartStyle<TSource>
{
    BarChart Design(ExpandoObject[] data, Dimension<TSource> dimension, Measure<TSource>[] measures);
}

public static class BarChartStyleHelpers
{
    public static IBarChartStyle<TSource> GetStyle<TSource>(BarChartStyles style)
    {
        return style switch
        {
            BarChartStyles.Default => new DefaultBarChartStyle<TSource>(),
            BarChartStyles.Dashboard => new DashboardBarChartStyle<TSource>(),
            _ => throw new InvalidOperationException($"Style {style} not found.")
        };
    }
}

public class DefaultBarChartStyle<TSource> : IBarChartStyle<TSource>
{
    public BarChart Design(ExpandoObject[] data, Dimension<TSource> dimension, Measure<TSource>[] measures)
    {
        return new BarChart(data)
            .Bar(measures.Select(m => new Bar(m.Name, 1)).ToArray())
            .YAxis(new YAxis())
            .XAxis(new XAxis(dimension.Name).TickLine(false).AxisLine(false).MinTickGap(10))
            .CartesianGrid(new CartesianGrid().Horizontal())
            .Tooltip(new Ivy.Charts.Tooltip().Animated(true))
            .Legend(new Legend()
                .Layout(Legend.Layouts.Horizontal)
                .Align(Legend.Alignments.Center)
                .VerticalAlign(Legend.VerticalAlignments.Bottom)
            )
        ;
    }
}

public class DashboardBarChartStyle<TSource> : IBarChartStyle<TSource>
{
    public BarChart Design(ExpandoObject[] data, Dimension<TSource> dimension, Measure<TSource>[] measures)
    {
        return new BarChart(data)
                .Vertical()
                .ColorScheme(ColorScheme.Default)
                .Bar(measures.Select(m =>
                    new Bar(m.Name, 1).Radius(8).FillOpacity(0.8)
                        .LabelList(new LabelList(dimension.Name).Position(Positions.InsideLeft).Offset(8).Fill(Colors.White))
                        .LabelList(new LabelList(measures[0].Name).Position(Positions.Right).Offset(8).Fill(Colors.Gray).NumberFormat("0"))
                ))
                .XAxis(new XAxis().Type(AxisTypes.Number).Hide())
                .YAxis(new YAxis(dimension.Name).Type(AxisTypes.Category).Hide())
                .CartesianGrid(new CartesianGrid().Vertical())
                .Tooltip(new Ivy.Charts.Tooltip().Animated(true))
        ;
    }
}

public class BarChartBuilder<TSource>(
    IQueryable<TSource> data,
    Dimension<TSource>? dimension = null,
    Measure<TSource>[]? measures = null,
    IBarChartStyle<TSource>? style = null,
    Func<BarChart, BarChart>? polish = null)
    : ViewBase
{
    private readonly List<Measure<TSource>> _measures = [.. measures ?? []];
    private Toolbox? _toolbox;
    private Func<Toolbox, Toolbox>? _toolboxFactory;

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
                var results = await data
                    .ToPivotTable()
                    .Dimension(dimension).Measures(_measures).ExecuteAsync();
                lineChartData.Set([.. results]);
            }
            finally
            {
                loading.Set(false);
            }
        }, [EffectTrigger.AfterInit()]);

        if (loading.Value)
        {
            return new ChatLoading();
        }

        var resolvedDesigner = style ?? BarChartStyleHelpers.GetStyle<TSource>(BarChartStyles.Default);

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

        return polish?.Invoke(configuredChart) ?? configuredChart;
    }

    public BarChartBuilder<TSource> Dimension(string name, Expression<Func<TSource, object>> selector)
    {
        dimension = new Dimension<TSource>(name, selector);
        return this;
    }

    public BarChartBuilder<TSource> Measure(string name, Expression<Func<IQueryable<TSource>, object>> aggregator)
    {
        _measures.Add(new Measure<TSource>(name, aggregator));
        return this;
    }

    public BarChartBuilder<TSource> Toolbox(Toolbox toolbox)
    {
        ArgumentNullException.ThrowIfNull(toolbox);
        _toolbox = toolbox;
        _toolboxFactory = null;
        return this;
    }

    public BarChartBuilder<TSource> Toolbox(Func<Toolbox, Toolbox> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        _toolbox = null;
        _toolboxFactory = configure;
        return this;
    }

    public BarChartBuilder<TSource> Toolbox()
    {
        return Toolbox(_ => new Toolbox());
    }
}

public static class BarChartExtensions
{
    public static BarChartBuilder<TSource> ToBarChart<TSource>(
    this IEnumerable<TSource> data,
    Expression<Func<TSource, object>>? dimension = null,
    Expression<Func<IQueryable<TSource>, object>>[]? measures = null,
    BarChartStyles style = BarChartStyles.Default,
    Func<BarChart, BarChart>? polish = null)
    {
        return data.AsQueryable().ToBarChart(dimension, measures, style, polish);
    }

    [OverloadResolutionPriority(1)]
    public static BarChartBuilder<TSource> ToBarChart<TSource>(
    this IQueryable<TSource> data,
    Expression<Func<TSource, object>>? dimension = null,
    Expression<Func<IQueryable<TSource>, object>>[]? measures = null,
    BarChartStyles style = BarChartStyles.Default,
    Func<BarChart, BarChart>? polish = null)
    {
        return new BarChartBuilder<TSource>(data,
            dimension != null ? new Dimension<TSource>(ExpressionNameHelper.SuggestName(dimension) ?? "Dimension", dimension) : null,
            measures?.Select(m => new Measure<TSource>(ExpressionNameHelper.SuggestName(m) ?? "Measure", m)).ToArray(),
            BarChartStyleHelpers.GetStyle<TSource>(style),
            polish
        );
    }
}

