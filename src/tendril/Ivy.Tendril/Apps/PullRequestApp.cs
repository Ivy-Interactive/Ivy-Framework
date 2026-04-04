using Ivy;
using Ivy.Tendril.Apps.PullRequest;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps;

[App(title: "Pull Requests", icon: Icons.GitPullRequest, group: new[] { "Tools" }, order: 27)]
public class PullRequestApp : ViewBase
{
    public override object? Build()
    {
        var planService = UseService<PlanReaderService>();
        var refreshToken = UseRefreshToken();
        var nav = this.UseNavigation();
        var showPlan = UseState<string?>(null);

        var plans = planService.GetPlans()
            .Where(p => p.Prs.Count > 0)
            .OrderByDescending(p => p.Id)
            .ToList();

        var rows = plans.SelectMany(plan => plan.Prs.Select((pr, i) => new PrRow
        {
            Id = $"{plan.Id}-{i}",
            PlanId = $"{plan.Id:D5}",
            Repository = ExtractRepo(pr),
            Pr = pr,
            Plan = $"#{plan.Id:D5} {plan.Title}",
            PlanFolderPath = plan.FolderPath
        })).ToList();

        var dataTable = rows.AsQueryable()
            .ToDataTable(idSelector: t => t.Id)
            .RefreshToken(refreshToken)
            .Width(Size.Full())
            .Height(Size.Full())
            .Header(t => t.PlanId, "Plan ID")
            .Header(t => t.Repository, "Repository")
            .Header(t => t.Pr, "PR")
            .Header(t => t.Plan, "Plan")
            .Renderer(t => t.PlanId, new ButtonDisplayRenderer())
            .Renderer(t => t.Pr, new LinkDisplayRenderer())
            .SortDirection(t => t.PlanId, SortDirection.Descending)
            .Hidden(t => t.Id)
            .Hidden(t => t.PlanFolderPath)
            .Config(c =>
            {
                c.AllowSorting = true;
                c.AllowFiltering = true;
                c.ShowSearch = true;
                c.SelectionMode = SelectionModes.None;
                c.ShowIndexColumn = false;
                c.BatchSize = 50;
                c.EnableCellClickEvents = true;
            })
            .OnCellClick(e =>
            {
                if (e.Value.ColumnName == "PlanId")
                {
                    var planId = e.Value.CellValue?.ToString();
                    var row = rows.FirstOrDefault(r => r.PlanId == planId);
                    if (row != null && !string.IsNullOrEmpty(row.PlanFolderPath) && Directory.Exists(row.PlanFolderPath))
                        showPlan.Set(row.PlanFolderPath);
                }
                return ValueTask.CompletedTask;
            })
            .RowActions(
                new MenuItem(Label: "View Plan", Icon: Icons.FileText, Tag: "view-plan").Tooltip("Open the associated plan"),
                new MenuItem(Label: "Open PR", Icon: Icons.ExternalLink, Tag: "open-pr").Tooltip("Open the pull request in browser")
            )
            .OnRowAction(e =>
            {
                var tag = e.Value.Tag?.ToString();
                var id = e.Value.Id?.ToString();
                var row = rows.FirstOrDefault(r => r.Id == id);

                if (row != null)
                {
                    if (tag == "view-plan")
                    {
                        if (!string.IsNullOrEmpty(row.PlanFolderPath) && Directory.Exists(row.PlanFolderPath))
                            showPlan.Set(row.PlanFolderPath);
                    }
                    else if (tag == "open-pr")
                    {
                        nav.Navigate(row.Pr);
                    }
                }
                return ValueTask.CompletedTask;
            });

        if (showPlan.Value is { } planPath)
        {
            var folderName = Path.GetFileName(planPath);
            var content = planService.ReadLatestRevision(folderName);
            var plan = planService.GetPlanByFolder(planPath);

            var sheetContent = string.IsNullOrEmpty(content)
                ? Text.P("Plan not found or empty.")
                : (object)new Markdown(MarkdownHelper.AnnotateBrokenFileLinks(content))
                    .DangerouslyAllowLocalFiles();

            return Layout.Vertical().Height(Size.Full()) | new Fragment(
                dataTable,
                new Sheet(
                    onClose: () => showPlan.Set(null),
                    content: sheetContent,
                    title: plan?.Title ?? folderName
                ).Width(Size.Half()).Resizable()
            );
        }

        return Layout.Vertical().Height(Size.Full()) | dataTable;
    }

    /// <summary>
    /// Extracts "owner/repo" from a GitHub PR URL.
    /// E.g. "https://github.com/owner/repo/pull/123" -> "owner/repo"
    /// </summary>
    internal static string ExtractRepo(string prUrl)
    {
        try
        {
            var uri = new Uri(prUrl);
            var segments = uri.AbsolutePath.Trim('/').Split('/');
            if (segments.Length >= 2)
                return $"{segments[0]}/{segments[1]}";
        }
        catch { }
        return prUrl;
    }
}
