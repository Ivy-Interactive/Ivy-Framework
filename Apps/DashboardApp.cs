using Ivy;
using Ivy.Tendril.Apps.Plans;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps;

[App(title: "Dashboard", icon: Icons.ChartBar, group: new[] { "Tools" }, order: 1)]
public class DashboardApp : ViewBase
{
    public override object? Build()
    {
        var planService = UseService<PlanReaderService>();
        var configService = UseService<ConfigService>();
        var refreshToken = UseRefreshToken();
        UseInterval(() =>
        {
            refreshToken.Refresh();
        }, TimeSpan.FromSeconds(60));

        var plans = planService.GetPlans();

        // Statistics cards
        var totalCount = plans.Count;
        var draftCount = plans.Count(p => p.Status == PlanStatus.Draft);
        var inProgressCount = plans.Count(p => p.Status is PlanStatus.Building or PlanStatus.Executing or PlanStatus.Updating);
        var reviewCount = plans.Count(p => p.Status == PlanStatus.ReadyForReview);
        var completedCount = plans.Count(p => p.Status == PlanStatus.Completed);
        var failedCount = plans.Count(p => p.Status == PlanStatus.Failed);

        var statsRow = Layout.Horizontal().Gap(2).Padding(2)
            | BuildStatCard(totalCount, "Total Plans")
            | BuildStatCard(draftCount, "Draft")
            | BuildStatCard(inProgressCount, "In Progress")
            | BuildStatCard(reviewCount, "Ready for Review")
            | BuildStatCard(completedCount, "Completed")
            | BuildStatCard(failedCount, "Failed");

        // Daily activity table - last 7 days
        var today = DateTime.UtcNow.Date;
        var days = Enumerable.Range(0, 7).Select(i => today.AddDays(-i)).ToList();

        var rows = days.Select(day =>
        {
            var dayLabel = day == today ? "Today"
                : day == today.AddDays(-1) ? "Yesterday"
                : day.ToString("MMM dd");

            var createdCount = plans.Count(p => p.Created.Date == day);
            var dayCompletedCount = plans.Count(p => p.Status == PlanStatus.Completed && p.Updated.Date == day);
            var prsMerged = plans.Where(p => p.Status == PlanStatus.Completed && p.Updated.Date == day).Sum(p => p.Prs.Count);
            var dayFailedCount = plans.Count(p => p.Status == PlanStatus.Failed && p.Updated.Date == day);

            var completedOrFailedPlans = plans
                .Where(p => p.Updated.Date == day && p.Status is PlanStatus.Completed or PlanStatus.Failed or PlanStatus.ReadyForReview)
                .ToList();

            var dayCost = completedOrFailedPlans.Sum(p => planService.GetPlanTotalCost(p.FolderPath));
            var dayTokens = completedOrFailedPlans.Sum(p => planService.GetPlanTotalTokens(p.FolderPath));

            return new DashboardDayRow
            {
                Date = dayLabel,
                SortDate = day,
                Created = createdCount,
                Completed = dayCompletedCount,
                PrsMerged = prsMerged,
                Failed = dayFailedCount,
                Cost = dayCost > 0 ? $"${dayCost:F2}" : "",
                Tokens = dayTokens > 0 ? FormatTokens(dayTokens) : ""
            };
        }).ToList();

        var dataTable = rows.AsQueryable()
            .ToDataTable(idSelector: t => t.SortDate.ToString("yyyy-MM-dd"))
            .RefreshToken(refreshToken)
            .Width(Size.Full())
            .Height(Size.Px(320))
            .Header(t => t.Date, "Date")
            .Header(t => t.Created, "Created")
            .Header(t => t.Completed, "Completed")
            .Header(t => t.PrsMerged, "PRs Merged")
            .Header(t => t.Failed, "Failed")
            .Header(t => t.Cost, "Cost")
            .Header(t => t.Tokens, "Tokens")
            .Hidden(t => t.SortDate)
            .Config(c =>
            {
                c.AllowSorting = false;
                c.AllowFiltering = false;
                c.ShowSearch = false;
                c.SelectionMode = SelectionModes.None;
                c.ShowIndexColumn = false;
                c.BatchSize = 7;
            });

        // Per-project breakdown chart
        var projectData = plans
            .GroupBy(p => p.Project)
            .Select(g => new { Project = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .ToArray();

        var projectProgress = new StackedProgress(
            projectData.Select(p => new ProgressSegment(
                Value: p.Count,
                Color: configService.GetProjectColor(p.Project),
                Label: p.Project
            )).ToArray()
        ).ShowLabels();

        // Hourly cost & tokens combined bar chart
        var hourlyBurn = planService.GetHourlyTokenBurn(days: 7);

        var combinedChart = hourlyBurn.ToBarChart(
                style: BarChartStyles.Default,
                polish: chart => chart with
                {
                    Bars =
                    [
                        new Bar("Cost ($)").Radius(4).FillOpacity(0.8).YAxisIndex(0),
                        new Bar("Tokens").Radius(4).FillOpacity(0.8).YAxisIndex(1),
                    ],
                    YAxis =
                    [
                        new YAxis("Cost ($)").TickFormatter("C2"),
                        new YAxis("Tokens").Orientation(YAxis.Orientations.Right),
                    ]
                })
            .Dimension("Hour", e => e.Hour.ToString("MM/dd HH"))
            .Measure("Cost ($)", e => e.Sum(f => (double)f.Cost))
            .Measure("Tokens", e => e.Sum(f => (double)f.Tokens))
            .Height(Size.Px(350))
            .Width(Size.Full());

        var content = Layout.Vertical().Gap(2)
            | projectProgress
            | dataTable
            | combinedChart;

        return new HeaderLayout(
            header: statsRow,
            content: content
        );
    }

    private static string FormatTokens(int tokens)
    {
        return tokens >= 1_000_000 ? $"{tokens / 1_000_000.0:F1}M"
             : tokens >= 1_000 ? $"{tokens / 1_000.0:F0}K"
             : tokens.ToString();
    }

    private static object BuildStatCard(int count, string label)
    {
        return Layout.Vertical().Padding(1)
            | Text.Block(count.ToString()).Bold()
            | Text.Muted(label);
    }
}

public class DashboardDayRow
{
    public string Date { get; set; } = "";
    public DateTime SortDate { get; set; }
    public int Created { get; set; }
    public int Completed { get; set; }
    public int PrsMerged { get; set; }
    public int Failed { get; set; }
    public string Cost { get; set; } = "";
    public string Tokens { get; set; } = "";
}
