using Ivy;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps;

public class RecommendationRow
{
    public string Id { get; set; } = "";
    public string Date { get; set; } = "";
    public string PlanId { get; set; } = "";
    public string PlanFolderName { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
}

[App(title: "Recommendations", icon: Icons.Lightbulb, group: new[] { "Tools" }, order: 15)]
public class RecommendationsApp : ViewBase
{
    public override object? Build()
    {
        var planService = UseService<PlanReaderService>();
        var nav = this.UseNavigation();
        var refreshToken = UseRefreshToken();

        UseInterval(() => refreshToken.Refresh(), TimeSpan.FromMinutes(1));

        var recommendations = planService.GetRecommendations();

        var rows = recommendations.Select(r => new RecommendationRow
        {
            Id = $"{r.PlanId}-{r.Title.GetHashCode()}",
            Date = r.Date.ToString("yyyy-MM-dd HH:mm"),
            PlanId = r.PlanId,
            PlanFolderName = r.PlanFolderName,
            Title = r.Title,
            Description = r.Description.Length > 200 ? r.Description.Substring(0, 200) + "..." : r.Description
        }).ToList();

        var dataTable = rows.AsQueryable()
            .ToDataTable(idSelector: r => r.Id)
            .RefreshToken(refreshToken)
            .Width(Size.Full())
            .Height(Size.Full())
            .Header(r => r.Date, "Date")
            .Header(r => r.PlanId, "Plan")
            .Header(r => r.Title, "Title")
            .Header(r => r.Description, "Description")
            .Hidden(r => r.Id)
            .Hidden(r => r.PlanFolderName)
            .Config(c =>
            {
                c.AllowSorting = true;
                c.AllowFiltering = true;
                c.ShowSearch = true;
                c.SelectionMode = SelectionModes.None;
                c.ShowIndexColumn = true;
                c.BatchSize = 50;
                c.EnableCellClickEvents = true;
            })
            .Renderer(r => r.PlanId, new ButtonDisplayRenderer())
            .OnCellClick(e =>
            {
                if (e.Value.ColumnName == "Plan")
                {
                    var planId = e.Value.CellValue?.ToString();
                    if (!string.IsNullOrEmpty(planId))
                    {
                        var row = rows.FirstOrDefault(r => r.PlanId == planId);
                        if (row != null)
                        {
                            var fullPath = Path.Combine(planService.PlansDirectory, row.PlanFolderName);
                            if (Directory.Exists(fullPath))
                                nav.Navigate<PlanViewerApp>(new PlanViewerAppArgs(fullPath));
                        }
                    }
                }
                return ValueTask.CompletedTask;
            });

        return new HeaderLayout(
            header: Text.Block($"All Recommendations ({recommendations.Count})").Bold(),
            content: dataTable
        );
    }
}
