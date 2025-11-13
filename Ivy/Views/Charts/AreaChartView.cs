using System;
using System.Collections.Immutable;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Ivy.Charts;
using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;

namespace Ivy.Views.Charts;

/// <summary>
/// Defines the available visual styles for area charts.
/// </summary>
public enum AreaChartStyles
{
    /// <summary>Default area chart style with full axes, legend, and grid.</summary>
    Default,
    /// <summary>Dashboard-optimized style with minimal axes and no legend for compact display.</summary>
    Dashboard
}

/// <summary>
/// Interface for defining area chart visual styles and configurations.
/// </summary>
/// <typeparam name="TSource">The type of the source data objects.</typeparam>
public interface IAreaChartStyle<TSource>
{
    /// <summary>
    /// Designs and configures an area chart with the specified data, dimension, and measures.
    /// </summary>
    /// <param name="data">The processed data in ExpandoObject format for chart rendering.</param>
    /// <param name="dimension">The dimension configuration for the X-axis.</param>
    /// <param name="measures">The measure configurations for the data series.</param>
    /// <returns>A configured AreaChart widget ready for rendering.</returns>
    AreaChart Design(ExpandoObject[] data, Dimension<TSource> dimension, Measure<TSource>[] measures);
}

/// <summary>
/// Helper methods for creating area chart style instances.
/// </summary>
public static class AreaChartStyleHelpers
{
    /// <summary>
    /// Gets an area chart style instance for the specified style type.
    /// </summary>
    /// <typeparam name="TSource">The type of the source data objects.</typeparam>
    /// <param name="style">The area chart style to create.</param>
    /// <returns>An instance of the specified area chart style.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the specified style is not found.</exception>
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

/// <summary>
/// Default area chart style with full axes, legend, and grid for comprehensive data visualization.
/// </summary>
/// <typeparam name="TSource">The type of the source data objects.</typeparam>
public class DefaultAreaChartStyle<TSource> : IAreaChartStyle<TSource>
{
    /// <summary>
    /// Designs a default area chart with Y-axis, legend, grid, and animated tooltip.
    /// </summary>
    /// <param name="data">The processed data for chart rendering.</param>
    /// <param name="dimension">The dimension configuration for the X-axis.</param>
    /// <param name="measures">The measure configurations for the data series.</param>
    /// <returns>A fully configured area chart with default styling.</returns>
    public AreaChart Design(ExpandoObject[] data, Dimension<TSource> dimension, Measure<TSource>[] measures)
    {
        return new AreaChart(data)
            .Area(measures.Select(m => new Area(m.Name, 1)).ToArray())
            .YAxis(new YAxis())
            .XAxis(new XAxis(dimension.Name).TickLine(false).AxisLine(false).MinTickGap(10))
            .CartesianGrid(new CartesianGrid().Horizontal())
            .Tooltip(new Ivy.Charts.Tooltip().Animated(true))
            .Legend();

    }
}

/// <summary>
/// Dashboard-optimized area chart style with minimal visual elements for compact display.
/// </summary>
/// <typeparam name="TSource">The type of the source data objects.</typeparam>
public class DashboardAreaChartStyle<TSource> : IAreaChartStyle<TSource>
{
    /// <summary>
    /// Designs a dashboard area chart with minimal axes and no legend for compact visualization.
    /// </summary>
    /// <param name="data">The processed data for chart rendering.</param>
    /// <param name="dimension">The dimension configuration for the X-axis.</param>
    /// <param name="measures">The measure configurations for the data series.</param>
    /// <returns>A compact area chart optimized for dashboard display.</returns>
    public AreaChart Design(ExpandoObject[] data, Dimension<TSource> dimension, Measure<TSource>[] measures)
    {
        return new AreaChart(data)
            .ColorScheme(ColorScheme.Default)
            .Area(measures.Select(m => new Area(m.Name, 1)).ToArray())
            .XAxis(new XAxis(dimension.Name).TickLine(false).AxisLine(false).MinTickGap(10))
            .CartesianGrid(new CartesianGrid().Horizontal())
            .Tooltip(new Ivy.Charts.Tooltip().Animated(true))
        ;
    }
}

/// <summary>
/// A fluent builder for creating area charts from data sources with dimensions and measures.
/// </summary>
/// <typeparam name="TSource">The type of the source data objects.</typeparam>
/// <remarks>
/// Provides a fluent API for configuring area charts with automatic data processing,
/// asynchronous loading, and customizable styling. Transforms source data into pivot table format
/// and applies the specified visual style to create the final chart.
/// </remarks>
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

    /// <summary>
    /// Builds the area chart by processing the data and applying the configured style.
    /// </summary>
    /// <returns>An AreaChart widget with the processed data and applied styling, or a loading indicator during data processing.</returns>
    /// <exception cref="InvalidOperationException">Thrown when dimension or measures are not configured.</exception>
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

        return polish?.Invoke(configuredChart) ?? configuredChart;
    }

    /// <summary>
    /// Configures the dimension (X-axis) for the area chart.
    /// </summary>
    /// <param name="name">The display name for the dimension.</param>
    /// <param name="selector">Expression to select the dimension value from source objects.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public AreaChartBuilder<TSource> Dimension(string name, Expression<Func<TSource, object>> selector)
    {
        dimension = new Dimension<TSource>(name, selector);
        return this;
    }

    /// <summary>
    /// Adds a measure (data series) to the area chart.
    /// </summary>
    /// <param name="name">The display name for the measure.</param>
    /// <param name="aggregator">Expression to aggregate the measure values from the data source.</param>
    /// <returns>The builder instance for method chaining.</returns>
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
}

/// <summary>
/// Extension methods for creating area charts from data collections.
/// </summary>
public static class AreaChartExtensions
{
    /// <summary>
    /// Creates an area chart builder from an enumerable data source.
    /// </summary>
    /// <typeparam name="TSource">The type of the source data objects.</typeparam>
    /// <param name="data">The enumerable data source.</param>
    /// <param name="dimension">Optional dimension expression for the X-axis.</param>
    /// <param name="measures">Optional array of measure expressions for data series.</param>
    /// <param name="style">The visual style to apply to the chart.</param>
    /// <param name="polish">Optional function to apply final customizations to the chart.</param>
    /// <returns>An AreaChartBuilder for fluent configuration.</returns>
    public static AreaChartBuilder<TSource> ToAreaChart<TSource>(
        this IEnumerable<TSource> data,
        Expression<Func<TSource, object>>? dimension = null,
        Expression<Func<IQueryable<TSource>, object>>[]? measures = null,
        AreaChartStyles style = AreaChartStyles.Default,
        Func<AreaChart, AreaChart>? polish = null)
    {
        return data.AsQueryable().ToAreaChart(dimension, measures, style, polish);
    }

    /// <summary>
    /// Creates an area chart builder from a queryable data source.
    /// </summary>
    /// <typeparam name="TSource">The type of the source data objects.</typeparam>
    /// <param name="data">The queryable data source.</param>
    /// <param name="dimension">Optional dimension expression for the X-axis.</param>
    /// <param name="measures">Optional array of measure expressions for data series.</param>
    /// <param name="style">The visual style to apply to the chart.</param>
    /// <param name="polish">Optional function to apply final customizations to the chart.</param>
    /// <returns>An AreaChartBuilder for fluent configuration.</returns>
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

