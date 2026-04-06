using System.Collections.Immutable;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record ChordChartInput(string Source, string Target, double Value);

public enum ChordChartStyles
{
    Default,
    Sorted,
    Dashboard
}

public interface IChordChartStyle<TSource>
{
    ChordChart Design(ChordData data);
}

public static class ChordChartStyleHelpers
{
    public static IChordChartStyle<TSource> GetStyle<TSource>(ChordChartStyles style)
    {
        return style switch
        {
            ChordChartStyles.Default => new DefaultChordChartStyle<TSource>(),
            ChordChartStyles.Sorted => new SortedChordChartStyle<TSource>(),
            ChordChartStyles.Dashboard => new DashboardChordChartStyle<TSource>(),
            _ => throw new InvalidOperationException($"Style {style} not found.")
        };
    }
}

public class DefaultChordChartStyle<TSource> : IChordChartStyle<TSource>
{
    public ChordChart Design(ChordData data)
    {
        return new ChordChart(data)
            .Tooltip(new ChartTooltip().Animated(true))
            .Legend(new Legend()
                .Layout(Legend.Layouts.Horizontal)
                .Align(Legend.Alignments.Center)
                .VerticalAlign(Legend.VerticalAlignments.Bottom)
            );
    }
}

public class SortedChordChartStyle<TSource> : IChordChartStyle<TSource>
{
    public ChordChart Design(ChordData data)
    {
        return new ChordChart(data)
            .Tooltip(new ChartTooltip().Animated(true))
            .Sort()
            .SortSubGroups()
            .Legend(new Legend()
                .Layout(Legend.Layouts.Horizontal)
                .Align(Legend.Alignments.Center)
                .VerticalAlign(Legend.VerticalAlignments.Bottom)
            );
    }
}

public class DashboardChordChartStyle<TSource> : IChordChartStyle<TSource>
{
    public ChordChart Design(ChordData data)
    {
        return new ChordChart(data)
            .Tooltip(new ChartTooltip().Animated(true))
            .PadAngle(5)
            .Toolbox();
    }
}

