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
/// Defines the available visual styles for line charts.
/// </summary>
public enum LineChartStyles
{
    /// <summary>Default line chart style with full axes, legend, and basic line styling.</summary>
    Default,
    /// <summary>Dashboard-optimized style with natural curves, grid, and minimal axes for compact display.</summary>
    Dashboard,
    /// <summary>Custom style with rainbow colors, step curves, full grid, and enhanced visual elements.</summary>
    Custom
}

/// <summary>
/// Interface for defining line chart visual styles and configurations.
/// </summary>
/// <typeparam name="TSource">The type of the source data objects.</typeparam>
public interface ILineChartStyle<TSource>
{
    /// <summary>
    /// Designs and configures a line chart with the specified data, dimension, measures, and table calculations.
    /// </summary>
    /// <param name="data">The processed data in ExpandoObject format for chart rendering.</param>
    /// <param name="dimension">The dimension configuration for the X-axis.</param>
    /// <param name="measures">The measure configurations for the data series.</param>
    /// <param name="calculations">The table calculations for computed series.</param>
    /// <returns>A configured LineChart widget ready for rendering.</returns>
    LineChart Design(ExpandoObject[] data, Dimension<TSource> dimension, Measure<TSource>[] measures, TableCalculation[] calculations);
}

/// <summary>
/// Helper methods for creating line chart style instances.
/// </summary>
public static class LineChartStyleHelpers
{
    /// <summary>
    /// Gets a line chart style instance for the specified style type.
    /// </summary>
    /// <typeparam name="TSource">The type of the source data objects.</typeparam>
    /// <param name="style">The line chart style to create.</param>
    /// <returns>An instance of the specified line chart style.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the specified style is not found.</exception>
    public static ILineChartStyle<TSource> GetStyle<TSource>(LineChartStyles style)
    {
        return style switch
        {
            LineChartStyles.Default => new DefaultLineChartStyle<TSource>(),
            LineChartStyles.Dashboard => new DashboardLineChartStyle<TSource>(),
            LineChartStyles.Custom => new CustomLineChartStyle<TSource>(),
            _ => throw new InvalidOperationException($"Style {style} not found.")
        };
    }
}

