using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record FunnelChartData(string? Stage, double Value);

public enum FunnelChartStyles
{
    Default,
    Horizontal,
    Dashboard
}

public interface IFunnelChartStyle<TSource>
{
    FunnelChart Design(FunnelChartData[] data);
}

public static class FunnelChartStyleHelpers
{
    public static IFunnelChartStyle<TSource> GetStyle<TSource>(FunnelChartStyles style)
    {
        return style switch
        {
            FunnelChartStyles.Default => new DefaultFunnelChartStyle<TSource>(),
            FunnelChartStyles.Horizontal => new HorizontalFunnelChartStyle<TSource>(),
            FunnelChartStyles.Dashboard => new DashboardFunnelChartStyle<TSource>(),
            _ => throw new InvalidOperationException($"Style {style} not found.")
        };
    }
}

public class DefaultFunnelChartStyle<TSource> : IFunnelChartStyle<TSource>
{
    public FunnelChart Design(FunnelChartData[] data)
    {
        return new FunnelChart(data)
            .Funnel(nameof(FunnelChartData.Value), nameof(FunnelChartData.Stage))
            .Tooltip(new ChartTooltip().Animated(true))
            .Sort(FunnelSort.Descending)
            .Legend(new Legend()
                .Layout(Legend.Layouts.Horizontal)
                .Align(Legend.Alignments.Center)
                .VerticalAlign(Legend.VerticalAlignments.Bottom)
            );
    }
}

public class HorizontalFunnelChartStyle<TSource> : IFunnelChartStyle<TSource>
{
    public FunnelChart Design(FunnelChartData[] data)
    {
        return new FunnelChart(data)
            .Funnel(nameof(FunnelChartData.Value), nameof(FunnelChartData.Stage))
            .Tooltip(new ChartTooltip().Animated(true))
            .Sort(FunnelSort.Descending)
            .Orientation(FunnelOrientation.Horizontal)
            .Legend(new Legend()
                .Layout(Legend.Layouts.Horizontal)
                .Align(Legend.Alignments.Center)
                .VerticalAlign(Legend.VerticalAlignments.Bottom)
            );
    }
}

public class DashboardFunnelChartStyle<TSource> : IFunnelChartStyle<TSource>
{
    public FunnelChart Design(FunnelChartData[] data)
    {
        return new FunnelChart(data)
            .Funnel(nameof(FunnelChartData.Value), nameof(FunnelChartData.Stage))
            .Tooltip(new ChartTooltip().Animated(true))
            .Sort(FunnelSort.Descending)
            .Gap(4);
    }
}

public class FunnelChartBuilder<TSource>(
    IQueryable<TSource> data,
    Dimension<TSource> stage,
    Measure<TSource> value,
    IFunnelChartStyle<TSource>? style = null,
    Func<FunnelChart, FunnelChart>? polish = null)
    : ViewBase
{
    private Toolbox? _toolbox;
    private Func<Toolbox, Toolbox>? _toolboxFactory;
    private Size? _height;
    private Size? _width;

    public override object? Build()
    {
        var chartData = UseState(ImmutableArray.Create<FunnelChartData>);
        var loading = UseState(true);
        var exception = UseState<Exception?>((Exception?)null);

        UseEffect(async () =>
        {
            try
            {
                var results = await data
                    .ToPivotTable()
                    .Dimension(stage).Measure(value).Produces<FunnelChartData>().ExecuteAsync()
                    .ToArrayAsync();
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

        var resolvedDesigner = style ?? FunnelChartStyleHelpers.GetStyle<TSource>(FunnelChartStyles.Default);

        var scaffolded = resolvedDesigner.Design(
            chartData.Value.ToArray()
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

    public FunnelChartBuilder<TSource> Height(Size size)
    {
        _height = size;
        return this;
    }

    public FunnelChartBuilder<TSource> Width(Size size)
    {
        _width = size;
        return this;
    }

    public FunnelChartBuilder<TSource> Toolbox(Toolbox toolbox)
    {
        ArgumentNullException.ThrowIfNull(toolbox);
        _toolbox = toolbox;
        _toolboxFactory = null;
        return this;
    }

    public FunnelChartBuilder<TSource> Toolbox(Func<Toolbox, Toolbox> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        _toolbox = null;
        _toolboxFactory = configure;
        return this;
    }

    public FunnelChartBuilder<TSource> Toolbox()
    {
        return Toolbox(_ => new Toolbox());
    }
}

public static partial class FunnelChartExtensions
{
    public static FunnelChartBuilder<TSource> ToFunnelChart<TSource>(
        this IEnumerable<TSource> data,
        Expression<Func<TSource, object>> stage,
        Expression<Func<IQueryable<TSource>, object>> value,
        FunnelChartStyles style = FunnelChartStyles.Default,
        Func<FunnelChart, FunnelChart>? polish = null)
    {
        return data.AsQueryable().ToFunnelChart(stage, value, style, polish);
    }

    [OverloadResolutionPriority(1)]
    public static FunnelChartBuilder<TSource> ToFunnelChart<TSource>(
        this IQueryable<TSource> data,
        Expression<Func<TSource, object>> stage,
        Expression<Func<IQueryable<TSource>, object>> value,
        FunnelChartStyles style = FunnelChartStyles.Default,
        Func<FunnelChart, FunnelChart>? polish = null)
    {
        return new FunnelChartBuilder<TSource>(data,
            new Dimension<TSource>(nameof(FunnelChartData.Stage), stage),
            new Measure<TSource>(nameof(FunnelChartData.Value), value),
            FunnelChartStyleHelpers.GetStyle<TSource>(style),
            polish
        );
    }
}
