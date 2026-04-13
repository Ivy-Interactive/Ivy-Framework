using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps;

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


[App(title: "Dashboard", icon: Icons.ChartBar, group: ["Apps"], order: MenuOrder.Dashboard)]
public class DashboardApp : ViewBase
{
    public override object Build()
    {
        var planService = UseService<IPlanReaderService>();
        var configService = UseService<IConfigService>();
        var refreshToken = UseRefreshToken();
        UseInterval(() => { refreshToken.Refresh(); },
            planService.IsDatabaseReady ? TimeSpan.FromSeconds(60) : TimeSpan.FromSeconds(2));

        var selectedProject = UseState<string?>(null);

        if (!planService.IsDatabaseReady)
        {
            return Layout.Vertical().AlignContent(Align.Center).Height(Size.Full()).Gap(2)
                   | Text.Muted("Loading Dashboard Data...");
        }

        var stats = planService.GetDashboardData(selectedProject.Value);

        // Statistics cards
        var statsRow = Layout.Horizontal().Gap(2).Padding(2)
                       | BuildStatCard(stats.TotalCount.ToString(), "Total Plans")
                       | BuildStatCard(stats.DraftCount.ToString(), "Draft")
                       | BuildStatCard(stats.InProgressCount.ToString(), "In Progress")
                       | BuildStatCard(stats.ReviewCount.ToString(), "Ready for Review")
                       | BuildStatCard(stats.CompletedCount.ToString(), "Completed")
                       | BuildStatCard(stats.FailedCount.ToString(), "Failed")
                       | BuildStatCard(FormatHelper.FormatCost(stats.AvgCostPerPlan), "Avg Cost/Plan");

        var today = DateTime.UtcNow.Date;

        var rows = stats.DailyStats.Select(d =>
        {
            var dayLabel = d.Date == today ? "Today"
                : d.Date == today.AddDays(-1) ? "Yesterday"
                : d.Date.ToString("MMM dd");

            var costPerPlan = d is { Completed: > 0, Cost: > 0 }
                ? FormatHelper.FormatCost(d.Cost / d.Completed)
                : "";

            return new DashboardDayRow
            {
                Date = dayLabel,
                SortDate = d.Date,
                Created = d.Created,
                Completed = d.Completed,
                PrsMerged = d.PrsMerged,
                Failed = d.Failed,
                Cost = d.Cost > 0 ? FormatHelper.FormatCost(d.Cost) : "",
                CostPerPlan = costPerPlan,
                Tokens = d.Tokens > 0 ? FormatHelper.FormatTokens(d.Tokens) : ""
            };
        }).ToList();

        var colWidth = Size.Fraction(1 / 8f);

        var dataTable = rows.AsQueryable()
            .ToDataTable(t => t.SortDate)
            .RefreshToken(refreshToken)
            .Width(Size.Full())
            .Height(Size.Px(307))
            .Header(t => t.Date, "Date")
            .Header(t => t.Created, "Created")
            .Header(t => t.Completed, "Completed")
            .Header(t => t.PrsMerged, "PRs")
            .Header(t => t.Failed, "Failed")
            .Header(t => t.Cost, "Cost")
            .Header(t => t.CostPerPlan, "Cost/Plan")
            .Header(t => t.Tokens, "Tokens")
            .Hidden(t => t.SortDate)
            .Width(e => e.Date, colWidth)
            .Width(e => e.Created, colWidth)
            .Width(e => e.Completed, colWidth)
            .Width(e => e.PrsMerged, colWidth)
            .Width(e => e.Failed, colWidth)
            .Width(e => e.Cost, colWidth)
            .Width(e => e.CostPerPlan, colWidth)
            .Width(e => e.Tokens, colWidth)
            .Config(c =>
            {
                c.AllowSorting = false;
                c.AllowFiltering = false;
                c.AllowColumnReordering = false;
                c.ShowSearch = false;
                c.SelectionMode = SelectionModes.None;
                c.ShowIndexColumn = false;
                c.BatchSize = 7;
            });

        // Per-project breakdown (always shows all projects)
        var projectData = stats.ProjectCounts;

        var projectProgress = new StackedProgress(
                    projectData.Select(p => new ProgressSegment(
                        p.Count,
                        configService.GetProjectColor(p.Project),
                        p.Project
                    )).ToArray()
                )
                .Selected(selectedProject.Value != null
                    ? projectData.FindIndex(p => p.Project == selectedProject.Value)
                    : null)
                .OnSelect(e =>
                {
                    var clickedProject = projectData[e.Value].Project;
                    selectedProject.Set(selectedProject.Value == clickedProject ? null : clickedProject);
                    return ValueTask.CompletedTask;
                });

        // Hourly cost & tokens combined bar chart
        var hourlyBurn = planService.GetHourlyTokenBurn(projectFilter: selectedProject.Value);
        if (selectedProject.Value == null)
        {
            hourlyBurn = hourlyBurn
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
        }

        const string costMeasureName = "Cost ($)";
        const string tokensMeasureName = "Tokens";

        var combinedChart = hourlyBurn.ToBarChart(
                style: BarChartStyles.Default,
                polish: chart => chart with
                {
                    Bars =
                    [
                        new Bar(costMeasureName).Radius(0).YAxisIndex(0),
                        new Bar(tokensMeasureName).Radius(0).YAxisIndex(1)
                    ],
                    XAxis =
                    [
                        new XAxis().TickFormatter("MM/dd HH:mm", TickFormatterType.Date).MinTickGap(15)
                    ],
                    YAxis =
                    [
                        new YAxis(costMeasureName).TickFormatter("C2", TickFormatterType.Number).Hide(),
                        new YAxis(tokensMeasureName).Orientation(YAxis.Orientations.Right).Hide()
                    ]
                })
            .FillGaps(TimeSpan.FromHours(1))
            .Dimension("Hour", e => e.Hour)
            .Measure(costMeasureName, e => e.Sum(f => (double)f.Cost))
            .Measure(tokensMeasureName, e => e.Sum(f => (double)f.Tokens))
            .Height(Size.Px(350))
            .Width(Size.Full());

        var content = Layout.Vertical().Gap(2)
                      | dataTable
                      | combinedChart;

        var header = Layout.Vertical()
                     | statsRow
                     | projectProgress.Width(Size.Full()).WithLayout().Margin(2);

        return new HeaderLayout(
            header,
            content
        );
    }

    private static object BuildStatCard(string value, string label)
    {
        return Layout.Vertical().Padding(1)
               | Text.Block(value).Bold()
               | Text.Muted(label);
    }
}
