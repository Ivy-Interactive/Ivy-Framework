using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy
{

    /// <summary>
    /// A funnel chart to visualize sequential stages in a process.
    /// </summary>
    public record FunnelChart : WidgetBase<FunnelChart>
    {
        public FunnelChart(object data) : this()
        {
            Data = data;
        }

        internal FunnelChart()
        {
            Width = Size.Full();
            Height = Size.Full();
        }

        [Prop] public object? Data { get; init; }

        [Prop] public ColorScheme ColorScheme { get; init; } = ColorScheme.Default;

        [Prop] public Legend? Legend { get; init; } = null;

        [Prop] public Toolbox? Toolbox { get; init; } = null;

        [Prop] public Funnel[] Funnels { get; init; } = [];

        [Prop] public ChartTooltip? Tooltip { get; init; }

        public static FunnelChart operator |(FunnelChart widget, object child)
        {
            throw new NotSupportedException("FunnelChart does not support children.");
        }
    }

    public static class FunnelChartExtensions
    {
        public static FunnelChart Funnel(this FunnelChart chart, Funnel funnel)
        {
            return chart with { Funnels = [.. chart.Funnels, funnel] };
        }

        public static FunnelChart Funnel(this FunnelChart chart, string dataKey, string nameKey)
        {
            return chart with { Funnels = [.. chart.Funnels, new Funnel(dataKey, nameKey)] };
        }

        public static FunnelChart ColorScheme(this FunnelChart chart, ColorScheme colorScheme)
        {
            return chart with { ColorScheme = colorScheme };
        }

        public static FunnelChart Legend(this FunnelChart chart, Legend? legend)
        {
            return chart with { Legend = legend };
        }

        public static FunnelChart Legend(this FunnelChart chart)
        {
            return chart with { Legend = new Legend() };
        }

        public static FunnelChart Toolbox(this FunnelChart chart, Toolbox toolbox)
        {
            return chart with { Toolbox = toolbox };
        }

        public static FunnelChart Toolbox(this FunnelChart chart)
        {
            return chart with { Toolbox = new Toolbox() };
        }

        public static FunnelChart Tooltip(this FunnelChart chart, ChartTooltip tooltip)
        {
            return chart with { Tooltip = tooltip };
        }

        public static FunnelChart Tooltip(this FunnelChart chart)
        {
            return chart with { Tooltip = new ChartTooltip() };
        }
    }

    public enum FunnelOrientations
    {
        Horizontal,
        Vertical
    }

    public record Funnel
    {
        public Funnel(string dataKey, string? nameKey = "name")
        {
            DataKey = dataKey;
            NameKey = nameKey;
        }

        internal Funnel()
        {
            DataKey = string.Empty;
        }

        public string DataKey { get; }

        public string? NameKey { get; set; } = "name";

        public LegendTypes LegendType { get; set; } = LegendTypes.Line;

        public Colors? Stroke { get; set; } = null;

        public int StrokeWidth { get; set; } = 1;

        public Colors? Fill { get; set; } = null;

        public double? FillOpacity { get; set; } = null;

        public string? StrokeDashArray { get; set; }

        public FunnelOrientations Orient { get; set; } = FunnelOrientations.Horizontal;

        public bool Animated { get; set; } = true;

        public LabelList[] LabelLists { get; set; } = [];
    }

    public static class FunnelExtensions
    {
        public static Funnel LegendType(this Funnel funnel, LegendTypes legendType)
        {
            return funnel with { LegendType = legendType };
        }

        public static Funnel Stroke(this Funnel funnel, Colors stroke)
        {
            return funnel with { Stroke = stroke };
        }

        public static Funnel StrokeWidth(this Funnel funnel, int strokeWidth)
        {
            return funnel with { StrokeWidth = strokeWidth };
        }

        public static Funnel StrokeDashArray(this Funnel funnel, string strokeDashArray)
        {
            return funnel with { StrokeDashArray = strokeDashArray };
        }

        public static Funnel Fill(this Funnel funnel, Colors fill)
        {
            return funnel with { Fill = fill };
        }

        public static Funnel FillOpacity(this Funnel funnel, double fillOpacity)
        {
            return funnel with { FillOpacity = fillOpacity };
        }

        public static Funnel Animated(this Funnel funnel, bool animated = true)
        {
            return funnel with { Animated = animated };
        }

        public static Funnel Orient(this Funnel funnel, FunnelOrientations orient)
        {
            return funnel with { Orient = orient };
        }

        public static Funnel LabelLists(this Funnel funnel, LabelList[] labelLists)
        {
            return funnel with { LabelLists = labelLists };
        }

        public static Funnel LabelList(this Funnel funnel, LabelList labelList)
        {
            return funnel with { LabelLists = [.. funnel.LabelLists, labelList] };
        }

        public static Funnel LabelList(this Funnel funnel, string dataKey)
        {
            return funnel with { LabelLists = [.. funnel.LabelLists, new LabelList(dataKey)] };
        }
    }
}

