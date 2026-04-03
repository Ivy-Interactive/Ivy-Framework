using System.Drawing;
using Ivy;
using Ivy.Core;
using Ivy.Hooks;
using Ivy.Tendril.Apps.Plans.Dialogs;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Plans;

public class ContentView(
    PlanFile? selectedPlan,
    List<PlanFile> allPlans,
    IState<PlanFile?> selectedPlanState,
    PlanReaderService planService,
    JobService jobService,
    Action refreshPlans,
    ConfigService config) : ViewBase
{
    private readonly PlanFile? _selectedPlan = selectedPlan;
    private readonly List<PlanFile> _allPlans = allPlans;
    private readonly IState<PlanFile?> _selectedPlanState = selectedPlanState;
    private readonly PlanReaderService _planService = planService;
    private readonly JobService _jobService = jobService;
    private readonly Action _refreshPlans = refreshPlans;
    private readonly ConfigService _config = config;

    public override object? Build()
    {
        var downloadUrl = PlanDownloadHelper.UsePlanDownload(Context, _planService, _selectedPlan);
        var client = UseService<IClientProvider>();
        var copyToClipboard = UseClipboard();
        var updateDialogOpen = UseState(false);
        var deleteDialogOpen = UseState(false);
        var createIssueDialogOpen = UseState(false);
        var openFile = UseState<string?>(null);
        var selectedRepoState = UseState<string?>(null);
        var issueAssigneeState = UseState<string?>(null);
        var issueLabelsState = UseState<string[]>(Array.Empty<string>());
        var issueCommentState = UseState("");

        var updateText = UseState("");
        var isEditing = UseState(false);
        var editContent = UseState("");
        var originalContent = UseState("");
        var isEditingPrev = UseState(false);
        var lastPlanId = UseState(_selectedPlan?.Id ?? -1);

        var selectedPlanRef = UseRef(_selectedPlan);

        UseEffect(() =>
        {
            var plan = selectedPlanRef.Value;
            if (isEditing.Value && !isEditingPrev.Value)
            {
                if (plan != null)
                {
                    var raw = _planService.ReadRawPlan(plan.FolderName);
                    editContent.Set(raw);
                    originalContent.Set(raw);
                }
                else
                {
                    isEditing.Set(false);
                }
            }
            else if (!isEditing.Value && isEditingPrev.Value)
            {
                if (plan != null && editContent.Value != originalContent.Value)
                {
                    _planService.SaveRevision(plan.FolderName, editContent.Value);
                    _refreshPlans();
                }
            }
            isEditingPrev.Set(isEditing.Value);
        }, isEditing);

#pragma warning disable CS8601
        selectedPlanRef.Value = _selectedPlan;
#pragma warning restore CS8601

        if (lastPlanId.Value != (_selectedPlan?.Id ?? -1))
        {
            lastPlanId.Set(_selectedPlan?.Id ?? -1);
            isEditing.Set(false);
        }

        if (_selectedPlan is null)
        {
            return Layout.Vertical().AlignContent(Align.Center).Height(Size.Full())
                | Text.Muted("Select a plan from the sidebar");
        }

        var currentIndex = _allPlans.FindIndex(p => p.FolderName == _selectedPlan.FolderName);

        var header = Layout.Horizontal().Width(Size.Full()).Padding(1).Gap(2)
            | Text.Block($"#{_selectedPlan.Id} {_selectedPlan.Title}").Bold();
        if (_selectedPlan.Status != PlanStatus.Draft)
            header |= new Badge(_selectedPlan.Status.ToString()).Variant(_selectedPlan.Status == PlanStatus.Failed ? BadgeVariant.Destructive : BadgeVariant.Outline);
        header |= new Badge(_selectedPlan.Project).Variant(BadgeVariant.Outline).WithProjectColor(_config, _selectedPlan.Project);
        header |= new Badge(_selectedPlan.Level).Variant(_config.GetBadgeVariant(_selectedPlan.Level));
        header |= Text.Muted($"rev:{_selectedPlan.RevisionCount}");

        if (_selectedPlan.DependsOn.Count > 0)
        {
            var depIds = string.Join(", ", _selectedPlan.DependsOn.Select(d =>
            {
                var name = Path.GetFileName(d);
                var dashIdx = name.IndexOf('-');
                return dashIdx > 0 ? name[..dashIdx] : name;
            }));
            header |= new Badge($"Depends on: {depIds}").Variant(BadgeVariant.Secondary);
        }

        header |= isEditing.ToSwitchInput(Icons.Code);
        header |= new Spacer().Width(Size.Grow());
        header |= Text.Rich()
            .Bold($"{currentIndex + 1}/{_allPlans.Count}", word: true)
            .Muted("plans", word: true);
        header |= new Button("Execute").Icon(Icons.Rocket).Primary().ShortcutKey("e").OnClick(() =>
        {
            _planService.TransitionState(_selectedPlan.FolderName, PlanStatus.Building);
            _jobService.StartJob("ExecutePlan", _selectedPlan.FolderPath);
            _refreshPlans();
        });

        var scrollableContent = Layout.Vertical().Width(Size.Auto().Max(Size.Units(200)));

        if (_selectedPlan.Status == PlanStatus.Failed)
        {
            scrollableContent |= BuildFailureCallout(_selectedPlan);
        }

        if (isEditing.Value)
        {
            scrollableContent |= editContent.ToCodeInput()
                .Language(Languages.Markdown)
                .Width(Size.Full());
        }
        else
        {
            scrollableContent |= new Markdown(MarkdownHelper.AnnotateBrokenFileLinks(_selectedPlan.LatestRevisionContent))
                .DangerouslyAllowLocalFiles()
                .OnLinkClick(url =>
                {
                    if (url.StartsWith("file:///", StringComparison.OrdinalIgnoreCase))
                    {
                        var filePath = url.Substring("file:///".Length);
                        openFile.Set(filePath);
                    }
                });
        }

        var actionBar = Layout.Horizontal().AlignContent(Align.Center).Gap(2).Padding(1)
            | new Button("Update").Icon(Icons.Pencil).Outline().ShortcutKey("u").OnClick(() => updateDialogOpen.Set(true))
            | new Button("Split").Icon(Icons.Scissors).Outline().ShortcutKey("s").OnClick(() =>
            {
                _planService.TransitionState(_selectedPlan.FolderName, PlanStatus.Updating);
                _jobService.StartJob("SplitPlan", _selectedPlan.FolderPath);
                _refreshPlans();
            })
            | new Button("Expand").Icon(Icons.UnfoldVertical).Outline().ShortcutKey("x").OnClick(() =>
            {
                _planService.TransitionState(_selectedPlan.FolderName, PlanStatus.Building);
                var planPath = _selectedPlan.FolderPath;
                _jobService.StartJob("ExpandPlan", planPath);
                _refreshPlans();
            })
            | new Button("Delete").Icon(Icons.Trash).Outline().ShortcutKey("Delete").OnClick(() => deleteDialogOpen.Set(true))
            | new Button("Previous").Icon(Icons.ChevronLeft).Outline().OnClick(() => GoToPrevious()).ShortcutKey("p")
            | new Button("Next").Icon(Icons.ChevronRight, Align.Right).Outline().OnClick(() => GoToNext()).ShortcutKey("n")
            | new Button().Icon(Icons.EllipsisVertical).Ghost().WithDropDown(
                new MenuItem("Create Issue", Icon: Icons.Github, Tag: "CreateIssue").OnSelect(() => createIssueDialogOpen.Set(true)),
                new MenuItem("Download", Icon: Icons.Download, Tag: "Download").OnSelect(() =>
                {
                    var url = downloadUrl.Value;
                    if (!string.IsNullOrEmpty(url)) client.OpenUrl(url);
                }),
                new MenuItem("Copy Path to Clipboard", Icon: Icons.ClipboardCopy, Tag: "CopyPath").OnSelect(() =>
                {
                    copyToClipboard(_selectedPlan.FolderPath);
                    client.Toast("Copied path to clipboard", "Path Copied");
                }),
                new MenuItem("Mark as Completed", Icon: Icons.CircleCheck, Tag: "MarkCompleted").OnSelect(() =>
                {
                    _planService.TransitionState(_selectedPlan.FolderName, PlanStatus.Completed);
                    _refreshPlans();
                }),
                new MenuItem("Open plan.yaml", Icon: Icons.FileText, Tag: "OpenPlanYaml").OnSelect(() =>
                {
                    var yamlPath = System.IO.Path.Combine(_selectedPlan.FolderPath, "plan.yaml");
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = _config.Editor.Command,
                        Arguments = yamlPath,
                        UseShellExecute = true
                    });
                })
            );

        var mainContent = Layout.Vertical()
            | scrollableContent;

        var mainLayout = new HeaderLayout(
            header: header,
            content: new FooterLayout(
                footer: actionBar,
                content: mainContent
            ).Size(Size.Full())
        ).Scroll(Scroll.None).Size(Size.Full()).Key(_selectedPlan.Id);

        var elements = new List<object>
        {
            mainLayout,
            new UpdatePlanDialog(updateDialogOpen, updateText, _selectedPlan, _jobService, _planService, _refreshPlans),
            new DeletePlanDialog(deleteDialogOpen, _selectedPlan, _planService, _refreshPlans),
            new CreateIssueDialog(createIssueDialogOpen, selectedRepoState, issueAssigneeState, issueLabelsState, issueCommentState, _selectedPlan, _jobService)
        };

        if (openFile.Value is { } filePath2)
        {
            var ext = Path.GetExtension(filePath2);
            var imageExts = new[] { ".png", ".jpg", ".jpeg", ".gif", ".svg", ".webp" };
            object sheetContent;
            if (imageExts.Contains(ext, StringComparer.OrdinalIgnoreCase))
            {
                var imageUrl = $"/ivy/local-file?path={Uri.EscapeDataString(filePath2)}";
                sheetContent = new Image(imageUrl) { ObjectFit = ImageFit.Contain, Alt = Path.GetFileName(filePath2) };
            }
            else
            {
                if (File.Exists(filePath2))
                {
                    var fileContent = File.ReadAllText(filePath2);
                    var language = FileApp.GetLanguage(ext);
                    sheetContent = new Markdown($"```{language.ToString().ToLowerInvariant()}\n{fileContent}\n```");
                }
                else
                {
                    var fileName = Path.GetFileName(filePath2);
                    var repoPaths = (_selectedPlan.Repos?.Count ?? 0) > 0
                        ? _selectedPlan.Repos
                        : _config.GetProject(_selectedPlan.Project)?.RepoPaths ?? [];
                    var suggestions = MarkdownHelper.FindFilesInRepos(repoPaths, fileName);
                    var content = suggestions.Count > 0
                        ? $"File not found.\n\nDid you mean:\n{string.Join("\n", suggestions.Select(s => $"- `{s}`"))}"
                        : "File not found.";
                    sheetContent = new Markdown(content);
                }
            }

            var finalContent = File.Exists(filePath2)
                ? (object)new HeaderLayout(
                    header: new Button("Open in VS Code").Icon(Icons.ExternalLink).Outline().OnClick(() =>
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = "code",
                            Arguments = $"\"{filePath2}\"",
                            UseShellExecute = true
                        });
                    }),
                    content: sheetContent
                )
                : sheetContent;

            elements.Add(new Sheet(
                onClose: () => openFile.Set(null),
                content: finalContent,
                title: Path.GetFileName(filePath2)
            ).Width(Size.Half()).Resizable());
        }

        return new Fragment(elements.ToArray());
    }

    internal static object BuildFailureCallout(PlanFile plan)
    {
        var verificationDir = Path.Combine(plan.FolderPath, "verification");
        var failedVerifications = plan.Verifications
            .Where(v => v.Status is "Fail" or "Pending")
            .ToList();

        if (failedVerifications.Count > 0 && Directory.Exists(verificationDir))
        {
            var parts = new List<string>();
            foreach (var v in failedVerifications)
            {
                var reportPath = Path.Combine(verificationDir, $"{v.Name}.md");
                if (!File.Exists(reportPath))
                {
                    parts.Add($"**{v.Name}** — {v.Status}, no report generated");
                    continue;
                }

                var report = File.ReadAllText(reportPath);

                // Extract the Output section content
                var outputMatch = System.Text.RegularExpressions.Regex.Match(
                    report, @"## Output\s*\n([\s\S]*?)(?=\n## |\z)");
                var output = outputMatch.Success
                    ? outputMatch.Groups[1].Value.Trim()
                    : null;

                // Extract Issues Found section
                var issuesMatch = System.Text.RegularExpressions.Regex.Match(
                    report, @"## Issues Found\s*\n([\s\S]*?)(?=\n## |\z)");
                var issues = issuesMatch.Success
                    ? issuesMatch.Groups[1].Value.Trim()
                    : null;

                var detail = output ?? issues ?? "See verification report for details";
                parts.Add($"**{v.Name}** — {detail}");
            }

            return Callout.Destructive(string.Join("\n\n", parts), "Execution Failed");
        }

        // Fall back to last execution log
        var logsDir = Path.Combine(plan.FolderPath, "logs");
        if (Directory.Exists(logsDir))
        {
            var lastLog = Directory.GetFiles(logsDir, "*.md")
                .OrderByDescending(f => f)
                .FirstOrDefault();
            if (lastLog != null)
            {
                var logContent = File.ReadAllText(lastLog);
                var summaryMatch = System.Text.RegularExpressions.Regex.Match(
                    logContent, @"## Summary\s*\n([\s\S]*?)(?=\n## |\z)");
                if (summaryMatch.Success)
                    return Callout.Destructive(summaryMatch.Groups[1].Value.Trim(), "Execution Failed");

                var statusMatch = System.Text.RegularExpressions.Regex.Match(
                    logContent, @"\*\*Status:\*\*\s*(.+)");
                if (statusMatch.Success)
                {
                    var status = statusMatch.Groups[1].Value.Trim();
                    if (status == "Completed")
                        return Callout.Warning("Execution reported as completed but plan is in Failed state. The process may have crashed during state transition.", "State Mismatch");
                    return Callout.Destructive($"Last execution status: {status}", "Execution Failed");
                }
            }
        }

        return Callout.Destructive("No details available. Check the logs folder.", "Execution Failed");
    }

    private void GoToNext()
    {
        if (_allPlans.Count == 0) return;
        var currentIndex = _allPlans.FindIndex(p => p.FolderName == _selectedPlan?.FolderName);
        var nextIndex = (currentIndex + 1) % _allPlans.Count;
        _selectedPlanState.Set(_allPlans[nextIndex]);
    }

    private void GoToPrevious()
    {
        if (_allPlans.Count == 0) return;
        var currentIndex = _allPlans.FindIndex(p => p.FolderName == _selectedPlan?.FolderName);
        var prevIndex = (currentIndex - 1 + _allPlans.Count) % _allPlans.Count;
        _selectedPlanState.Set(_allPlans[prevIndex]);
    }
}
