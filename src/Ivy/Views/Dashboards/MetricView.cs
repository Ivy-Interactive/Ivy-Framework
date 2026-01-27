using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Hooks;
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
    Func<IViewContext, QueryResult<MetricRecord>> useMetricData
) : ViewBase
{
    public override object? Build()
    {
        var query = useMetricData(Context);

        if (query.Error is not null)
        {
            return new Card(
                Layout.Vertical().Gap(2)
                | (Layout.Horizontal().Gap(2)
                   | Text.P(title).Small().NoWrap().Overflow(Overflow.Ellipsis).Color(Colors.Gray)
                   | new Spacer().Width(Size.Grow())
                   | (icon?.ToIcon().Color(Colors.Gray)))
                | new ErrorTeaserView(query.Error)
            ).Height(Size.Full());
        }

        if (query.Loading || query.Value is null)
        {
            return new Card(
                Layout.Vertical().Gap(2)
                | (Layout.Horizontal().Gap(2)
                   | Text.P(title).Small().NoWrap().Overflow(Overflow.Ellipsis).Color(Colors.Gray)
                   | new Spacer().Width(Size.Grow())
                   | (icon?.ToIcon().Color(Colors.Gray)))
                | new Skeleton()
            ).Height(Size.Full());
        }

        var x = query.Value;

        object? footer = x.GoalAchieved != null
            ? new Progress((int)Math.Round(x.GoalAchieved.Value * 100.0))
                .ColorVariant(Progress.ColorVariants.EmeraldGradient)
                .Goal(x.GoalFormatted)
            : null;

        return new Card(
                content: Text.Display(x.MetricFormatted).NoWrap().Overflow(Overflow.Clip),
                header: Layout.Horizontal().Align(Align.Center)
                    | Text.H4(title).NoWrap().Overflow(Overflow.Ellipsis).Width(Size.Grow())
                    | (Layout.Horizontal().Align(Align.Right).Gap(1).Width(Size.Fit())
                        | (x.TrendComparedToPreviousPeriod != null ? x.TrendComparedToPreviousPeriod >= 0
                                ? Icons.TrendingUp.ToIcon().Color(Colors.Success).Small()
                                : Icons.TrendingDown.ToIcon().Color(Colors.Destructive).Small()
                            : null)
                        | (x.TrendComparedToPreviousPeriod != null ? x.TrendComparedToPreviousPeriod >= 0
                                ? Text.P(x.TrendComparedToPreviousPeriod.Value.ToString("P1")).Small().Color(Colors.Success)
                                : Text.P(x.TrendComparedToPreviousPeriod.Value.ToString("P1")).Small().Color(Colors.Destructive)
                            : null)
                        | (icon?.ToIcon().Color(Colors.Gray))),
                footer: footer
        ).Height(Size.Full());
    }
}
