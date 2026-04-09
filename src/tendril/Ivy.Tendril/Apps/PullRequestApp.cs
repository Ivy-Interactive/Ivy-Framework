using Ivy.Tendril.Apps.Plans;
using Ivy.Tendril.Apps.PullRequest;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps;

[App(title: "Pull Requests", icon: Icons.GitPullRequest, group: ["Apps"], order: MenuOrder.PullRequests)]
public class PullRequestApp : ViewBase
{
    public override object Build()
    {
        var planService = UseService<IPlanReaderService>();
        var refreshToken = UseRefreshToken();
        var nav = UseNavigation();
        var showPlan = UseState<string?>(null);
        var openFile = UseState<string?>(null);
        var config = UseService<IConfigService>();
        var githubService = UseService<IGithubService>();
        var statusQuery = UseQuery<Dictionary<string, string>, string>(
            "pr-statuses",
            async (_, ct) =>
            {
                var allPlans = planService.GetPlans()
                    .Where(p => p.Prs.Count > 0)
                    .ToList();

                var keys = allPlans
                    .SelectMany(p => p.Prs.Where(IsValidUrl))
                    .Select(ExtractRepo)
                    .Distinct()
                    .ToList();

                var tasks = keys.Select(async repoKey =>
                {
                    var parts = repoKey.Split('/');
                    if (parts.Length != 2) return new Dictionary<string, string>();
                    return await githubService.GetPrStatusesAsync(parts[0], parts[1]);
                }).ToList();

                var results = await Task.WhenAll(tasks);
                var allStatuses = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var statuses in results)
                    foreach (var kvp in statuses)
                        allStatuses[kvp.Key] = kvp.Value;

                return allStatuses;
            },
            initialValue: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        );

        var plans = planService.GetPlans()
            .Where(p => p.Prs.Count > 0)
            .OrderByDescending(p => p.Id)
            .ToList();

        var prStatuses = statusQuery.Value ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var rows = plans.SelectMany(plan =>
        {
            var costValue = planService.GetPlanTotalCost(plan.FolderPath);
            var cost = costValue > 0 ? $"${costValue:F2}" : "";
            var tokenValue = planService.GetPlanTotalTokens(plan.FolderPath);
            var tokens = tokenValue > 0 ? FormatHelper.FormatTokens(tokenValue) : "";
            return plan.Prs.Where(IsValidUrl).Select((pr, i) => new PrRow
            {
                Id = $"{plan.Id}-{i}",
                PlanId = $"{plan.Id:D5}",
                Repository = ExtractRepo(pr),
                Status = prStatuses.GetValueOrDefault(pr, ""),
                Pr = pr,
                Plan = $"#{plan.Id:D5} {plan.Title}",
                Cost = cost,
                Tokens = tokens,
                PlanFolderPath = plan.FolderPath
            });
        }).ToList();

        var dataTable = rows.AsQueryable()
            .ToDataTable(t => t.Id)
            .RefreshToken(refreshToken)
            .Width(Size.Full())
            .Height(Size.Full())
            .Header(t => t.Repository, "Repository")
            .Header(t => t.Status, "Status")
            .Header(t => t.Cost, "Cost")
            .Header(t => t.Tokens, "Tokens")
            .Header(t => t.Pr, "PR")
            .Header(t => t.Plan, "Plan")
            .Width(t => t.Repository, Size.Fraction(1 / 3f))
            .Width(t => t.Status, Size.Px(90))
            .Width(t => t.Pr, Size.Fraction(1 / 3f))
            .Width(t => t.Plan, Size.Fraction(1 / 3f))
            .Width(t => t.Cost, Size.Px(90))
            .Width(t => t.Tokens, Size.Px(90))
            .Renderer(t => t.Status, new LabelsDisplayRenderer
            {
                BadgeColorMapping = new Dictionary<string, string>
                {
                    ["Open"] = nameof(Colors.Green),
                    ["Merged"] = nameof(Colors.Purple),
                    ["Closed"] = nameof(Colors.Zinc)
                }
            })
            .Renderer(t => t.Plan, new LinkDisplayRenderer())
            .Renderer(t => t.Pr, new LinkDisplayRenderer())
            .SortDirection(t => t.PlanId, SortDirection.Descending)
            .Hidden(t => t.Id)
            .Hidden(t => t.PlanId)
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
                if (e.Value.ColumnName == "Plan")
                {
                    var row = rows.ElementAtOrDefault(e.Value.RowIndex);
                    if (row != null && !string.IsNullOrEmpty(row.PlanFolderPath) &&
                        Directory.Exists(row.PlanFolderPath))
                        showPlan.Set(row.PlanFolderPath);
                }

                return ValueTask.CompletedTask;
            })
            .RowActions(
                new MenuItem("View Plan", Icon: Icons.FileText, Tag: "view-plan").Tooltip("Open the associated plan"),
                new MenuItem("Open PR", Icon: Icons.ExternalLink, Tag: "open-pr").Tooltip(
                    "Open the pull request in browser")
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
                    .DangerouslyAllowLocalFiles()
                    .OnLinkClick(FileLinkHelper.CreateFileLinkClickHandler(openFile));

            var repoPaths = plan?.GetEffectiveRepoPaths(config) ?? [];
            var fileLinkSheet = FileLinkHelper.BuildFileLinkSheet(
                openFile.Value, () => openFile.Set(null), repoPaths);

            var planSheet = new Sheet(
                () => showPlan.Set(null),
                sheetContent,
                plan?.Title ?? folderName
            ).Width(Size.Half()).Resizable();

            if (fileLinkSheet is not null)
                return Layout.Vertical().Height(Size.Full()) | new Fragment(dataTable, planSheet, fileLinkSheet);

            return Layout.Vertical().Height(Size.Full()) | new Fragment(dataTable, planSheet);
        }

        return Layout.Vertical().Height(Size.Full()) | dataTable;
    }

    /// <summary>
    ///     Extracts "owner/repo" from a GitHub PR URL.
    ///     E.g. "https://github.com/owner/repo/pull/123" -> "owner/repo"
    /// </summary>
    internal static bool IsValidUrl(string value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
        (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

    internal static string ExtractRepo(string prUrl)
    {
        try
        {
            var uri = new Uri(prUrl);
            var segments = uri.AbsolutePath.Trim('/').Split('/');
            if (segments.Length >= 2)
                return $"{segments[0]}/{segments[1]}";
        }
        catch (UriFormatException ex)
        {
            Console.Error.WriteLine($"Failed to parse PR URL '{prUrl}': {ex.Message}");
        }

        return prUrl;
    }
}
