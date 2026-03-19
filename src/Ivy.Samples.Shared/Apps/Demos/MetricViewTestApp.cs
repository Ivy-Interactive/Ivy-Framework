namespace Ivy.Samples.Shared.Apps.Demos;

[App(icon: Icons.Activity)]
public class MetricViewTestApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Grid().Columns(2)
            | new MetricView(
                "Test Metric",
                Icons.Activity,
                ctx => ctx.UseQuery(
                    key: "test",
                    fetcher: () => Task.FromResult(new MetricRecord("100", 0.15, 0.75, "Target: 120"))
                )
            )
            | new MetricView(
                "Another Metric",
                null,
                ctx => ctx.UseQuery(
                    key: "test2",
                    fetcher: () => Task.FromResult(new MetricRecord("200", -0.05, 0.95, "Target: 210"))
                )
            )
            | new MetricView(
                "No Goal Metric",
                Icons.TrendingUp,
                ctx => ctx.UseQuery(
                    key: "test3",
                    fetcher: () => Task.FromResult(new MetricRecord("42", 0.33))
                )
            )
            | new MetricView(
                "Error Metric",
                Icons.TriangleAlert,
                ctx => ctx.UseQuery(
                    key: "test4",
                    fetcher: () => Task.FromException<MetricRecord>(new InvalidOperationException("Simulated data fetch error"))
                )
            );
    }
}
