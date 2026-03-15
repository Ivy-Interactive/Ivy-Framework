using System.Collections.Generic;
using System.Linq;
using Ivy.Core;
using Ivy.Views.Charts;

namespace Ivy.Samples.Shared.Apps.Widgets.Charts;

[App("Charts", "Funnel Chart", Order = 100)]
public class FunnelChartApp : SampleBase
{
    private readonly record struct FunnelData(string Stage, int Value);

    protected override object? BuildSample()
    {
        var data = new List<FunnelData>
        {
            new("Visits", 100000),
            new("Signups", 80000),
            new("Cart", 60000),
            new("Checkout", 40000),
            new("Purchases", 20000)
        };

        return Layout.Grid().Columns(2)
            | ((new Card().Title("Standard Funnel").Description("Shows stages in a conversion pipeline using default settings.")
                | data.ToFunnelChart(
                    dimension: x => x.Stage,
                    measure: q => q.Sum(x => x.Value))
                    .Toolbox()
              ).GridColumnSpan(2))
            | ((new Card().Title("Rainbow Funnel").Description("Shows stages using the rainbow color scheme and explicit legend alignment.")
                | data.ToFunnelChart(
                    dimension: x => x.Stage,
                    measure: q => q.Sum(x => x.Value),
                    polish: chart => chart
                        .ColorScheme(ColorScheme.Rainbow)
                        .Legend(new Legend())
                ).Toolbox()).GridColumnSpan(2))
            | (new Card().Title("Vertical Funnel").Description("Shows stages in a conversion pipeline using vertical orientation.")
                | data.ToFunnelChart(
                    dimension: x => x.Stage,
                    measure: q => q.Sum(x => x.Value),
                    polish: chart => chart with { Funnels = [chart.Funnels[0].Orient(FunnelOrientations.Vertical)] }
                ).Toolbox())
            | (new Card().Title("Vertical Funnel (Rainbow)").Description("Shows vertical orientation with the rainbow color scheme.")
                | data.ToFunnelChart(
                    dimension: x => x.Stage,
                    measure: q => q.Sum(x => x.Value),
                    polish: chart => chart with
                    {
                        ColorScheme = ColorScheme.Rainbow,
                        Funnels = [chart.Funnels[0].Orient(FunnelOrientations.Vertical)]
                    }
                ).Toolbox());
    }
}
