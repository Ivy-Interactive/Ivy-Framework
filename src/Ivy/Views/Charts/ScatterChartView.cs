using System.Collections.Immutable;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace Ivy;

public enum ScatterChartStyles
{
    Default,
    Dashboard,
    Custom
}

public interface IScatterChartStyle<TSource>
{
    ScatterChart Design(ExpandoObject[] data, Dimension<TSource> dimensionX, Dimension<TSource> dimensionY, Dimension<TSource>? size, Dimension<TSource>? color);
}

public static class ScatterChartStyleHelpers
{
    public static IScatterChartStyle<TSource> GetStyle<TSource>(ScatterChartStyles style)
    {
        return style switch
        {
            ScatterChartStyles.Default => new DefaultScatterChartStyle<TSource>(),
            ScatterChartStyles.Dashboard => new DashboardScatterChartStyle<TSource>(),
            ScatterChartStyles.Custom => new CustomScatterChartStyle<TSource>(),
            _ => throw new InvalidOperationException($"Style {style} not found.")
        };
    }
}

public class DefaultScatterChartStyle<TSource> : IScatterChartStyle<TSource>
{
    public ScatterChart Design(ExpandoObject[] data, Dimension<TSource> dimensionX, Dimension<TSource> dimensionY, Dimension<TSource>? size, Dimension<TSource>? color)
    {
        var chart = new ScatterChart(data)
            .Scatter(new Scatter("Value"))
            .XAxis(new XAxis(dimensionX.Name).Type(AxisTypes.Number))
            .YAxis(new YAxis(dimensionY.Name).Type(AxisTypes.Number))
            .Tooltip(new ChartTooltip().Animated(true))
            .Legend(new Legend()
                .Layout(Legend.Layouts.Horizontal)
                .Align(Legend.Alignments.Center)
                .VerticalAlign(Legend.VerticalAlignments.Bottom));

        if (size is not null)
        {
            chart = chart.ZAxis(new ZAxis(size.Name));
        }

        return chart;
    }
}

public class DashboardScatterChartStyle<TSource> : IScatterChartStyle<TSource>
{
    public ScatterChart Design(ExpandoObject[] data, Dimension<TSource> dimensionX, Dimension<TSource> dimensionY, Dimension<TSource>? size, Dimension<TSource>? color)
    {
        var chart = new ScatterChart(data)
            .ColorScheme(ColorScheme.Default)
            .CartesianGrid(new CartesianGrid().Horizontal().Vertical())
            .Scatter(new Scatter("Value"))
            .XAxis(new XAxis(dimensionX.Name).Type(AxisTypes.Number).TickLine(false).AxisLine(false))
            .YAxis(new YAxis(dimensionY.Name).Type(AxisTypes.Number).TickLine(false).AxisLine(false))
            .Tooltip(new ChartTooltip().Animated(true));

        if (size is not null)
        {
            chart = chart.ZAxis(new ZAxis(size.Name));
        }

        return chart;
    }
}

public class CustomScatterChartStyle<TSource> : IScatterChartStyle<TSource>
{
    public ScatterChart Design(ExpandoObject[] data, Dimension<TSource> dimensionX, Dimension<TSource> dimensionY, Dimension<TSource>? size, Dimension<TSource>? color)
    {
        var chart = new ScatterChart(data)
            .ColorScheme(ColorScheme.Rainbow)
            .CartesianGrid(new CartesianGrid().Horizontal().Vertical())
            .Scatter(new Scatter("Value").Shape(ScatterShape.Diamond))
            .XAxis(new XAxis(dimensionX.Name).Type(AxisTypes.Number).TickLine(true).AxisLine(true))
            .YAxis(new YAxis(dimensionY.Name).Type(AxisTypes.Number).TickLine(true).AxisLine(true))
            .Tooltip(new ChartTooltip().Animated(true))
            .Legend(new Legend()
                .Layout(Legend.Layouts.Horizontal)
                .Align(Legend.Alignments.Center)
                .VerticalAlign(Legend.VerticalAlignments.Bottom));

        if (size is not null)
        {
            chart = chart.ZAxis(new ZAxis(size.Name));
        }

        return chart;
    }
}

