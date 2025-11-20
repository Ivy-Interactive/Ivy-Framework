using Ivy.Charts;
using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets.Charts;

[App(icon: Icons.ChartLine, searchHints: ["cohort", "retention", "churn", "ltv", "heatmap"])]
public class HeatmapChartApp : ViewBase
{
    public override object? Build()
    {
        var retentionData = new[]
        {
            new { Cohort = "Jan", Period = 1, Value = 100, Label = "100%" },
            new { Cohort = "Jan", Period = 2, Value = 45, Label = "45%" },
            new { Cohort = "Jan", Period = 3, Value = 35, Label = "35%" },
            new { Cohort = "Jan", Period = 4, Value = 30, Label = "30%" },

            new { Cohort = "Feb", Period = 2, Value = 100, Label = "100%" },
            new { Cohort = "Feb", Period = 3, Value = 48, Label = "48%" },
            new { Cohort = "Feb", Period = 4, Value = 38, Label = "38%" },

            new { Cohort = "Mar", Period = 3, Value = 100, Label = "100%" },
            new { Cohort = "Mar", Period = 4, Value = 50, Label = "50%" },

            new { Cohort = "Apr", Period = 4, Value = 100, Label = "100%" },

            new { Cohort = "Total", Period = 1, Value = 100, Label = "100" },
            new { Cohort = "Total", Period = 2, Value = 245, Label = "245" },
            new { Cohort = "Total", Period = 3, Value = 283, Label = "283" },
            new { Cohort = "Total", Period = 4, Value = 318, Label = "318" },
        };

        var ltvData = new[]
        {
            new { Cohort = "Jan", Period = 1, Value = 10, Label = "$10" },
            new { Cohort = "Jan", Period = 2, Value = 15, Label = "$15" },
            new { Cohort = "Jan", Period = 3, Value = 20, Label = "$20" },
            new { Cohort = "Jan", Period = 4, Value = 25, Label = "$25" },

            new { Cohort = "Feb", Period = 2, Value = 12, Label = "$12" },
            new { Cohort = "Feb", Period = 3, Value = 18, Label = "$18" },
            new { Cohort = "Feb", Period = 4, Value = 22, Label = "$22" },

            new { Cohort = "Mar", Period = 3, Value = 11, Label = "$11" },
            new { Cohort = "Mar", Period = 4, Value = 16, Label = "$16" },

            new { Cohort = "Apr", Period = 4, Value = 8, Label = "$8" },
        };

        var churnData = new[]
        {
            new { Cohort = "Jan", Period = 1, Value = 0, Label = "0%" },
            new { Cohort = "Jan", Period = 2, Value = 55, Label = "55%" },
            new { Cohort = "Jan", Period = 3, Value = 65, Label = "65%" },
            new { Cohort = "Jan", Period = 4, Value = 70, Label = "70%" },

            new { Cohort = "Feb", Period = 2, Value = 0, Label = "0%" },
            new { Cohort = "Feb", Period = 3, Value = 52, Label = "52%" },
            new { Cohort = "Feb", Period = 4, Value = 62, Label = "62%" },

            new { Cohort = "Mar", Period = 3, Value = 0, Label = "0%" },
            new { Cohort = "Mar", Period = 4, Value = 50, Label = "50%" },

            new { Cohort = "Apr", Period = 4, Value = 0, Label = "0%" },
        };

        var mrrData = new[]
        {
            new { Cohort = "Jan", Period = 1, Value = 5000, Label = "$5k" },
            new { Cohort = "Jan", Period = 2, Value = 4800, Label = "$4.8k" },
            new { Cohort = "Jan", Period = 3, Value = 4200, Label = "$4.2k" },
            new { Cohort = "Jan", Period = 4, Value = 3800, Label = "$3.8k" },

            new { Cohort = "Feb", Period = 2, Value = 6000, Label = "$6k" },
            new { Cohort = "Feb", Period = 3, Value = 5700, Label = "$5.7k" },
            new { Cohort = "Feb", Period = 4, Value = 5200, Label = "$5.2k" },

            new { Cohort = "Mar", Period = 3, Value = 5500, Label = "$5.5k" },
            new { Cohort = "Mar", Period = 4, Value = 5200, Label = "$5.2k" },

            new { Cohort = "Apr", Period = 4, Value = 7000, Label = "$7k" },
        };

        return Layout.Grid().Columns(2)
            | (new Card()
                | new HeatmapChart(retentionData)
                    .Title("Retention Rate")
                    .MetricType("percentage")
                    .ShowTotal())
            | (new Card()
                | new HeatmapChart(ltvData)
                    .Title("LTV per User")
                    .MetricType("currency")
                    .ShowTotal())
            | (new Card()
                | new HeatmapChart(churnData)
                    .Title("Churn Rate")
                    .MetricType("percentage")
                    .ShowTotal())
            | (new Card()
                | new HeatmapChart(mrrData)
                    .Title("MRR per Cohort")
                    .MetricType("currency")
                    .ShowTotal())
        ;
    }
}
