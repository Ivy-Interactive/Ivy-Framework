using Ivy.Tendril.Apps.Plans;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps;

[App(title: "Dashboard", icon: Icons.ChartBar, group: new[] { "Tools" }, order: MenuOrder.Dashboard)]
public class DashboardApp : ViewBase
{
    public override object? Build()
    {
        var planService = UseService<IPlanReaderService>();
        var configService = UseService<IConfigService>();
        var refreshToken = UseRefreshToken();
        UseInterval(() =>
        {
            refreshToken.Refresh();
        }, TimeSpan.FromSeconds(60));

        var selectedProject = UseState<string?>(null);

        var plans = planService.GetPlans();

        // Filter by selected project
        var filteredPlans = selectedProject.Value != null
            ? plans.Where(p => p.Project == selectedProject.Value).ToList()
            : plans;

        // Statistics cards
        var totalCount = filteredPlans.Count;
        var draftCount = filteredPlans.Count(p => p.Status == PlanStatus.Draft);
        var inProgressCount = filteredPlans.Count(p => p.Status is PlanStatus.Building or PlanStatus.Executing or PlanStatus.Updating);
        var reviewCount = filteredPlans.Count(p => p.Status == PlanStatus.ReadyForReview);
        var completedCount = filteredPlans.Count(p => p.Status == PlanStatus.Completed);
        var failedCount = filteredPlans.Count(p => p.Status == PlanStatus.Failed);

        var completedOrFailedPlans = filteredPlans
            .Where(p => p.Status is PlanStatus.Completed or PlanStatus.Failed or PlanStatus.ReadyForReview)
            .ToList();

        // Pre-compute costs and tokens once per plan to avoid duplicate I/O
        var costCache = completedOrFailedPlans
            .ToDictionary(p => p.FolderPath, p => planService.GetPlanTotalCost(p.FolderPath));
        var tokenCache = completedOrFailedPlans
            .ToDictionary(p => p.FolderPath, p => planService.GetPlanTotalTokens(p.FolderPath));

        var totalCost = costCache.Values.Sum();
        var avgCost = completedOrFailedPlans.Count > 0
            ? totalCost / completedOrFailedPlans.Count
            : 0;

        var statsRow = Layout.Horizontal().Gap(2).Padding(2)
            | BuildStatCard(totalCount.ToString(), "Total Plans")
            | BuildStatCard(draftCount.ToString(), "Draft")
            | BuildStatCard(inProgressCount.ToString(), "In Progress")
            | BuildStatCard(reviewCount.ToString(), "Ready for Review")
            | BuildStatCard(completedCount.ToString(), "Completed")
            | BuildStatCard(failedCount.ToString(), "Failed")
            | BuildStatCard(FormatHelper.FormatCost(avgCost), "Avg Cost/Plan");

        var today = DateTime.UtcNow.Date;
        var days = Enumerable.Range(0, 7).Select(i => today.AddDays(-i)).ToList();

        var rows = days.Select(day =>
        {
            var dayLabel = day == today ? "Today"
                : day == today.AddDays(-1) ? "Yesterday"
                : day.ToString("MMM dd");

            var createdCount = filteredPlans.Count(p => p.Created.Date == day);
            var dayCompletedCount = filteredPlans.Count(p => p.Status == PlanStatus.Completed && p.Updated.Date == day);
            var prsMerged = filteredPlans.Where(p => p.Status == PlanStatus.Completed && p.Updated.Date == day).Sum(p => p.Prs.Count);
            var dayFailedCount = filteredPlans.Count(p => p.Status == PlanStatus.Failed && p.Updated.Date == day);

            var completedOrFailedPlans = filteredPlans
                .Where(p => p.Updated.Date == day && p.Status is PlanStatus.Completed or PlanStatus.Failed or PlanStatus.ReadyForReview)
                .ToList();

            var dayCost = completedOrFailedPlans.Sum(p => costCache.GetValueOrDefault(p.FolderPath, 0m));
            var dayTokens = completedOrFailedPlans.Sum(p => tokenCache.GetValueOrDefault(p.FolderPath, 0));
            var costPerPlan = dayCompletedCount > 0 && dayCost > 0
                ? FormatHelper.FormatCost(dayCost / dayCompletedCount)
                : "";

            return new DashboardDayRow
            {
                Date = dayLabel,
                SortDate = day,
                Created = createdCount,
                Completed = dayCompletedCount,
                PrsMerged = prsMerged,
                Failed = dayFailedCount,
                Cost = dayCost > 0 ? FormatHelper.FormatCost(dayCost) : "",
                CostPerPlan = costPerPlan,
                Tokens = dayTokens > 0 ? FormatHelper.FormatTokens(dayTokens) : ""
            };
        }).ToList();

        var dataTable = rows.AsQueryable()
            .ToDataTable(idSelector: t => t.SortDate)
            .RefreshToken(refreshToken)
            .Width(Size.Full())
            .Height(Size.Px(320))
            .Header(t => t.Date, "Date")
            .Header(t => t.Created, "Created")
            .Header(t => t.Completed, "Completed")
            .Header(t => t.PrsMerged, "PRs / Merged")
            .Header(t => t.Failed, "Failed")
            .Header(t => t.Cost, "Cost")
            .Header(t => t.CostPerPlan, "Cost/Plan")
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
                Label: $"{p.Project} ({p.Count})"
            )).ToArray()
        )
        .Selected(selectedProject.Value != null
            ? Array.FindIndex(projectData, p => p.Project == selectedProject.Value)
            : null)
        .OnSelect(async e =>
        {
            var clickedProject = projectData[e.Value].Project;
            selectedProject.Set(selectedProject.Value == clickedProject ? null : clickedProject);
        });

        // Hourly cost & tokens combined bar chart
        var allHourlyBurn = planService.GetHourlyTokenBurn(days: 7);
        var hourlyBurn = selectedProject.Value != null
            ? allHourlyBurn.Where(h => h.Project == selectedProject.Value).ToList()
            : allHourlyBurn
                .GroupBy(h => h.Hour)
                .Select(g => new HourlyTokenBurn
                {
                    Hour = g.Key,
                    Cost = g.Sum(h => h.Cost),
                    Tokens = g.Sum(h => h.Tokens),
                    Project = ""
                })
                .OrderBy(h => h.Hour)
                .ToList();

        var combinedChart = hourlyBurn.ToBarChart(
                style: BarChartStyles.Default,
                polish: chart => chart with
                {
                    CartesianGrid = null,
                    Bars =
                    [
                        new Bar("Cost ($)").Radius(4).FillOpacity(0.8).YAxisIndex(0),
                        new Bar("Tokens").Radius(4).FillOpacity(0.8).YAxisIndex(1),
                    ],
                    XAxis =
                    [
                        new XAxis().TickFormatter("MM/dd HH").Hide()
                    ],
                    YAxis =
                    [
                        new YAxis("Cost ($)").TickFormatter("C2").Hide(),
                        new YAxis("Tokens").Orientation(YAxis.Orientations.Right).Hide(),
                    ]
                })
            .FillGaps(TimeSpan.FromHours(1))
            .Dimension("Hour", e => e.Hour)
            .Measure("Cost ($)", e => e.Sum(f => (double)f.Cost))
            .Measure("Tokens", e => e.Sum(f => (double)f.Tokens))
            .Height(Size.Px(350))
            .Width(Size.Full());

        var content = Layout.Vertical().Gap(2)
            | dataTable
            | new Card(
                header: new Box(projectProgress).Padding(2),
                content: combinedChart
            );

        return new HeaderLayout(
            header: statsRow,
            content: content
        );
    }

    private static object BuildStatCard(string value, string label)
    {
        return Layout.Vertical().Padding(1)
            | Text.Block(value).Bold()
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
    public string CostPerPlan { get; set; } = "";
    public string Tokens { get; set; } = "";
}
