using Ivy.Core;
using Ivy.Tendril.Apps.Plans;
using Ivy.Tendril.Apps.Review.Dialogs;
using Ivy.Tendril.Services;
using Ivy.Widgets.DiffView;
using Microsoft.Extensions.Logging;

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

    public override object Build()
    {
        var client = UseService<IClientProvider>();
        var logger = UseService<ILogger<ContentView>>();
        var copyToClipboard = UseClipboard();
        var openVerification = UseState<string?>(null);
        var openArtifact = UseState<string?>(null);
        var openFile = UseState<string?>(null);
        var openCommit = UseState<string?>(null);
        var discardDialogOpen = UseState(false);
        var suggestChangesOpen = UseState(false);
        var suggestChangesText = UseState("");
        var customPrOpen = UseState(false);
        var rerunDialogOpen = UseState(false);

        var githubService = UseService<IGithubService>();
        var assigneesError = UseState<string?>(null);
        var assigneesQuery = UseQuery<string[], string>(
            _selectedPlan?.Project ?? "",
            async (_, _) =>
            {
                if (_selectedPlan is null)
                {
                    assigneesError.Set(null);
                    return Array.Empty<string>();
                }
                var repos = _selectedPlan.GetEffectiveRepoPaths(_config);
                var repoPath = repos.FirstOrDefault();
                if (repoPath is null)
                {
                    assigneesError.Set(null);
                    return Array.Empty<string>();
                }
                var repoConfig = GithubService.GetRepoConfigFromPath(repoPath);
                if (repoConfig is null)
                {
                    assigneesError.Set(null);
                    return Array.Empty<string>();
                }
                var (assignees, error) = await githubService.GetAssigneesAsync(repoConfig.Owner, repoConfig.Name);
                assigneesError.Set(error);
                return assignees.ToArray();
            },
            initialValue: Array.Empty<string>()
        );
        var selectedTab = UseState(0);

        var verificationReportQuery = UseQuery<string, string>(
            openVerification.Value ?? "",
            async (name, ct) =>
            {
                if (string.IsNullOrEmpty(name) || _selectedPlan is null) return "";
                var verificationDir = Path.GetFullPath(Path.Combine(_selectedPlan.FolderPath, "verification"));
                var resolvedPath = Path.GetFullPath(Path.Combine(verificationDir, $"{name}.md"));
                if (!resolvedPath.StartsWith(verificationDir, StringComparison.OrdinalIgnoreCase))
                    return "Access denied: file is outside the verification folder.";
                return await Task.Run(() =>
                    File.Exists(resolvedPath) ? FileHelper.ReadAllText(resolvedPath) : $"No report found for {name}.", ct);
            },
            initialValue: ""
        );

        var artifactContentQuery = UseQuery<string, string>(
            openArtifact.Value ?? "",
            async (filePath, ct) =>
            {
                if (string.IsNullOrEmpty(filePath)) return "";
                if (_selectedPlan is null) return "";
                var artifactsDir = Path.GetFullPath(Path.Combine(_selectedPlan.FolderPath, "artifacts"));
                var resolvedPath = Path.GetFullPath(filePath);
                if (!resolvedPath.StartsWith(artifactsDir, StringComparison.OrdinalIgnoreCase))
                    return "Access denied: file is outside the artifacts folder.";
                return await Task.Run(() =>
                    File.Exists(resolvedPath) ? FileHelper.ReadAllText(resolvedPath) : "File not found.", ct);
            },
            initialValue: ""
        );

        var commitQuery = UseQuery<PlanContentHelpers.CommitDetailData?, string>(
            openCommit.Value ?? "",
            async (hash, ct) =>
            {
                if (string.IsNullOrEmpty(hash)) return null;
                if (_selectedPlan is null) return null;
                var repoPaths2 = _selectedPlan.GetEffectiveRepoPaths(_config);
                return await Task.Run(() =>
                {
                    foreach (var repo in repoPaths2)
                    {
                        var title = _gitService.GetCommitTitle(repo, hash);
                        if (title != null)
                        {
                            var diff = _gitService.GetCommitDiff(repo, hash);
                            var files = _gitService.GetCommitFiles(repo, hash);
                            return new PlanContentHelpers.CommitDetailData(title, diff, files);
                        }
                    }
                    return null;
                }, ct);
            },
            initialValue: null
        );

        var allChangesQuery = UseQuery<PlanContentHelpers.AllChangesData?, string>(
            _selectedPlan?.FolderPath ?? "",
            async (folderPath, ct) =>
            {
                return await Task.Run(() =>
                {
                    if (_selectedPlan is null) return null;
                    return PlanContentHelpers.GetAllChangesData(_selectedPlan, _config, _gitService);
                }, ct);
            },
            initialValue: null
        );

        var planContentQuery = UseQuery<PlanContentData, string>(
            _selectedPlan?.FolderPath ?? "",
            async (folderPath, ct) =>
            {
                return await Task.Run(() =>
                {
                    if (_selectedPlan is null)
                        return new PlanContentData(new List<RecommendationYaml>(), null,
                            new Dictionary<string, List<string>>(), new List<PlanContentHelpers.CommitRow>(),
                            new Dictionary<string, bool>(), new List<(string Name, bool ConditionMet)>());

                    // Recommendations
                    var recsPath = Path.Combine(folderPath, "artifacts", "recommendations.yaml");
                    List<RecommendationYaml> recs;
                    try
                    {
                        recs = File.Exists(recsPath)
                            ? YamlHelper.Deserializer.Deserialize<List<RecommendationYaml>>(
                                FileHelper.ReadAllText(recsPath)) ?? new List<RecommendationYaml>()
                            : new List<RecommendationYaml>();
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Failed to parse recommendations.yaml for plan {FolderPath}", folderPath);
                        recs = new List<RecommendationYaml>();
                    }

                    // Summary
                    var summPath = Path.Combine(folderPath, "artifacts", "summary.md");
                    var summaryMd = File.Exists(summPath) ? FileHelper.ReadAllText(summPath) : null;

                    // Artifacts
                    var artifacts = PlanContentHelpers.GetArtifacts(folderPath);

                    // Commit rows
                    var commitRows = PlanContentHelpers.BuildCommitRows(_selectedPlan!, _config, _gitService);

                    // Verification report existence
                    var verReports = _selectedPlan.Verifications.ToDictionary(
                        v => v.Name,
                        v => File.Exists(Path.Combine(folderPath, "verification", $"{v.Name}.md")));

                    // Review action conditions
                    var projectConfig = _config.GetProject(_selectedPlan.Project);
                    var reviewActions = projectConfig?.ReviewActions ?? [];
                    var actionStates = new (string Name, bool ConditionMet)[reviewActions.Count];
                    Parallel.For(0, reviewActions.Count, i =>
                    {
                        var action = reviewActions[i];
                        if (string.IsNullOrEmpty(action.Condition))
                        {
                            actionStates[i] = (action.Name, true);
                            return;
                        }
                        actionStates[i] = (action.Name, PlatformHelper.EvaluatePowerShellCondition(action.Condition, folderPath));
                    });

                    return new PlanContentData(recs, summaryMd, artifacts, commitRows, verReports, actionStates.ToList());
                }, ct);
            },
            options: QueryScope.View,
            initialValue: new PlanContentData(new List<RecommendationYaml>(), null,
                new Dictionary<string, List<string>>(), new List<PlanContentHelpers.CommitRow>(), new Dictionary<string, bool>(),
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
                     | Text.Block($"#{_selectedPlan.Id} {_selectedPlan.Title}").Bold().NoWrap().Overflow(Overflow.Ellipsis);

        if (!string.IsNullOrEmpty(_selectedPlan.SourceUrl))
            header |= new Button(_selectedPlan.SourceUrl.Contains("/pull/") ? "PR" : "Issue")
                .Icon(Icons.ExternalLink).Ghost().OnClick(() => client.OpenUrl(_selectedPlan.SourceUrl));

        header |= new Spacer().Width(Size.Grow());

        header |= Text.Rich()
                         .Bold($"{currentIndex + 1}/{_allPlans.Count}", word: true)
                         .Muted("plans", word: true);

        header |= new Button("Make PR").Icon(Icons.GitPullRequest).Primary().OnClick(() =>
        {
            var repoPaths = _selectedPlan.GetEffectiveRepoPaths(_config);
            var project = _config.GetProject(_selectedPlan.Project);
            var allYolo = repoPaths.All(rp =>
                project?.GetRepoRef(rp)?.PrRule == "yolo");

            if (allYolo)
            {
                _jobService.StartJob("MakePr", _selectedPlan.FolderPath);
                _planService.TransitionState(_selectedPlan.FolderName, PlanStatus.Building);
                _refreshPlans();
            }
            else
            {
                customPrOpen.Set(true);
            }
        }).ShortcutKey("m").WithConfetti(AnimationTrigger.Click);

        // Content sections
        var content = Layout.Vertical();

        var planData = planContentQuery.Value;

        // Plan tab content (not dependent on query — uses in-memory data)
        var reviewAnnotated = MarkdownHelper.AnnotateBrokenFileLinks(_selectedPlan.LatestRevisionContent);
        reviewAnnotated = MarkdownHelper.AnnotateBrokenPlanLinks(reviewAnnotated, _planService.PlansDirectory);
        var planTabContent = new Markdown(reviewAnnotated)
            .DangerouslyAllowLocalFiles()
            .OnLinkClick(FileLinkHelper.CreateFileLinkClickHandler(openFile, planId =>
            {
                var planFolder = Directory.GetDirectories(_planService.PlansDirectory, $"{planId:D5}-*")
                    .FirstOrDefault();
                if (planFolder != null)
                {
                    var plan = _planService.GetPlanByFolder(planFolder);
                    if (plan != null)
                        _selectedPlanState.Set(plan);
                }
            }));

        if (planContentQuery.Loading)
        {
            content |= Layout.Vertical().AlignContent(Align.Center).Height(Size.Full())
                       | Text.Muted("Loading...");
        }
        else if (planData is null)
        {
            var errorMsg = planContentQuery.Error is { } err
                ? $"Failed to load plan data: {err.Message}"
                : "Failed to load plan data. Please try refreshing.";
            content |= Layout.Vertical().AlignContent(Align.Center).Height(Size.Full())
                       | Text.Muted(errorMsg);
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

            // Git tab content (combines commits and PRs)
            var gitLayout = Layout.Vertical().Gap(2);

            var problematicCommits = planData.CommitRows
                .Where(r => string.IsNullOrEmpty(r.Title) || r.FileCount == 0)
                .ToList();

            if (problematicCommits.Count > 0)
            {
                var warnings = problematicCommits.Select(r =>
                {
                    if (string.IsNullOrEmpty(r.Title))
                        return $"`{r.ShortHash}` — commit not found or has no message";
                    return $"`{r.ShortHash}` — commit has no file changes";
                });
                gitLayout |= Callout.Warning(
                    string.Join("\n", warnings),
                    "Potentially corrupted commits");
            }

            if (_selectedPlan.Commits.Count > 0)
            {
                gitLayout |= Text.Block("Commits").Bold();
                var commitsTable = new Table(
                    new TableRow(
                            new TableCell("Commit").IsHeader(),
                            new TableCell("Message").IsHeader(),
                            new TableCell("Files").IsHeader()
                        )
                    { IsHeader = true }
                );
                foreach (var row in planData.CommitRows)
                    commitsTable |= new TableRow(
                        new TableCell(new Button(row.ShortHash).Inline().OnClick(() => openCommit.Set(row.Hash))),
                        new TableCell(row.Title),
                        new TableCell(row.FileCount?.ToString() ?? "–")
                    );
                gitLayout |= commitsTable;
            }
            if (_selectedPlan.Prs.Count > 0)
            {
                if (_selectedPlan.Commits.Count > 0)
                    gitLayout |= new Separator();

                gitLayout |= Text.Block("Pull Requests").Bold();
                var prsTable = new Table(
                    new TableRow(
                            new TableCell("Repository").IsHeader(),
                            new TableCell("PR").IsHeader()
                        )
                    { IsHeader = true }
                );
                foreach (var pr in _selectedPlan.Prs.Where(PullRequestApp.IsValidUrl))
                {
                    var prCapture = pr;
                    prsTable |= new TableRow(
                        new TableCell(PullRequestApp.ExtractRepo(pr)),
                        new TableCell(new Button(pr).Link().OnClick(() => client.OpenUrl(prCapture)))
                    );
                }
                gitLayout |= prsTable;
            }

            if (_selectedPlan.Commits.Count == 0 && _selectedPlan.Prs.Count == 0)
            {
                gitLayout |= Text.Muted("No commits or pull requests yet.");
            }

            // Artifacts tab content
            var artifactsLayout = Layout.Vertical().Gap(2);
            artifactsLayout |= PlanContentHelpers.RenderArtifactScreenshots(planData.Artifacts);

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
                            if (!PlatformHelper.RunPowerShellAction(actionCapture.Action, _selectedPlan.FolderPath))
                            {
                                Console.Error.WriteLine($"Failed to run review action '{actionCapture.Name}': pwsh not found");
                            }
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

            // Changes tab content
            object changesTabContent;
            var changesData = allChangesQuery.Value;
            var changesFileCount = 0;
            if (allChangesQuery.Loading)
            {
                changesTabContent = Text.Muted("Loading...");
            }
            else if (changesData is null)
            {
                changesTabContent = Text.Muted("No commits yet.");
            }
            else
            {
                changesFileCount = changesData.Files.Count;
                var changesLayout = Layout.Vertical().Gap(4).Padding(2);

                var statsText =
                    $"{changesData.Files.Count} files changed ({changesData.AddedCount} added, {changesData.ModifiedCount} modified, {changesData.DeletedCount} deleted)";
                changesLayout |= Text.Block(statsText).Bold();

                if (changesData.Files.Count > 0)
                {
                    var filesLayout = Layout.Vertical().Gap(1);
                    foreach (var (status, filePath) in changesData.Files)
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

                    changesLayout |= filesLayout;
                }

                if (!string.IsNullOrWhiteSpace(changesData.Diff))
                {
                    changesLayout |= new DiffView().Diff(changesData.Diff).Split();
                }

                changesTabContent = changesLayout;
            }

            // Build tabs
            var tabs = Layout.Tabs(
                new Tab("Summary", Cap(summaryTabContent)),
                new Tab("Verifications", Cap(verificationsTable)).Badge(_selectedPlan.Verifications.Count.ToString()),
                new Tab("Git", Cap(gitLayout)).Badge((_selectedPlan.Commits.Count + _selectedPlan.Prs.Count).ToString()),
                new Tab("Changes", Cap(changesTabContent)).Badge(changesFileCount > 0 ? changesFileCount.ToString() : ""),
                new Tab("Artifacts", Cap(artifactsLayout)).Badge(totalArtifacts.ToString()),
                new Tab("Recommendations", Cap(recommendationsLayout)).Badge(planData.Recommendations.Count.ToString()),
                new Tab("Plan", Cap(planTabContent))
            ).OnSelect(v => selectedTab.Set(v)).SelectedIndex(selectedTab.Value).Variant(TabsVariant.Content);

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
            content |= PlanContentHelpers.RenderCommitDetailSheet(
                commitQuery.Value,
                commitQuery.Loading || commitQuery.Value is null && !string.IsNullOrEmpty(openCommit.Value),
                commitHash,
                () => openCommit.Set(null));
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
                FileLinkHelper.BuildFileLinkSheet(openFile.Value, () => openFile.Set(null), fileRepoPaths, _config);
            if (fileLinkSheet != null) content |= fileLinkSheet;
        }

        // Dialogs
        content |= new SuggestChangesDialog(suggestChangesOpen, suggestChangesText, _selectedPlan, _jobService,
            _planService, _refreshPlans);
        content |= new CustomPrDialog(customPrOpen, _selectedPlan, _jobService, _planService, _refreshPlans,
            assigneesQuery, assigneesError);

        // Discard confirmation dialog
        content |= new DiscardPlanDialog(discardDialogOpen, _selectedPlan, _planService, _refreshPlans);

        // Rerun dialog
        content |= new RerunDialog(rerunDialogOpen, _selectedPlan, _jobService, _planService, _refreshPlans);

        // Action bar
        var actionBar = Layout.Horizontal().AlignContent(Align.Center).Gap(2).Padding(1)
                        | new Button("Rerun").Icon(Icons.RotateCw).Outline().ShortcutKey("r").OnClick(() =>
                        {
                            rerunDialogOpen.Set(true);
                        })
                        | new Button("Suggest Changes").Icon(Icons.MessageSquare).Outline().OnClick(() =>
                        {
                            suggestChangesOpen.Set(true);
                        }).ShortcutKey("d")
                        | new Button("Discard").Icon(Icons.Trash).Outline().ShortcutKey("Backspace").OnClick(() =>
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
                                .OnSelect(() => { _config.OpenInEditor(_selectedPlan.FolderPath); }),
                            new MenuItem("Open plan.yaml", Icon: Icons.FileText, Tag: "OpenPlanYaml").OnSelect(() =>
                            {
                                var yamlPath = Path.Combine(_selectedPlan.FolderPath, "plan.yaml");
                                _config.OpenInEditor(yamlPath);
                            })
                        );

        return new HeaderLayout(
            header,
            new FooterLayout(
                actionBar,
                content
            ).Size(Size.Full())
        ).Scroll(Scroll.None).Size(Size.Full()).Key(_selectedPlan.Id);

        object Cap(object inner)
        {
            return Layout.Vertical().Width(Size.Auto().Max(Size.Units(200))) | inner;
        }
    }

    internal static bool ValidateArtifactPath(string filePath, string planFolderPath)
    {
        var artifactsDir = Path.GetFullPath(Path.Combine(planFolderPath, "artifacts"));
        var resolvedPath = Path.GetFullPath(filePath);
        return resolvedPath.StartsWith(artifactsDir, StringComparison.OrdinalIgnoreCase);
    }

    internal static bool ValidateVerificationPath(string name, string planFolderPath)
    {
        var verificationDir = Path.GetFullPath(Path.Combine(planFolderPath, "verification"));
        var resolvedPath = Path.GetFullPath(Path.Combine(verificationDir, $"{name}.md"));
        return resolvedPath.StartsWith(verificationDir, StringComparison.OrdinalIgnoreCase);
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

    private record PlanContentData(
        List<RecommendationYaml> Recommendations,
        string? SummaryMarkdown,
        Dictionary<string, List<string>> Artifacts,
        List<PlanContentHelpers.CommitRow> CommitRows,
        Dictionary<string, bool> VerificationReports,
        List<(string Name, bool ConditionMet)> ReviewActionStates);
}
