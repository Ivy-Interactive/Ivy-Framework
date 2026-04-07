using System.Diagnostics;
using Ivy.Core;
using Ivy.Tendril.Apps.Plans;
using Ivy.Tendril.Apps.Review.Dialogs;
using Ivy.Tendril.Services;
using Ivy.Widgets.DiffView;

namespace Ivy.Tendril.Apps.Review;

public class ContentView(
    PlanFile? selectedPlan,
    List<PlanFile> allPlans,
    IState<PlanFile?> selectedPlanState,
    IPlanReaderService planService,
    IJobService jobService,
    Action refreshPlans,
    IConfigService config,
    IGitService gitService) : ViewBase
{
    private readonly List<PlanFile> _allPlans = allPlans;
    private readonly IConfigService _config = config;
    private readonly IGitService _gitService = gitService;
    private readonly IJobService _jobService = jobService;
    private readonly IPlanReaderService _planService = planService;
    private readonly Action _refreshPlans = refreshPlans;
    private readonly PlanFile? _selectedPlan = selectedPlan;
    private readonly IState<PlanFile?> _selectedPlanState = selectedPlanState;

    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        var copyToClipboard = UseClipboard();
        var openVerification = UseState<string?>(null);
        var openArtifact = UseState<string?>(null);
        var openFile = UseState<string?>(null);
        var openCommit = UseState<string?>(null);
        var discardDialogOpen = UseState(false);
        var suggestChangesOpen = UseState(false);
        var suggestChangesText = UseState("");
        var customPrOpen = UseState(false);

        var githubService = UseService<IGithubService>();
        var assigneesQuery = UseQuery<string[], string>(
            _selectedPlan?.Project ?? "",
            async (_, ct) =>
            {
                if (_selectedPlan is null) return Array.Empty<string>();
                var repos = _selectedPlan.GetEffectiveRepoPaths(_config);
                var repoPath = repos.FirstOrDefault();
                if (repoPath is null) return Array.Empty<string>();
                var repoConfig = GithubService.GetRepoConfigFromPath(repoPath);
                if (repoConfig is null) return Array.Empty<string>();
                var result = await githubService.GetAssigneesAsync(repoConfig.Owner, repoConfig.Name);
                return result.ToArray();
            },
            initialValue: Array.Empty<string>()
        );
        var selectedTab = UseState(0);

        var verificationReportQuery = UseQuery<string, string>(
            openVerification.Value ?? "",
            async (name, ct) =>
            {
                if (string.IsNullOrEmpty(name)) return "";
                var path = Path.Combine(_selectedPlan!.FolderPath, "verification", $"{name}.md");
                return await Task.Run(() =>
                    File.Exists(path) ? FileHelper.ReadAllText(path) : $"No report found for {name}.", ct);
            },
            initialValue: ""
        );

        var artifactContentQuery = UseQuery<string, string>(
            openArtifact.Value ?? "",
            async (filePath, ct) =>
            {
                if (string.IsNullOrEmpty(filePath)) return "";
                return await Task.Run(() =>
                    File.Exists(filePath) ? FileHelper.ReadAllText(filePath) : "File not found.", ct);
            },
            initialValue: ""
        );

        var planContentQuery = UseQuery<PlanContentData, string>(
            _selectedPlan?.FolderPath ?? "",
            async (folderPath, ct) =>
            {
                return await Task.Run(() =>
                {
                    if (_selectedPlan is null)
                        return new PlanContentData(new List<RecommendationYaml>(), null,
                            new Dictionary<string, List<string>>(), new List<CommitRow>(),
                            new Dictionary<string, bool>(), new List<(string Name, bool ConditionMet)>());

                    // Recommendations
                    var recsPath = Path.Combine(folderPath, "artifacts", "recommendations.yaml");
                    var recs = File.Exists(recsPath)
                        ? YamlHelper.Deserializer.Deserialize<List<RecommendationYaml>>(
                            FileHelper.ReadAllText(recsPath)) ?? new List<RecommendationYaml>()
                        : new List<RecommendationYaml>();

                    // Summary
                    var summPath = Path.Combine(folderPath, "artifacts", "summary.md");
                    var summaryMd = File.Exists(summPath) ? FileHelper.ReadAllText(summPath) : null;

                    // Artifacts
                    var artifacts = GetArtifacts(folderPath);

                    // Commit rows
                    var repoPaths = _selectedPlan!.GetEffectiveRepoPaths(_config);
                    var commitRows = _selectedPlan.Commits.Select(commit =>
                    {
                        var title = repoPaths
                            .Select(repo => _gitService.GetCommitTitle(repo, commit))
                            .FirstOrDefault(t => t != null) ?? "";
                        var shortHash = commit.Length > 7 ? commit[..7] : commit;
                        return new CommitRow(commit, shortHash, title);
                    }).ToList();

                    // Verification report existence
                    var verReports = _selectedPlan.Verifications.ToDictionary(
                        v => v.Name,
                        v => File.Exists(Path.Combine(folderPath, "verification", $"{v.Name}.md")));

                    // Review action conditions
                    var projectConfig = _config.GetProject(_selectedPlan.Project);
                    var reviewActions = projectConfig?.ReviewActions ?? new List<ReviewActionConfig>();
                    var actionStates = reviewActions.Select(action =>
                    {
                        if (string.IsNullOrEmpty(action.Condition)) return (action.Name, true);
                        try
                        {
                            var psi = new ProcessStartInfo
                            {
                                FileName = "pwsh",
                                Arguments =
                                    $"-NoProfile -Command \"if ({action.Condition}) {{ exit 0 }} else {{ exit 1 }}\"",
                                WorkingDirectory = folderPath,
                                RedirectStandardOutput = true,
                                RedirectStandardError = true,
                                UseShellExecute = false,
                                CreateNoWindow = true
                            };
                            var proc = Process.Start(psi);
                            proc?.WaitForExit(5000);
                            return (action.Name, proc?.ExitCode == 0);
                        }
                        catch
                        {
                            return (action.Name, false);
                        }
                    }).ToList();

                    return new PlanContentData(recs, summaryMd, artifacts, commitRows, verReports, actionStates);
                }, ct);
            },
            initialValue: new PlanContentData(new List<RecommendationYaml>(), null,
                new Dictionary<string, List<string>>(), new List<CommitRow>(), new Dictionary<string, bool>(),
                new List<(string Name, bool ConditionMet)>())
        );

        UseEffect(() => { selectedTab.Set(0); }, _selectedPlanState);

        if (_selectedPlan is null)
        {
            if (_allPlans.Count == 0)
                return Layout.Vertical().AlignContent(Align.Center).Height(Size.Full()).Gap(2)
                       | new Icon(Icons.Inbox).Large().Color(Colors.Gray)
                       | Text.Muted("No plans to review");

            return Layout.Vertical().AlignContent(Align.Center).Height(Size.Full())
                   | Text.Muted("Select a completed plan to review");
        }

        var currentIndex = _allPlans.FindIndex(p => p.FolderName == _selectedPlan.FolderName);

        // Header
        var header = Layout.Horizontal().Width(Size.Full()).Padding(1).Gap(2)
                     | Text.Block($"#{_selectedPlan.Id} {_selectedPlan.Title}").Bold()
                     | new Spacer().Width(Size.Grow())
                     | Text.Rich()
                         .Bold($"{currentIndex + 1}/{_allPlans.Count}", word: true)
                         .Muted("plans", word: true)
                     | new Button("Make PR").Icon(Icons.GitPullRequest).Primary().OnClick(() =>
                     {
                         _jobService.StartJob("MakePr", _selectedPlan.FolderPath);
                         _planService.TransitionState(_selectedPlan.FolderName, PlanStatus.Building);
                         _refreshPlans();
                     }).ShortcutKey("m").WithConfetti(AnimationTrigger.Click);

        // Content sections
        var content = Layout.Vertical();

        object Cap(object inner)
        {
            return Layout.Vertical().Width(Size.Auto().Max(Size.Units(200))) | inner;
        }

        var planData = planContentQuery.Value;

        // Plan tab content (not dependent on query — uses in-memory data)
        var planTabContent = new Markdown(MarkdownHelper.AnnotateBrokenFileLinks(_selectedPlan.LatestRevisionContent))
            .DangerouslyAllowLocalFiles()
            .OnLinkClick(FileLinkHelper.CreateFileLinkClickHandler(openFile));

        if (planContentQuery.Loading)
        {
            content |= Layout.Vertical().AlignContent(Align.Center).Height(Size.Full())
                       | Text.Muted("Loading...");
        }
        else
        {
            // Summary tab content
            object summaryTabContent;
            if (planData.SummaryMarkdown is { } summaryMd)
            {
                var summaryLayout = Layout.Vertical().Gap(2);
                summaryLayout |= new Markdown(summaryMd).DangerouslyAllowLocalFiles();
                summaryTabContent = summaryLayout;
            }
            else
            {
                summaryTabContent = Text.Muted("No summary available.");
            }

            // Verifications tab content
            var verificationsTable = new Table(
                new TableRow(
                        new TableCell("Status").IsHeader(),
                        new TableCell("Name").IsHeader()
                    )
                    { IsHeader = true }
            );
            foreach (var v in _selectedPlan.Verifications)
            {
                var hasReport = planData.VerificationReports.TryGetValue(v.Name, out var exists) && exists;
                var nameCapture = v.Name;
                var nameCell = hasReport
                    ? new Button(v.Name).Inline().OnClick(() => openVerification.Set(nameCapture))
                    : (object)Text.Block(v.Name);

                verificationsTable |= new TableRow(
                    new TableCell(new Badge(v.Status).Variant(
                        StatusMappings.VerificationStatusBadgeVariants.TryGetValue(v.Status, out var variant)
                            ? variant
                            : BadgeVariant.Outline)),
                    new TableCell(nameCell)
                );
            }

            // Commits tab content
            var commitsTable = new Table(
                new TableRow(
                        new TableCell("Commit").IsHeader(),
                        new TableCell("Message").IsHeader()
                    )
                    { IsHeader = true }
            );
            foreach (var row in planData.CommitRows)
                commitsTable |= new TableRow(
                    new TableCell(new Button(row.ShortHash).Inline().OnClick(() => openCommit.Set(row.Hash))),
                    new TableCell(row.Title)
                );

            // PRs tab content
            object prsContent;
            if (_selectedPlan.Prs.Count > 0)
            {
                var prsTable = new Table(
                    new TableRow(
                            new TableCell("Repository").IsHeader(),
                            new TableCell("PR").IsHeader()
                        )
                        { IsHeader = true }
                );
                foreach (var pr in _selectedPlan.Prs)
                {
                    var prCapture = pr;
                    prsTable |= new TableRow(
                        new TableCell(PullRequestApp.ExtractRepo(pr)),
                        new TableCell(new Button(pr).Link().OnClick(() => client.OpenUrl(prCapture)))
                    );
                }

                prsContent = prsTable;
            }
            else
            {
                prsContent = new Empty();
            }

            // Artifacts tab content
            var artifactsLayout = Layout.Vertical().Gap(2);

            if (planData.Artifacts.TryGetValue("screenshots", out var screenshotFiles))
            {
                var screenshotsLayout = Layout.Horizontal().Gap(2).Wrap();
                foreach (var file in screenshotFiles)
                {
                    var imageUrl = $"/ivy/local-file?path={Uri.EscapeDataString(file)}";
                    screenshotsLayout |= new Image(imageUrl)
                            { ObjectFit = ImageFit.Contain, Alt = Path.GetFileName(file), Overlay = true }
                        .Height(Size.Units(15)).Width(Size.Units(22))
                        .BorderColor(Colors.Neutral)
                        .BorderStyle(BorderStyle.Solid)
                        .BorderThickness(1)
                        .BorderRadius(BorderRadius.Rounded);
                }

                artifactsLayout |= screenshotsLayout;
            }

            var totalArtifacts = (planData.Artifacts.GetValueOrDefault("screenshots")?.Count ?? 0)
                                 + (planData.Artifacts.ContainsKey("sample") ? 1 : 0);

            // Review actions
            var reviewActionStates = planData.ReviewActionStates;
            var projectConfig = _config.GetProject(_selectedPlan.Project);
            var reviewActions = projectConfig?.ReviewActions ?? new List<ReviewActionConfig>();
            if (reviewActions.Count > 0)
            {
                var actionsBar = Layout.Horizontal().Gap(2).Padding(1);
                for (var i = 0; i < reviewActions.Count; i++)
                {
                    var action = reviewActions[i];
                    var conditionMet = i < reviewActionStates.Count && reviewActionStates[i].ConditionMet;

                    var btn = new Button(action.Name).Icon(Icons.Play).Outline();
                    if (!conditionMet)
                    {
                        btn = btn.Disabled();
                    }
                    else
                    {
                        var actionCapture = action;
                        btn = btn.OnClick(() =>
                        {
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = "pwsh",
                                Arguments = $"-NoProfile -Command \"{actionCapture.Action}\"",
                                WorkingDirectory = _selectedPlan.FolderPath,
                                UseShellExecute = true
                            });
                        });
                    }

                    actionsBar |= btn;
                }

                content |= actionsBar;
            }

            // Recommendations tab content
            var recommendationsLayout = Layout.Vertical().Gap(4).Padding(2);
            if (planData.Recommendations.Count == 0)
                recommendationsLayout |= Text.Muted("No recommendations.");
            else
                foreach (var rec in planData.Recommendations)
                {
                    var card = Layout.Vertical().Gap(1)
                               | Text.Block(rec.Title).Bold()
                               | new Markdown(rec.Description).DangerouslyAllowLocalFiles();
                    recommendationsLayout |= card;
                    recommendationsLayout |= new Separator();
                }

            // Build tabs
            var tabs = new TabsLayout(
                e => selectedTab.Set(e.Value),
                null,
                null,
                null,
                selectedTab.Value,
                new Tab("Summary", Cap(summaryTabContent)),
                new Tab("Verifications", Cap(verificationsTable)).Badge(_selectedPlan.Verifications.Count.ToString()),
                new Tab("Commits", Cap(commitsTable)).Badge(_selectedPlan.Commits.Count.ToString()),
                new Tab("PRs", Cap(prsContent)).Badge(_selectedPlan.Prs.Count.ToString()),
                new Tab("Artifacts", Cap(artifactsLayout)).Badge(totalArtifacts.ToString()),
                new Tab("Recommendations", Cap(recommendationsLayout)).Badge(planData.Recommendations.Count.ToString()),
                new Tab("Plan", Cap(planTabContent))
            ).Variant(TabsVariant.Content);

            content |= tabs;
        }

        // Sheet modals (outside TabsLayout so they render as overlays)
        if (openVerification.Value is { } verName)
            content |= new Sheet(
                () => openVerification.Set(null),
                verificationReportQuery.Loading
                    ? Text.Muted("Loading...")
                    : new Markdown(verificationReportQuery.Value).DangerouslyAllowLocalFiles(),
                verName
            ).Width(Size.Half()).Resizable();

        if (openCommit.Value is { } commitHash && _selectedPlan is not null)
        {
            var repoPaths2 = _selectedPlan.GetEffectiveRepoPaths(_config);

            string? commitDiff = null;
            List<(string Status, string FilePath)>? commitFiles = null;
            string? commitTitle = null;
            foreach (var repo in repoPaths2)
            {
                commitTitle = _gitService.GetCommitTitle(repo, commitHash);
                if (commitTitle != null)
                {
                    commitDiff = _gitService.GetCommitDiff(repo, commitHash);
                    commitFiles = _gitService.GetCommitFiles(repo, commitHash);
                    break;
                }
            }

            var shortHash = commitHash.Length > 7 ? commitHash[..7] : commitHash;
            var commitSheetContent = Layout.Vertical().Gap(4).Padding(2);

            if (commitFiles is { Count: > 0 })
            {
                var filesLayout = Layout.Vertical().Gap(1);
                filesLayout |= Text.Block("Changed Files").Bold();
                foreach (var (status, filePath) in commitFiles)
                {
                    var (label, variant) = status switch
                    {
                        "A" => ("Added", BadgeVariant.Success),
                        "D" => ("Deleted", BadgeVariant.Destructive),
                        _ => ("Modified", BadgeVariant.Outline)
                    };
                    filesLayout |= Layout.Horizontal().Gap(2)
                                   | new Badge(label).Variant(variant).Small()
                                   | Text.Block(filePath);
                }

                commitSheetContent |= filesLayout;
            }

            if (!string.IsNullOrWhiteSpace(commitDiff))
            {
                commitSheetContent |= Text.Block("Diff").Bold();
                commitSheetContent |= new DiffView().Diff(commitDiff).Split();
            }

            content |= new Sheet(
                () => openCommit.Set(null),
                commitSheetContent,
                $"Commit {shortHash} — {commitTitle}"
            ).Width(Size.Half()).Resizable();
        }

        if (openArtifact.Value is { } artifactPath)
        {
            var language = FileApp.GetLanguage(Path.GetExtension(artifactPath));
            content |= new Sheet(
                () => openArtifact.Set(null),
                artifactContentQuery.Loading
                    ? Text.Muted("Loading...")
                    : new Markdown($"```{language.ToString().ToLowerInvariant()}\n{artifactContentQuery.Value}\n```"),
                Path.GetFileName(artifactPath)
            ).Width(Size.Half()).Resizable();
        }

        if (_selectedPlan is not null)
        {
            var fileRepoPaths = _selectedPlan.GetEffectiveRepoPaths(_config);
            var fileLinkSheet =
                FileLinkHelper.BuildFileLinkSheet(openFile.Value, () => openFile.Set(null), fileRepoPaths);
            if (fileLinkSheet != null) content |= fileLinkSheet;
        }

        // Dialogs
        content |= new SuggestChangesDialog(suggestChangesOpen, suggestChangesText, _selectedPlan, _jobService,
            _planService, _refreshPlans);
        content |= new CustomPrDialog(customPrOpen, _selectedPlan, _jobService, _planService, _refreshPlans,
            assigneesQuery);

        // Discard confirmation dialog
        content |= new DiscardPlanDialog(discardDialogOpen, _selectedPlan, _planService, _refreshPlans);

        // Action bar
        var actionBar = Layout.Horizontal().AlignContent(Align.Center).Gap(2).Padding(1)
                        | new Button("Suggest Changes").Icon(Icons.MessageSquare).Outline().OnClick(() =>
                        {
                            suggestChangesOpen.Set(true);
                        }).ShortcutKey("d")
                        | new Button("Discard").Icon(Icons.Trash).Outline().OnClick(() =>
                        {
                            discardDialogOpen.Set(true);
                        })
                        | new Button("Previous").Icon(Icons.ChevronLeft).Outline().OnClick(() => GoToPrevious())
                            .ShortcutKey("p")
                        | new Button("Next").Icon(Icons.ChevronRight, Align.Right).Outline().OnClick(() => GoToNext())
                            .ShortcutKey("n")
                        | new Button().Icon(Icons.EllipsisVertical).Ghost().WithDropDown(
                            new MenuItem("Custom PR", Icon: Icons.GitPullRequest, Tag: "CustomPR").OnSelect(() =>
                            {
                                customPrOpen.Set(true);
                            }),
                            new MenuItem("Set Completed", Icon: Icons.CircleCheck, Tag: "SetCompleted").OnSelect(() =>
                            {
                                _planService.TransitionState(_selectedPlan.FolderName, PlanStatus.Completed);
                                _refreshPlans();
                            }),
                            new MenuItem("Open in File Manager", Icon: Icons.FolderOpen, Tag: "OpenInExplorer")
                                .OnSelect(() => { PlatformHelper.OpenInFileManager(_selectedPlan.FolderPath); }),
                            new MenuItem("Open in Terminal", Icon: Icons.Terminal, Tag: "OpenInTerminal").OnSelect(() =>
                            {
                                PlatformHelper.OpenInTerminal(_selectedPlan.FolderPath);
                            }),
                            new MenuItem("Copy Path to Clipboard", Icon: Icons.ClipboardCopy, Tag: "CopyPath")
                                .OnSelect(() =>
                                {
                                    copyToClipboard(_selectedPlan.FolderPath);
                                    client.Toast("Copied path to clipboard", "Path Copied");
                                }),
                            new MenuItem($"Open in {_config.Editor.Label}", Icon: Icons.Code, Tag: "OpenInEditor")
                                .OnSelect(() =>
                                {
                                    Process.Start(new ProcessStartInfo
                                    {
                                        FileName = _config.Editor.Command,
                                        Arguments = $"\"{_selectedPlan.FolderPath}\"",
                                        UseShellExecute = true
                                    });
                                }),
                            new MenuItem("Open plan.yaml", Icon: Icons.FileText, Tag: "OpenPlanYaml").OnSelect(() =>
                            {
                                var yamlPath = Path.Combine(_selectedPlan.FolderPath, "plan.yaml");
                                Process.Start(new ProcessStartInfo
                                {
                                    FileName = _config.Editor.Command,
                                    Arguments = yamlPath,
                                    UseShellExecute = true
                                });
                            })
                        );

        return new HeaderLayout(
            header,
            new FooterLayout(
                actionBar,
                content
            ).Size(Size.Full())
        ).Scroll(Scroll.None).Size(Size.Full()).Key(_selectedPlan.Id);
    }

    private static Dictionary<string, List<string>> GetArtifacts(string folderPath)
    {
        var artifactsDir = Path.Combine(folderPath, "artifacts");
        var result = new Dictionary<string, List<string>>();
        if (!Directory.Exists(artifactsDir)) return result;

        foreach (var subDir in Directory.GetDirectories(artifactsDir))
        {
            var category = Path.GetFileName(subDir);
            var files = Directory.GetFiles(subDir, "*", SearchOption.AllDirectories).ToList();
            if (files.Count > 0)
                result[category] = files;
        }

        var rootFiles = Directory.GetFiles(artifactsDir).ToList();
        if (rootFiles.Count > 0)
            result["other"] = rootFiles;

        return result;
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

    private record CommitRow(string Hash, string ShortHash, string Title);

    private record PlanContentData(
        List<RecommendationYaml> Recommendations,
        string? SummaryMarkdown,
        Dictionary<string, List<string>> Artifacts,
        List<CommitRow> CommitRows,
        Dictionary<string, bool> VerificationReports,
        List<(string Name, bool ConditionMet)> ReviewActionStates);
}