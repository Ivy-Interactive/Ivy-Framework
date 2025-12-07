using System;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Ivy.Charts;
using Ivy.Core;
using Ivy.Core.Hooks;

namespace Ivy.Views.Charts;

public record PieChartData(string? Dimension, double Measure);

public enum PieChartStyles
{
    Default,
    Dashboard,
    Donut
}

public interface IPieChartStyle<TSource>
{
    PieChart Design(PieChartData[] data, PieChartTotal? total);
}

public static class PieChartStyleHelpers
{
    public static IPieChartStyle<TSource> GetStyle<TSource>(PieChartStyles style)
    {
        return style switch
        {
            PieChartStyles.Default => new DefaultPieChartStyle<TSource>(),
            PieChartStyles.Dashboard => new DashboardPieChartStyle<TSource>(),
            PieChartStyles.Donut => new DonutPieChartStyle<TSource>(),
            _ => throw new InvalidOperationException($"Style {style} not found.")
        };
    }
}

public class DefaultPieChartStyle<TSource> : IPieChartStyle<TSource>
{
    public PieChart Design(PieChartData[] data, PieChartTotal? total)
    {
        return new PieChart(data)
            .Pie(nameof(PieChartData.Measure), nameof(PieChartData.Dimension))
            .Tooltip(new Ivy.Charts.Tooltip().Animated(true))
            .Legend(new Legend()
                .Layout(Legend.Layouts.Horizontal)
                .Align(Legend.Alignments.Center)
                .VerticalAlign(Legend.VerticalAlignments.Bottom)
            );
    }
}

public class DashboardPieChartStyle<TSource> : IPieChartStyle<TSource>
{
    public PieChart Design(PieChartData[] data, PieChartTotal? total)
    {
        return new PieChart(data)
                .Pie(new Pie(nameof(PieChartData.Measure), nameof(PieChartData.Dimension))
                    .InnerRadius(total != null ? "50%" : (string?)null!)
                )
                .Total(total)
                .ColorScheme(ColorScheme.Default)
                .Legend(new Legend()
                    .IconType(Legend.IconTypes.Rect)
                    .Layout(Legend.Layouts.Horizontal)
                    .Align(Legend.Alignments.Center)
                    .VerticalAlign(Legend.VerticalAlignments.Bottom)
                )
                .Tooltip(new Ivy.Charts.Tooltip().Animated(true));
    }
}

public class DonutPieChartStyle<TSource> : IPieChartStyle<TSource>
{
    public PieChart Design(PieChartData[] data, PieChartTotal? total)
    {
        return new PieChart(data)
                .Pie(new Pie(nameof(PieChartData.Measure), nameof(PieChartData.Dimension))
                    .InnerRadius("50%")
                    .OuterRadius("80%")
                    .Animated(true)
                )
                .ColorScheme(ColorScheme.Rainbow)
                .Tooltip(new Ivy.Charts.Tooltip().Animated(true))
                .Legend(new Legend()
                    .Layout(Legend.Layouts.Horizontal)
                    .Align(Legend.Alignments.Center)
                    .VerticalAlign(Legend.VerticalAlignments.Bottom)
                );
    }
}

public class PieChartBuilder<TSource>(
    IQueryable<TSource> data,
    Dimension<TSource> dimension,
    Measure<TSource> measure,
    IPieChartStyle<TSource>? style = null,
    PieChartTotal? total = null,
    Func<PieChart, PieChart>? polish = null)
    : ViewBase
{
    private Toolbox? _toolbox;
    private Func<Toolbox, Toolbox>? _toolboxFactory;

    public override object? Build()
    {
        var pieChartData = UseState(ImmutableArray.Create<PieChartData>);
        var loading = UseState(true);
        var exception = UseState<Exception?>((Exception?)null);

        UseEffect(async () =>
        {
            try
            {
                var results = await data
                    .ToPivotTable()
                    .Dimension(dimension).Measure(measure).Produces<PieChartData>().ExecuteAsync()
                    .ToArrayAsync();
                pieChartData.Set([.. results]);
            }
            catch (Exception e)
            {
                exception.Set(e);
            }
            finally
            {
                loading.Set(false);
            }
        }, [EffectTrigger.AfterInit()]);

        if (exception.Value is not null)
        {
            return new ErrorTeaserView(exception.Value);
        }

        if (loading.Value)
        {
            return new ChatLoading();
        }

        var resolvedDesigner = style ?? PieChartStyleHelpers.GetStyle<TSource>(PieChartStyles.Default);

        var scaffolded = resolvedDesigner.Design(
           pieChartData.Value.ToArray(),
           total
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

    public PieChartBuilder<TSource> Toolbox(Toolbox toolbox)
    {
        ArgumentNullException.ThrowIfNull(toolbox);
        _toolbox = toolbox;
        _toolboxFactory = null;
        return this;
    }

    public PieChartBuilder<TSource> Toolbox(Func<Toolbox, Toolbox> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        _toolbox = null;
        _toolboxFactory = configure;
        return this;
    }

    public PieChartBuilder<TSource> Toolbox()
    {
        return Toolbox(_ => new Toolbox());
    }
}


public static class PieChartExtensions
{
    public static PieChartBuilder<TSource> ToPieChart<TSource>(
    this IEnumerable<TSource> data,
    Expression<Func<TSource, object>> dimension,
    Expression<Func<IQueryable<TSource>, object>> measure,
    PieChartStyles style = PieChartStyles.Default,
    PieChartTotal? total = null,
    Func<PieChart, PieChart>? polish = null)
    {
        return data.AsQueryable().ToPieChart(dimension, measure, style, total, polish);
    }

    [OverloadResolutionPriority(1)]
    public static PieChartBuilder<TSource> ToPieChart<TSource>(
    this IQueryable<TSource> data,
    Expression<Func<TSource, object>> dimension,
    Expression<Func<IQueryable<TSource>, object>> measure,
    PieChartStyles style = PieChartStyles.Default,
    PieChartTotal? total = null,
    Func<PieChart, PieChart>? polish = null)
    {
        return new PieChartBuilder<TSource>(data,
            new Dimension<TSource>(nameof(PieChartData.Dimension), dimension),
            new Measure<TSource>(nameof(PieChartData.Measure), measure),
            PieChartStyleHelpers.GetStyle<TSource>(style),
            total,
            polish
        );
    }
}