namespace Ivy.Views.Charts
{
    using System;
    using System.Collections.Immutable;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using Ivy.Core;
    using Ivy.Core.Hooks;

    public record FunnelChartData(string? Dimension, double Measure);

    public enum FunnelChartStyles
    {
        Default
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
                _ => throw new InvalidOperationException($"Style {style} not found.")
            };
        }
    }

    public class DefaultFunnelChartStyle<TSource> : IFunnelChartStyle<TSource>
    {
        public FunnelChart Design(FunnelChartData[] data)
        {
            return new FunnelChart(data)
                .Funnel(nameof(FunnelChartData.Measure), nameof(FunnelChartData.Dimension))
                .Tooltip(new ChartTooltip().Animated(true))
                .Legend(new Legend()
                    .Layout(Legend.Layouts.Horizontal)
                    .Align(Legend.Alignments.Center)
                    .VerticalAlign(Legend.VerticalAlignments.Bottom)
                );
        }
    }

    public class FunnelChartBuilder<TSource>(
        IQueryable<TSource> data,
        Dimension<TSource> dimension,
        Measure<TSource> measure,
        IFunnelChartStyle<TSource>? style = null,
        Func<FunnelChart, FunnelChart>? polish = null)
        : ViewBase
    {
        private Toolbox? _toolbox;
        private Func<Toolbox, Toolbox>? _toolboxFactory;

        public override object? Build()
        {
            var funnelChartData = UseState(ImmutableArray.Create<FunnelChartData>);
            var loading = UseState(true);
            var exception = UseState<Exception?>((Exception?)null);

            UseEffect(async () =>
            {
                try
                {
                    var results = await data
                      .ToPivotTable()
                      .Dimension(dimension).Measure(measure).Produces<FunnelChartData>().ExecuteAsync()
                      .ToArrayAsync();

                    var sortedResults = results.OrderByDescending(r => r.Measure).ToArray();
                    funnelChartData.Set([.. sortedResults]);
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
               funnelChartData.Value.ToArray()
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

    public static class FunnelChartExtensions
    {
        public static FunnelChartBuilder<TSource> ToFunnelChart<TSource>(
        this IEnumerable<TSource> data,
        Expression<Func<TSource, object>> dimension,
        Expression<Func<IQueryable<TSource>, object>> measure,
        FunnelChartStyles style = FunnelChartStyles.Default,
        Func<FunnelChart, FunnelChart>? polish = null)
        {
            return data.AsQueryable().ToFunnelChart(dimension, measure, style, polish);
        }

        [OverloadResolutionPriority(1)]
        public static FunnelChartBuilder<TSource> ToFunnelChart<TSource>(
        this IQueryable<TSource> data,
        Expression<Func<TSource, object>> dimension,
        Expression<Func<IQueryable<TSource>, object>> measure,
        FunnelChartStyles style = FunnelChartStyles.Default,
        Func<FunnelChart, FunnelChart>? polish = null)
        {
            return new FunnelChartBuilder<TSource>(data,
                new Dimension<TSource>(nameof(FunnelChartData.Dimension), dimension),
                new Measure<TSource>(nameof(FunnelChartData.Measure), measure),
                FunnelChartStyleHelpers.GetStyle<TSource>(style),
                polish
            );
        }
    }
}
