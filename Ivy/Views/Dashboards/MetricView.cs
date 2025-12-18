using Ivy.Core;
using Ivy.Shared;

namespace Ivy.Views.Dashboards;

public record MetricRecord(
    string MetricFormatted,
    double? TrendComparedToPreviousPeriod = null,
    double? GoalAchieved = null,
    string? GoalFormatted = null);

public class MetricView(
    string title,
    Icons? icon,
    Func<Task<MetricRecord>> metricData
) : ViewBase
{
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
            return new Card(
                Layout.Vertical().Gap(2)
                | (Layout.Horizontal().Gap(2)
                   | Text.Small(title).NoWrap().Overflow(Overflow.Ellipsis).Color(Colors.Gray)
                   | new Spacer().Width(Size.Grow())
                   | (icon?.ToIcon().Color(Colors.Gray)))
                | new ErrorTeaserView(failed.Value)
            ).Height(Size.Full());
        }

        if (data.Value is null)
        {
            return new Card(
                Layout.Vertical().Gap(2)
                | (Layout.Horizontal().Gap(2)
                   | Text.Small(title).NoWrap().Overflow(Overflow.Ellipsis).Color(Colors.Gray)
                   | new Spacer().Width(Size.Grow())
                   | (icon?.ToIcon().Color(Colors.Gray)))
                | new Skeleton()
            ).Height(Size.Full());
        }

        var x = data.Value;

        object? footer = x.GoalAchieved != null
            ? new Progress((int)Math.Round(x.GoalAchieved.Value * 100.0))
                .ColorVariant(Progress.ColorVariants.EmeraldGradient)
                .Goal(x.GoalFormatted)
            : null;

        return new Card(
                content: Text.ExtraLarge(x.MetricFormatted).NoWrap().Overflow(Overflow.Clip),
                header: Layout.Horizontal().Align(Align.Center)
                    | Text.H4(title).WithLayout().Grow()
                    | (Layout.Horizontal().Align(Align.Right).Gap(1).Width(Size.Fit())
                        | (x.TrendComparedToPreviousPeriod != null ? x.TrendComparedToPreviousPeriod >= 0
                                ? Icons.TrendingUp.ToIcon().Color(Colors.Success).Small()
                                : Icons.TrendingDown.ToIcon().Color(Colors.Destructive).Small()
                            : null)
                        | (x.TrendComparedToPreviousPeriod != null ? x.TrendComparedToPreviousPeriod >= 0
                                ? Text.Small(x.TrendComparedToPreviousPeriod.Value.ToString("P1")).Color(Colors.Success)
                                : Text.Small(x.TrendComparedToPreviousPeriod.Value.ToString("P1")).Color(Colors.Destructive)
                            : null)),
                footer: footer
        ).Height(Size.Full());
    }
}