/// <summary>
/// Default line chart style with full axes, legend, and basic line styling for comprehensive data visualization.
/// </summary>
/// <typeparam name="TSource">The type of the source data objects.</typeparam>
public class DefaultLineChartStyle<TSource> : ILineChartStyle<TSource>
{
    /// <summary>
    /// Designs a default line chart with Y-axis, legend, and animated tooltip.
    /// </summary>
    /// <param name="data">The processed data for chart rendering.</param>
    /// <param name="dimension">The dimension configuration for the X-axis.</param>
    /// <param name="measures">The measure configurations for the data series.</param>
    /// <param name="calculations">The table calculations for computed series.</param>
    /// <returns>A fully configured line chart with default styling.</returns>
    public LineChart Design(ExpandoObject[] data, Dimension<TSource> dimension, Measure<TSource>[] measures, TableCalculation[] calculations)
    {

        return new LineChart(data)
                .Line(measures.Select(m => new Line(m.Name)).ToArray())
                .Line(calculations.Select(c => new Line(c.Name)).ToArray())
                .YAxis(new YAxis())
                .XAxis(dimension.Name)
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
/// Dashboard-optimized line chart style with natural curves, horizontal grid, and minimal axes for compact display.
/// </summary>
/// <typeparam name="TSource">The type of the source data objects.</typeparam>
public class DashboardLineChartStyle<TSource> : ILineChartStyle<TSource>
{
    /// <summary>
    /// Designs a dashboard line chart with natural curve lines, horizontal grid, and minimal X-axis for compact visualization.
    /// </summary>
    /// <param name="data">The processed data for chart rendering.</param>
    /// <param name="dimension">The dimension configuration for the X-axis.</param>
    /// <param name="measures">The measure configurations for the data series with natural curves and increased stroke width.</param>
    /// <param name="calculations">The table calculations for computed series with natural curves.</param>
    /// <returns>A compact line chart optimized for dashboard display with smooth curves and minimal visual clutter.</returns>
    public LineChart Design(ExpandoObject[] data, Dimension<TSource> dimension, Measure<TSource>[] measures, TableCalculation[] calculations)
    {
        return new LineChart(data)
                .ColorScheme(ColorScheme.Default)
                .CartesianGrid(new CartesianGrid().Horizontal())
                .Line(measures.Select(m => new Line(m.Name).CurveType(CurveTypes.Natural).StrokeWidth(2)).ToArray())
                .Line(calculations.Select(c => new Line(c.Name).CurveType(CurveTypes.Natural)).ToArray())
                .XAxis(new XAxis(dimension.Name).TickLine(false).AxisLine(false).MinTickGap(10))
                .Tooltip(new Ivy.Charts.Tooltip().Animated(true))
            ;
    }
}

/// <summary>
/// Custom line chart style with rainbow colors, step curves, full grid, and enhanced visual elements for distinctive presentation.
/// </summary>
/// <typeparam name="TSource">The type of the source data objects.</typeparam>
public class CustomLineChartStyle<TSource> : ILineChartStyle<TSource>
{
    /// <summary>
    /// Designs a custom line chart with rainbow color scheme, step curves, full grid, and enhanced axes for distinctive visualization.
    /// </summary>
    /// <param name="data">The processed data for chart rendering.</param>
    /// <param name="dimension">The dimension configuration for the X-axis with full styling.</param>
    /// <param name="measures">The measure configurations for the data series with step curves and thick strokes.</param>
    /// <param name="calculations">The table calculations for computed series with step curves.</param>
    /// <returns>A visually distinctive line chart with rainbow colors, step curves, and full grid for enhanced data presentation.</returns>
    public LineChart Design(ExpandoObject[] data, Dimension<TSource> dimension, Measure<TSource>[] measures, TableCalculation[] calculations)
    {
        return new LineChart(data)
                .ColorScheme(ColorScheme.Rainbow)
                .CartesianGrid(new CartesianGrid().Horizontal().Vertical())
                .Line(measures.Select(m => new Line(m.Name).CurveType(CurveTypes.Step).StrokeWidth(3)).ToArray())
                .Line(calculations.Select(c => new Line(c.Name).CurveType(CurveTypes.Step)).ToArray())
                .XAxis(new XAxis(dimension.Name).TickLine(true).AxisLine(true).MinTickGap(10))
                .YAxis(new YAxis().TickLine(true).AxisLine(true))
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
/// A fluent builder for creating line charts from data sources with dimensions, measures, and table calculations.
/// </summary>
/// <typeparam name="TSource">The type of the source data objects.</typeparam>
/// <remarks>
/// Provides a fluent API for configuring line charts with automatic data processing,
/// asynchronous loading, and customizable styling. Transforms source data into pivot table format
/// with support for both measures and table calculations, then applies the specified visual style to create the final chart.
/// </remarks>
public class LineChartBuilder<TSource>(
    IQueryable<TSource> data,
    Dimension<TSource>? dimension = null,
    Measure<TSource>[]? measures = null,
    ILineChartStyle<TSource>? style = null,
    Func<LineChart, LineChart>? polish = null
)
    : ViewBase
{
    private readonly List<Measure<TSource>> _measures = [.. measures ?? []];
    private readonly List<TableCalculation> _calculations = new();
    private Toolbox? _toolbox;
    private Func<Toolbox, Toolbox>? _toolboxFactory;

    /// <summary>
    /// Builds the line chart by processing the data and applying the configured style.
    /// </summary>
    /// <returns>A LineChart widget with the processed data and applied styling, or a loading indicator during data processing.</returns>
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
                    .Dimension(dimension).Measures(_measures).TableCalculations(_calculations).ExecuteAsync();
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

        var resolvedDesigner = style ?? LineChartStyleHelpers.GetStyle<TSource>(LineChartStyles.Default);

        var scaffolded = resolvedDesigner.Design(
            lineChartData.Value.ToExpando(),
            dimension,
            _measures.ToArray(),
            _calculations.ToArray()
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
    /// Configures the dimension (X-axis) for the line chart.
    /// </summary>
    /// <param name="name">The display name for the dimension.</param>
    /// <param name="selector">Expression to select the dimension value from source objects.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public LineChartBuilder<TSource> Dimension(string name, Expression<Func<TSource, object>> selector)
    {
        dimension = new Dimension<TSource>(name, selector);
        return this;
    }

    /// <summary>
    /// Adds a measure (data series) to the line chart.
    /// </summary>
    /// <param name="name">The display name for the measure.</param>
    /// <param name="aggregator">Expression to aggregate the measure values from the data source.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public LineChartBuilder<TSource> Measure(string name, Expression<Func<IQueryable<TSource>, object>> aggregator)
    {
        _measures.Add(new Measure<TSource>(name, aggregator));
        return this;
    }

    /// <summary>
    /// Adds a table calculation (computed series) to the line chart.
    /// </summary>
    /// <param name="calculation">The table calculation configuration for computed data series.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public LineChartBuilder<TSource> TableCalculation(TableCalculation calculation)
    {
        _calculations.Add(calculation);
        return this;
    }

    public LineChartBuilder<TSource> Toolbox(Toolbox toolbox)
    {
        ArgumentNullException.ThrowIfNull(toolbox);
        _toolbox = toolbox;
        _toolboxFactory = null;
        return this;
    }

    public LineChartBuilder<TSource> Toolbox(Func<Toolbox, Toolbox> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        _toolbox = null;
        _toolboxFactory = configure;
        return this;
    }

    public LineChartBuilder<TSource> Toolbox()
    {
        return Toolbox(_ => new Toolbox());
    }
}

/// <summary>
/// Extension methods for creating line charts from data collections.
/// </summary>
public static class LineChartExtensions
{
    /// <summary>
    /// Creates a line chart builder from an enumerable data source.
    /// </summary>
    /// <typeparam name="TSource">The type of the source data objects.</typeparam>
    /// <param name="data">The enumerable data source.</param>
    /// <param name="dimension">Optional dimension expression for the X-axis.</param>
    /// <param name="measures">Optional array of measure expressions for data series.</param>
    /// <param name="style">The visual style to apply to the chart.</param>
    /// <param name="polish">Optional function to apply final customizations to the chart.</param>
    /// <returns>A LineChartBuilder for fluent configuration.</returns>
    public static LineChartBuilder<TSource> ToLineChart<TSource>(
        this IEnumerable<TSource> data,
        Expression<Func<TSource, object>>? dimension = null,
        Expression<Func<IQueryable<TSource>, object>>[]? measures = null,
        LineChartStyles style = LineChartStyles.Default,
        Func<LineChart, LineChart>? polish = null)
    {
        return data.AsQueryable().ToLineChart(dimension, measures, style, polish);
    }

    /// <summary>
    /// Creates a line chart builder from a queryable data source.
    /// </summary>
    /// <typeparam name="TSource">The type of the source data objects.</typeparam>
    /// <param name="data">The queryable data source.</param>
    /// <param name="dimension">Optional dimension expression for the X-axis.</param>
    /// <param name="measures">Optional array of measure expressions for data series.</param>
    /// <param name="style">The visual style to apply to the chart.</param>
    /// <param name="polish">Optional function to apply final customizations to the chart.</param>
    /// <returns>A LineChartBuilder for fluent configuration.</returns>
    [OverloadResolutionPriority(1)]
    public static LineChartBuilder<TSource> ToLineChart<TSource>(
        this IQueryable<TSource> data,
        Expression<Func<TSource, object>>? dimension = null,
        Expression<Func<IQueryable<TSource>, object>>[]? measures = null,
        LineChartStyles style = LineChartStyles.Default,
        Func<LineChart, LineChart>? polish = null)
    {
        return new LineChartBuilder<TSource>(data,
            dimension != null ? new Dimension<TSource>(ExpressionNameHelper.SuggestName(dimension) ?? "Dimension", dimension) : null,
            measures?.Select(m => new Measure<TSource>(ExpressionNameHelper.SuggestName(m) ?? "Measure", m)).ToArray(),
            LineChartStyleHelpers.GetStyle<TSource>(style),
            polish
        );
    }
}

