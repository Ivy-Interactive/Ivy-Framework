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

/// <summary>
/// Defines the available visual styles for bar charts.
/// </summary>
public enum BarChartStyles
{
    /// <summary>Default bar chart style with full axes, legend, and grid.</summary>
    Default,
    /// <summary>Dashboard-optimized style with vertical layout, rounded bars, labels, and hidden axes for compact display.</summary>
    Dashboard
}

/// <summary>
/// Interface for defining bar chart visual styles and configurations.
/// </summary>
/// <typeparam name="TSource">The type of the source data objects.</typeparam>
public interface IBarChartStyle<TSource>
{
    /// <summary>
    /// Designs and configures a bar chart with the specified data, dimension, and measures.
    /// </summary>
    /// <param name="data">The processed data in ExpandoObject format for chart rendering.</param>
    /// <param name="dimension">The dimension configuration for the category axis.</param>
    /// <param name="measures">The measure configurations for the data series.</param>
    /// <returns>A configured BarChart widget ready for rendering.</returns>
    BarChart Design(ExpandoObject[] data, Dimension<TSource> dimension, Measure<TSource>[] measures);
}

/// <summary>
/// Helper methods for creating bar chart style instances.
/// </summary>
public static class BarChartStyleHelpers
{
    /// <summary>
    /// Gets a bar chart style instance for the specified style type.
    /// </summary>
    /// <typeparam name="TSource">The type of the source data objects.</typeparam>
    /// <param name="style">The bar chart style to create.</param>
    /// <returns>An instance of the specified bar chart style.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the specified style is not found.</exception>
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

/// <summary>
/// Default bar chart style with full axes, legend, and grid for comprehensive data visualization.
/// </summary>
/// <typeparam name="TSource">The type of the source data objects.</typeparam>
public class DefaultBarChartStyle<TSource> : IBarChartStyle<TSource>
{
    /// <summary>
    /// Designs a default bar chart with Y-axis, legend, grid, and animated tooltip.
    /// </summary>
    /// <param name="data">The processed data for chart rendering.</param>
    /// <param name="dimension">The dimension configuration for the X-axis.</param>
    /// <param name="measures">The measure configurations for the data series.</param>
    /// <returns>A fully configured bar chart with default styling.</returns>
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

/// <summary>
/// Dashboard-optimized bar chart style with vertical layout, rounded bars, and embedded labels for compact display.
/// </summary>
/// <typeparam name="TSource">The type of the source data objects.</typeparam>
public class DashboardBarChartStyle<TSource> : IBarChartStyle<TSource>
{
    /// <summary>
    /// Designs a dashboard bar chart with vertical layout, rounded bars, embedded labels, and hidden axes for compact visualization.
    /// </summary>
    /// <param name="data">The processed data for chart rendering.</param>
    /// <param name="dimension">The dimension configuration for the Y-axis categories.</param>
    /// <param name="measures">The measure configurations for the data series.</param>
    /// <returns>A compact bar chart optimized for dashboard display with vertical layout and embedded labels.</returns>
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

/// <summary>
/// A fluent builder for creating bar charts from data sources with dimensions and measures.
/// </summary>
/// <typeparam name="TSource">The type of the source data objects.</typeparam>
/// <remarks>
/// Provides a fluent API for configuring bar charts with automatic data processing,
/// asynchronous loading, and customizable styling. Transforms source data into pivot table format
/// and applies the specified visual style to create the final chart.
/// </remarks>
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

    /// <summary>
    /// Builds the bar chart by processing the data and applying the configured style.
    /// </summary>
    /// <returns>A BarChart widget with the processed data and applied styling, or a loading indicator during data processing.</returns>
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

    /// <summary>
    /// Configures the dimension (category axis) for the bar chart.
    /// </summary>
    /// <param name="name">The display name for the dimension.</param>
    /// <param name="selector">Expression to select the dimension value from source objects.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public BarChartBuilder<TSource> Dimension(string name, Expression<Func<TSource, object>> selector)
    {
        dimension = new Dimension<TSource>(name, selector);
        return this;
    }

    /// <summary>
    /// Adds a measure (data series) to the bar chart.
    /// </summary>
    /// <param name="name">The display name for the measure.</param>
    /// <param name="aggregator">Expression to aggregate the measure values from the data source.</param>
    /// <returns>The builder instance for method chaining.</returns>
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

/// <summary>
/// Extension methods for creating bar charts from data collections.
/// </summary>
public static class BarChartExtensions
{
    /// <summary>
    /// Creates a bar chart builder from an enumerable data source.
    /// </summary>
    /// <typeparam name="TSource">The type of the source data objects.</typeparam>
    /// <param name="data">The enumerable data source.</param>
    /// <param name="dimension">Optional dimension expression for the category axis.</param>
    /// <param name="measures">Optional array of measure expressions for data series.</param>
    /// <param name="style">The visual style to apply to the chart.</param>
    /// <param name="polish">Optional function to apply final customizations to the chart.</param>
    /// <returns>A BarChartBuilder for fluent configuration.</returns>
    public static BarChartBuilder<TSource> ToBarChart<TSource>(
        this IEnumerable<TSource> data,
        Expression<Func<TSource, object>>? dimension = null,
        Expression<Func<IQueryable<TSource>, object>>[]? measures = null,
        BarChartStyles style = BarChartStyles.Default,
        Func<BarChart, BarChart>? polish = null)
    {
        return data.AsQueryable().ToBarChart(dimension, measures, style, polish);
    }

    /// <summary>
    /// Creates a bar chart builder from a queryable data source.
    /// </summary>
    /// <typeparam name="TSource">The type of the source data objects.</typeparam>
    /// <param name="data">The queryable data source.</param>
    /// <param name="dimension">Optional dimension expression for the category axis.</param>
    /// <param name="measures">Optional array of measure expressions for data series.</param>
    /// <param name="style">The visual style to apply to the chart.</param>
    /// <param name="polish">Optional function to apply final customizations to the chart.</param>
    /// <returns>A BarChartBuilder for fluent configuration.</returns>
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

