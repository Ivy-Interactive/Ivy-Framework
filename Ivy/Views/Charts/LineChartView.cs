using System.Collections.Immutable;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Ivy.Charts;
using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;

namespace Ivy.Views.Charts;

public enum LineChartStyles
{
    Default,
    Dashboard,
    Custom
}

public interface ILineChartStyle<TSource>
{
    LineChart Design(ExpandoObject[] data, Dimension<TSource> dimension, Measure<TSource>[] measures, TableCalculation[] calculations);
}

public static class LineChartStyleHelpers
{
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

public class DefaultLineChartStyle<TSource> : ILineChartStyle<TSource>
{
    public LineChart Design(ExpandoObject[] data, Dimension<TSource> dimension, Measure<TSource>[] measures, TableCalculation[] calculations)
    {
        return new LineChart(data)
                .Line(measures.Select(m => new Line(m.Name)).ToArray())
                .Line(calculations.Select(c => new Line(c.Name)).ToArray())
                .YAxis(new YAxis())
                .XAxis(dimension.Name)
                .Tooltip(new Ivy.Charts.Tooltip().Animated(true))
                .Legend()
            ;
    }
}

public class DashboardLineChartStyle<TSource> : ILineChartStyle<TSource>
{
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

public class CustomLineChartStyle<TSource> : ILineChartStyle<TSource>
{
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
                .Legend()
            ;
    }
}

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

        return polish?.Invoke(scaffolded) ?? scaffolded;
    }

    public LineChartBuilder<TSource> Dimension(string name, Expression<Func<TSource, object>> selector)
    {
        dimension = new Dimension<TSource>(name, selector);
        return this;
    }

    public LineChartBuilder<TSource> Measure(string name, Expression<Func<IQueryable<TSource>, object>> aggregator)
    {
        _measures.Add(new Measure<TSource>(name, aggregator));
        return this;
    }

    public LineChartBuilder<TSource> TableCalculation(TableCalculation calculation)
    {
        _calculations.Add(calculation);
        return this;
    }
}

public static class LineChartExtensions
{
    public static LineChartBuilder<TSource> ToLineChart<TSource>(
        this IEnumerable<TSource> data,
        Expression<Func<TSource, object>>? dimension = null,
        Expression<Func<IQueryable<TSource>, object>>[]? measures = null,
        LineChartStyles style = LineChartStyles.Default,
        Func<LineChart, LineChart>? polish = null)
    {
        return data.AsQueryable().ToLineChart(dimension, measures, style, polish);
    }

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

