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

            var dayCost = plans
                .Where(p => p.Updated.Date == day && p.Status is PlanStatus.Completed or PlanStatus.Failed or PlanStatus.ReadyForReview)
                .Sum(p => planService.GetPlanTotalCost(p.FolderPath));

            return new DashboardDayRow
            {
                Date = dayLabel,
                SortDate = day,
                Created = createdCount,
                Completed = dayCompletedCount,
                PrsMerged = prsMerged,
                Failed = dayFailedCount,
                Cost = dayCost > 0 ? $"${dayCost:F2}" : ""
            };
        }).ToList();

        var dataTable = rows.AsQueryable()
            .ToDataTable(idSelector: t => t.Date)
            .RefreshToken(refreshToken)
            .Width(Size.Full())
            .Height(Size.Px(320))
            .Header(t => t.Date, "Date")
            .Header(t => t.Created, "Created")
            .Header(t => t.Completed, "Completed")
            .Header(t => t.PrsMerged, "PRs Merged")
            .Header(t => t.Failed, "Failed")
            .Header(t => t.Cost, "Cost")
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

        var projectChart = projectData.ToPieChart(
            e => e.Project,
            e => e.Sum(f => f.Count),
            PieChartStyles.Donut,
            total: new PieChartTotal(plans.Count.ToString(), "Total Plans")
        );

        var content = Layout.Vertical().Gap(2)
            | dataTable
            | projectChart;

        return new HeaderLayout(
            header: statsRow,
            content: content
        );
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
}
