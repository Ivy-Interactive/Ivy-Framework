using System.Collections.Immutable;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record SankeyChartInput(string Source, string Target, double Value);

public enum SankeyChartStyles
{
    Default,
    LeftAligned,
    Dashboard
}

public interface ISankeyChartStyle<TSource>
{
    SankeyChart Design(SankeyData data);
}

public static class SankeyChartStyleHelpers
{
    public static ISankeyChartStyle<TSource> GetStyle<TSource>(SankeyChartStyles style)
    {
        return style switch
        {
            SankeyChartStyles.Default => new DefaultSankeyChartStyle<TSource>(),
            SankeyChartStyles.LeftAligned => new LeftAlignedSankeyChartStyle<TSource>(),
            SankeyChartStyles.Dashboard => new DashboardSankeyChartStyle<TSource>(),
            _ => throw new InvalidOperationException($"Style {style} not found.")
        };
    }
}

public class DefaultSankeyChartStyle<TSource> : ISankeyChartStyle<TSource>
{
    public SankeyChart Design(SankeyData data)
    {
        return new SankeyChart(data)
            .Tooltip(new ChartTooltip().Animated(true))
            .Toolbox()
            .NodeAlign(SankeyAlign.Justify);
    }
}

public class LeftAlignedSankeyChartStyle<TSource> : ISankeyChartStyle<TSource>
{
    public SankeyChart Design(SankeyData data)
    {
        return new SankeyChart(data)
            .Tooltip(new ChartTooltip().Animated(true))
            .Toolbox()
            .NodeAlign(SankeyAlign.Left)
            .NodeGap(6);
    }
}

public class DashboardSankeyChartStyle<TSource> : ISankeyChartStyle<TSource>
{
    public SankeyChart Design(SankeyData data)
    {
        return new SankeyChart(data)
            .Tooltip(new ChartTooltip().Animated(true))
            .NodeWidth(15)
            .NodeGap(4);
    }
}