public class ScatterChartBuilder<TSource>(
    IQueryable<TSource> data,
    Dimension<TSource>? dimensionX = null,
    Dimension<TSource>? dimensionY = null,
    Dimension<TSource>? size = null,
    Dimension<TSource>? color = null,
    IScatterChartStyle<TSource>? style = null,
    Func<ScatterChart, ScatterChart>? polish = null
)
    : ViewBase
{
    private Toolbox? _toolbox;
    private Func<Toolbox, Toolbox>? _toolboxFactory;
    private Expression<Func<TSource, object>>? _sortSelector;
    private SortOrder _sortOrder = SortOrder.None;
    private Size? _height;
    private Size? _width;

    public override object? Build()
    {
        if (dimensionX is null)
        {
            throw new InvalidOperationException("A dimension for X axis is required.");
        }

        if (dimensionY is null)
        {
            throw new InvalidOperationException("A dimension for Y axis is required.");
        }

        var scatterChartData = UseState(ImmutableArray.Create<Dictionary<string, object>>);
        var loading = UseState(true);

        UseEffect(async () =>
        {
            try
            {
                // Build the data transformation
                var dimensions = new List<Dimension<TSource>> { dimensionX, dimensionY };
                if (size is not null) dimensions.Add(size);
                if (color is not null) dimensions.Add(color);

                // Compile selectors outside the expression tree
                var xSelector = dimensionX.Selector.Compile();
                var ySelector = dimensionY.Selector.Compile();
                var sizeSelector = size?.Selector.Compile();
                var colorSelector = color?.Selector.Compile();

                var queryData = data.Select(item => new
                {
                    X = xSelector(item),
                    Y = ySelector(item),
                    Size = sizeSelector != null ? sizeSelector(item) : null,
                    Color = colorSelector != null ? colorSelector(item) : null,
                    Value = 1
                }).ToList();

                var results = queryData.Select(d =>
                {
                    var dict = new Dictionary<string, object>
                    {
                        [dimensionX.Name] = d.X,
                        [dimensionY.Name] = d.Y,
                        ["Value"] = d.Value
                    };
                    if (size is not null && d.Size is not null)
                        dict[size.Name] = d.Size;
                    if (color is not null && d.Color is not null)
                        dict[color.Name] = d.Color;
                    return dict;
                }).ToArray();

                scatterChartData.Set([.. results]);
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

        var resolvedDesigner = style ?? ScatterChartStyleHelpers.GetStyle<TSource>(ScatterChartStyles.Default);

        var scaffolded = resolvedDesigner.Design(
            scatterChartData.Value.ToExpando(),
            dimensionX,
            dimensionY,
            size,
            color
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

    public ScatterChartBuilder<TSource> Height(Size size)
    {
        _height = size;
        return this;
    }

    public ScatterChartBuilder<TSource> Width(Size size)
    {
        _width = size;
        return this;
    }

    public ScatterChartBuilder<TSource> Dimension(string name, Expression<Func<TSource, object>> selector)
    {
        dimensionX = new Dimension<TSource>(name, selector);
        return this;
    }

    public ScatterChartBuilder<TSource> Measure(string name, Expression<Func<TSource, object>> selector)
    {
        dimensionY = new Dimension<TSource>(name, selector);
        return this;
    }

    public ScatterChartBuilder<TSource> Size(string name, Expression<Func<TSource, object>> selector)
    {
        size = new Dimension<TSource>(name, selector);
        return this;
    }

    public ScatterChartBuilder<TSource> Color(string name, Expression<Func<TSource, object>> selector)
    {
        color = new Dimension<TSource>(name, selector);
        return this;
    }

    public ScatterChartBuilder<TSource> Toolbox(Toolbox toolbox)
    {
        ArgumentNullException.ThrowIfNull(toolbox);
        _toolbox = toolbox;
        _toolboxFactory = null;
        return this;
    }

    public ScatterChartBuilder<TSource> Toolbox(Func<Toolbox, Toolbox> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        _toolbox = null;
        _toolboxFactory = configure;
        return this;
    }

    public ScatterChartBuilder<TSource> Toolbox()
    {
        return Toolbox(_ => new Toolbox());
    }

    public ScatterChartBuilder<TSource> SortBy(Expression<Func<TSource, object>> selector, SortOrder order = SortOrder.Ascending)
    {
        _sortSelector = selector;
        _sortOrder = order;
        return this;
    }

    public ScatterChartBuilder<TSource> SortBy(SortOrder order)
    {
        _sortOrder = order;
        _sortSelector = null;
        return this;
    }
}

public static partial class ScatterChartExtensions
{
    public static ScatterChartBuilder<TSource> ToScatterChart<TSource>(
        this IEnumerable<TSource> data,
        ScatterChartStyles style = ScatterChartStyles.Default,
        Func<ScatterChart, ScatterChart>? polish = null)
    {
        return data.AsQueryable().ToScatterChart(style, polish);
    }

    [OverloadResolutionPriority(1)]
    public static ScatterChartBuilder<TSource> ToScatterChart<TSource>(
        this IQueryable<TSource> data,
        ScatterChartStyles style = ScatterChartStyles.Default,
        Func<ScatterChart, ScatterChart>? polish = null)
    {
        return new ScatterChartBuilder<TSource>(data,
            null,
            null,
            null,
            null,
            ScatterChartStyleHelpers.GetStyle<TSource>(style),
            polish
        );
    }
}