public class ChordChartBuilder<TSource>(
    IQueryable<TSource> data,
    Func<TSource, string> sourceSelector,
    Func<TSource, string> targetSelector,
    Func<TSource, double> valueSelector,
    IChordChartStyle<TSource>? style = null,
    Func<ChordChart, ChordChart>? polish = null)
    : ViewBase
{
    private Toolbox? _toolbox;
    private Func<Toolbox, Toolbox>? _toolboxFactory;
    private Size? _height;
    private Size? _width;

    public override object? Build()
    {
        var chartData = UseState(ImmutableArray.Create<ChordChartInput>);
        var loading = UseState(true);
        var exception = UseState<Exception?>((Exception?)null);

        UseEffect(async () =>
        {
            try
            {
                var inputs = await Task.Run(() =>
                    data.Select(item => new ChordChartInput(
                        sourceSelector(item),
                        targetSelector(item),
                        valueSelector(item)
                    )).ToArray()
                );
                chartData.Set([.. inputs]);
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

        var inputs = chartData.Value;

        var nodeNames = inputs
            .SelectMany(i => new[] { i.Source, i.Target })
            .Distinct()
            .ToArray();

        var nodes = nodeNames.Select(n => new ChordNode(n)).ToArray();

        var nodeIndex = new Dictionary<string, int>();
        for (var i = 0; i < nodeNames.Length; i++)
            nodeIndex[nodeNames[i]] = i;

        var links = inputs
            .Select(i => new ChordLink(nodeIndex[i.Source], nodeIndex[i.Target], i.Value))
            .ToArray();

        var chordData = new ChordData(nodes, links);

        var resolvedDesigner = style ?? ChordChartStyleHelpers.GetStyle<TSource>(ChordChartStyles.Default);

        var scaffolded = resolvedDesigner.Design(chordData);

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

    public ChordChartBuilder<TSource> Height(Size size)
    {
        _height = size;
        return this;
    }

    public ChordChartBuilder<TSource> Width(Size size)
    {
        _width = size;
        return this;
    }

    public ChordChartBuilder<TSource> Toolbox(Toolbox toolbox)
    {
        ArgumentNullException.ThrowIfNull(toolbox);
        _toolbox = toolbox;
        _toolboxFactory = null;
        return this;
    }

    public ChordChartBuilder<TSource> Toolbox(Func<Toolbox, Toolbox> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        _toolbox = null;
        _toolboxFactory = configure;
        return this;
    }

    public ChordChartBuilder<TSource> Toolbox()
    {
        return Toolbox(_ => new Toolbox());
    }
}

public static class ChordChartViewExtensions
{
    public static ChordChartBuilder<TSource> ToChordChart<TSource>(
        this IEnumerable<TSource> data,
        Func<TSource, string> sourceSelector,
        Func<TSource, string> targetSelector,
        Func<TSource, double> valueSelector)
    {
        return data.AsQueryable().ToChordChart(sourceSelector, targetSelector, valueSelector);
    }

    public static ChordChartBuilder<TSource> ToChordChart<TSource>(
        this IEnumerable<TSource> data,
        Func<TSource, string> sourceSelector,
        Func<TSource, string> targetSelector,
        Func<TSource, double> valueSelector,
        ChordChartStyles style)
    {
        return data.AsQueryable().ToChordChart(sourceSelector, targetSelector, valueSelector, style);
    }

    public static ChordChartBuilder<TSource> ToChordChart<TSource>(
        this IEnumerable<TSource> data,
        Func<TSource, string> sourceSelector,
        Func<TSource, string> targetSelector,
        Func<TSource, double> valueSelector,
        IChordChartStyle<TSource> style)
    {
        return data.AsQueryable().ToChordChart(sourceSelector, targetSelector, valueSelector, style);
    }

    public static ChordChartBuilder<TSource> ToChordChart<TSource>(
        this IEnumerable<TSource> data,
        Func<TSource, string> sourceSelector,
        Func<TSource, string> targetSelector,
        Func<TSource, double> valueSelector,
        Func<ChordChart, ChordChart> polish)
    {
        return data.AsQueryable().ToChordChart(sourceSelector, targetSelector, valueSelector, polish);
    }

    public static ChordChartBuilder<TSource> ToChordChart<TSource>(
        this IEnumerable<TSource> data,
        Func<TSource, string> sourceSelector,
        Func<TSource, string> targetSelector,
        Func<TSource, double> valueSelector,
        ChordChartStyles style,
        Func<ChordChart, ChordChart> polish)
    {
        return data.AsQueryable().ToChordChart(sourceSelector, targetSelector, valueSelector, style, polish);
    }

    [OverloadResolutionPriority(1)]
    public static ChordChartBuilder<TSource> ToChordChart<TSource>(
        this IQueryable<TSource> data,
        Func<TSource, string> sourceSelector,
        Func<TSource, string> targetSelector,
        Func<TSource, double> valueSelector)
    {
        return new ChordChartBuilder<TSource>(data, sourceSelector, targetSelector, valueSelector);
    }

    [OverloadResolutionPriority(1)]
    public static ChordChartBuilder<TSource> ToChordChart<TSource>(
        this IQueryable<TSource> data,
        Func<TSource, string> sourceSelector,
        Func<TSource, string> targetSelector,
        Func<TSource, double> valueSelector,
        ChordChartStyles style)
    {
        return new ChordChartBuilder<TSource>(data, sourceSelector, targetSelector, valueSelector,
            ChordChartStyleHelpers.GetStyle<TSource>(style));
    }

    [OverloadResolutionPriority(1)]
    public static ChordChartBuilder<TSource> ToChordChart<TSource>(
        this IQueryable<TSource> data,
        Func<TSource, string> sourceSelector,
        Func<TSource, string> targetSelector,
        Func<TSource, double> valueSelector,
        IChordChartStyle<TSource> style)
    {
        return new ChordChartBuilder<TSource>(data, sourceSelector, targetSelector, valueSelector, style);
    }

    [OverloadResolutionPriority(1)]
    public static ChordChartBuilder<TSource> ToChordChart<TSource>(
        this IQueryable<TSource> data,
        Func<TSource, string> sourceSelector,
        Func<TSource, string> targetSelector,
        Func<TSource, double> valueSelector,
        Func<ChordChart, ChordChart> polish)
    {
        return new ChordChartBuilder<TSource>(data, sourceSelector, targetSelector, valueSelector, polish: polish);
    }

    [OverloadResolutionPriority(1)]
    public static ChordChartBuilder<TSource> ToChordChart<TSource>(
        this IQueryable<TSource> data,
        Func<TSource, string> sourceSelector,
        Func<TSource, string> targetSelector,
        Func<TSource, double> valueSelector,
        ChordChartStyles style,
        Func<ChordChart, ChordChart> polish)
    {
        return new ChordChartBuilder<TSource>(data, sourceSelector, targetSelector, valueSelector,
            ChordChartStyleHelpers.GetStyle<TSource>(style), polish);
    }
}
