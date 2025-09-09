using Ivy.Core;
using Ivy.Shared;

namespace Ivy.Views.Dashboards;

/// <summary>Dashboard metric data with formatted value, trend, and goal progress.</summary>
/// <param name="MetricFormatted">Formatted metric value.</param>
/// <param name="TrendComparedToPreviousPeriod">Optional trend percentage as decimal.</param>
/// <param name="GoalAchieved">Optional goal progress from 0 to 1.</param>
/// <param name="GoalFormatted">Optional formatted goal target.</param>
public record MetricRecord(
    string MetricFormatted,
    double? TrendComparedToPreviousPeriod,
    double? GoalAchieved,
    string? GoalFormatted);

/// <summary>Dashboard KPI component with async data loading, trends, and goal tracking.</summary>
public class MetricView(
    string title,
    Icons? icon,
    Func<Task<MetricRecord>> metricData
) : ViewBase
{
    /// <summary>Fixed height for consistent dashboard layout.</summary>
    private const int Height = 50;

    /// <summary>Builds the metric view with loading, error, or success states.</summary>
    public override object? Build()
    {
        var data = UseState<MetricRecord?>(() => null);
        var failed = UseState<Exception?>(() => null);

        UseEffect(async () =>
        {
            try
            {
                data.Set(await metricData());
            }
            catch (Exception ex)
            {
                failed.Set(ex);
            }
        }, []);

        if (failed.Value is not null)
        {
            return new Card().Title(title).Icon(icon).Height(Size.Units(Height)) | new ErrorTeaserView(failed.Value);
        }

        if (data.Value is null)
        {
            return new Card(
                new Skeleton()
            ).Title(title).Icon(icon).Height(Size.Units(Height));
        }

        var x = data.Value;

        return new Card(
                Layout.Horizontal().Align(Align.Left).Gap(2)
                    | Text.H3(x.MetricFormatted).NoWrap().Overflow(Overflow.Clip)
                    | (x.TrendComparedToPreviousPeriod != null
                        ? x.TrendComparedToPreviousPeriod >= 0
                            ? Icons.TrendingUp.ToIcon().Color(Colors.Primary)
                            : Icons.TrendingDown.ToIcon().Color(Colors.Red)
                        : null)
                    | (x.TrendComparedToPreviousPeriod != null
                        ? x.TrendComparedToPreviousPeriod >= 0
                            ? Text.Small(x.TrendComparedToPreviousPeriod.Value.ToString("P1")).Color(Colors.Primary)
                            : Text.Small(x.TrendComparedToPreviousPeriod.Value.ToString("P1")).Color(Colors.Red)
                        : null),
                x.GoalAchieved != null ? new Progress((int)Math.Round(x.GoalAchieved.Value * 100.0)).ColorVariant(Progress.ColorVariants.EmeraldGradient).Goal(x.GoalFormatted) : null
            ).Title(title).Icon(icon).Height(Size.Units(Height))
            ;
    }
}
