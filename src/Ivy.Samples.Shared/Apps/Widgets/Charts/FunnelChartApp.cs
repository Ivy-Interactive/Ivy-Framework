using System.Collections.Generic;
using System.Linq;
using Ivy.Charts;
using Ivy.Core;
using Ivy.Shared;
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

        return Layout.Grid().Columns(1)
            | (new Card().Title("Standard Funnel").Description("Shows stages in a conversion pipeline using default settings.")
                | data.ToFunnelChart(
                    dimension: x => x.Stage,
                    measure: q => q.Sum(x => x.Value))
                    .Toolbox()
              )
            | (new Card().Title("Rainbow Funnel").Description("Shows stages using the rainbow color scheme and explicit legend alignment.")
                | data.ToFunnelChart(
                    dimension: x => x.Stage,
                    measure: q => q.Sum(x => x.Value),
                    polish: chart => chart
                        .ColorScheme(Ivy.Charts.ColorScheme.Rainbow)
                        .Legend(new Ivy.Charts.Legend()
                            .Align(Ivy.Charts.Legend.Alignments.Left)
                            .VerticalAlign(Ivy.Charts.Legend.VerticalAlignments.Middle)
                            .Layout(Ivy.Charts.Legend.Layouts.Vertical)
                        )
                ).Toolbox());
    }
}