public class SankeyChartBuilder<TSource>(
    IQueryable<TSource> data,
    Func<TSource, string> sourceSelector,
    Func<TSource, string> targetSelector,
    Func<TSource, double> valueSelector,
    ISankeyChartStyle<TSource>? style = null,
    Func<SankeyChart, SankeyChart>? polish = null)
    : ViewBase
{
    private Toolbox? _toolbox;
    private Func<Toolbox, Toolbox>? _toolboxFactory;
    private Size? _height;
    private Size? _width;

    public override object? Build()
    {
        var chartData = UseState(ImmutableArray.Create<SankeyChartInput>);
        var loading = UseState(true);
        var exception = UseState<Exception?>((Exception?)null);

        UseEffect(async () =>
        {
            try
            {
                var inputs = await Task.Run(() =>
                    data.Select(item => new SankeyChartInput(
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

        var nodes = nodeNames.Select(n => new SankeyNode(n)).ToArray();

        var nodeIndex = new Dictionary<string, int>();
        for (var i = 0; i < nodeNames.Length; i++)
            nodeIndex[nodeNames[i]] = i;

        var links = inputs
            .Select(i => new SankeyLink(nodeIndex[i.Source], nodeIndex[i.Target], i.Value))
            .ToArray();

        var sankeyData = new SankeyData(nodes, links);

        var resolvedDesigner = style ?? SankeyChartStyleHelpers.GetStyle<TSource>(SankeyChartStyles.Default);

        var scaffolded = resolvedDesigner.Design(sankeyData);

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

    public SankeyChartBuilder<TSource> Height(Size size)
    {
        _height = size;
        return this;
    }

    public SankeyChartBuilder<TSource> Width(Size size)
    {
        _width = size;
        return this;
    }

    public SankeyChartBuilder<TSource> Toolbox(Toolbox toolbox)
    {
        ArgumentNullException.ThrowIfNull(toolbox);
        _toolbox = toolbox;
        _toolboxFactory = null;
        return this;
    }

    public SankeyChartBuilder<TSource> Toolbox(Func<Toolbox, Toolbox> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        _toolbox = null;
        _toolboxFactory = configure;
        return this;
    }

    public SankeyChartBuilder<TSource> Toolbox()
    {
        return Toolbox(_ => new Toolbox());
    }
}

public static class SankeyChartViewExtensions
{
    public static SankeyChartBuilder<TSource> ToSankeyChart<TSource>(
        this IEnumerable<TSource> data,
        Func<TSource, string> sourceSelector,
        Func<TSource, string> targetSelector,
        Func<TSource, double> valueSelector)
    {
        return data.AsQueryable().ToSankeyChart(sourceSelector, targetSelector, valueSelector);
    }

    public static SankeyChartBuilder<TSource> ToSankeyChart<TSource>(
        this IEnumerable<TSource> data,
        Func<TSource, string> sourceSelector,
        Func<TSource, string> targetSelector,
        Func<TSource, double> valueSelector,
        SankeyChartStyles style)
    {
        return data.AsQueryable().ToSankeyChart(sourceSelector, targetSelector, valueSelector, style);
    }

    public static SankeyChartBuilder<TSource> ToSankeyChart<TSource>(
        this IEnumerable<TSource> data,
        Func<TSource, string> sourceSelector,
        Func<TSource, string> targetSelector,
        Func<TSource, double> valueSelector,
        ISankeyChartStyle<TSource> style)
    {
        return data.AsQueryable().ToSankeyChart(sourceSelector, targetSelector, valueSelector, style);
    }

    public static SankeyChartBuilder<TSource> ToSankeyChart<TSource>(
        this IEnumerable<TSource> data,
        Func<TSource, string> sourceSelector,
        Func<TSource, string> targetSelector,
        Func<TSource, double> valueSelector,
        Func<SankeyChart, SankeyChart> polish)
    {
        return data.AsQueryable().ToSankeyChart(sourceSelector, targetSelector, valueSelector, polish);
    }

    public static SankeyChartBuilder<TSource> ToSankeyChart<TSource>(
        this IEnumerable<TSource> data,
        Func<TSource, string> sourceSelector,
        Func<TSource, string> targetSelector,
        Func<TSource, double> valueSelector,
        SankeyChartStyles style,
        Func<SankeyChart, SankeyChart> polish)
    {
        return data.AsQueryable().ToSankeyChart(sourceSelector, targetSelector, valueSelector, style, polish);
    }

    [OverloadResolutionPriority(1)]
    public static SankeyChartBuilder<TSource> ToSankeyChart<TSource>(
        this IQueryable<TSource> data,
        Func<TSource, string> sourceSelector,
        Func<TSource, string> targetSelector,
        Func<TSource, double> valueSelector)
    {
        return new SankeyChartBuilder<TSource>(data, sourceSelector, targetSelector, valueSelector);
    }

    [OverloadResolutionPriority(1)]
    public static SankeyChartBuilder<TSource> ToSankeyChart<TSource>(
        this IQueryable<TSource> data,
        Func<TSource, string> sourceSelector,
        Func<TSource, string> targetSelector,
        Func<TSource, double> valueSelector,
        SankeyChartStyles style)
    {
        return new SankeyChartBuilder<TSource>(data, sourceSelector, targetSelector, valueSelector,
            SankeyChartStyleHelpers.GetStyle<TSource>(style));
    }

    [OverloadResolutionPriority(1)]
    public static SankeyChartBuilder<TSource> ToSankeyChart<TSource>(
        this IQueryable<TSource> data,
        Func<TSource, string> sourceSelector,
        Func<TSource, string> targetSelector,
        Func<TSource, double> valueSelector,
        ISankeyChartStyle<TSource> style)
    {
        return new SankeyChartBuilder<TSource>(data, sourceSelector, targetSelector, valueSelector, style);
    }

    [OverloadResolutionPriority(1)]
    public static SankeyChartBuilder<TSource> ToSankeyChart<TSource>(
        this IQueryable<TSource> data,
        Func<TSource, string> sourceSelector,
        Func<TSource, string> targetSelector,
        Func<TSource, double> valueSelector,
        Func<SankeyChart, SankeyChart> polish)
    {
        return new SankeyChartBuilder<TSource>(data, sourceSelector, targetSelector, valueSelector, polish: polish);
    }

    [OverloadResolutionPriority(1)]
    public static SankeyChartBuilder<TSource> ToSankeyChart<TSource>(
        this IQueryable<TSource> data,
        Func<TSource, string> sourceSelector,
        Func<TSource, string> targetSelector,
        Func<TSource, double> valueSelector,
        SankeyChartStyles style,
        Func<SankeyChart, SankeyChart> polish)
    {
        return new SankeyChartBuilder<TSource>(data, sourceSelector, targetSelector, valueSelector,
            SankeyChartStyleHelpers.GetStyle<TSource>(style), polish);
    }
}
