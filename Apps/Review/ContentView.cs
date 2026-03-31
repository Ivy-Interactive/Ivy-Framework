using Ivy;
using Ivy.Core;
using Ivy.Hooks;
using Ivy.Tendril.Apps.Plans;
using Ivy.Tendril.Services;
using Ivy.Widgets.DiffView;

namespace Ivy.Tendril.Apps.Review;

public class ContentView(
    PlanFile? selectedPlan,
    List<PlanFile> allPlans,
    IState<PlanFile?> selectedPlanState,
    PlanReaderService planService,
    JobService jobService,
    Action refreshPlans,
    ConfigService config,
    GitService gitService) : ViewBase
{
    private readonly PlanFile? _selectedPlan = selectedPlan;
    private readonly List<PlanFile> _allPlans = allPlans;
    private readonly IState<PlanFile?> _selectedPlanState = selectedPlanState;
    private readonly PlanReaderService _planService = planService;
    private readonly JobService _jobService = jobService;
    private readonly Action _refreshPlans = refreshPlans;
    private readonly ConfigService _config = config;
    private readonly GitService _gitService = gitService;

    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        var copyToClipboard = UseClipboard();
        var openVerification = UseState<string?>(null);
        var openArtifact = UseState<string?>(null);
        var openFile = UseState<string?>(null);
        var openCommit = UseState<string?>(null);
        var showPlan = UseState(false);

        if (_selectedPlan is null)
        {
            return Layout.Vertical().Align(Align.Center).Height(Size.Full())
                | Text.Muted("Select a completed plan to review");
        }

        var currentIndex = _allPlans.FindIndex(p => p.FolderName == _selectedPlan.FolderName);

        // Header
        var statusVariant = _selectedPlan.Status == PlanStatus.ReadyForReview ? BadgeVariant.Success : BadgeVariant.Destructive;
        var header = Layout.Horizontal().Width(Size.Full()).Padding(1).Gap(2)
            | Text.Block($"#{_selectedPlan.Id} {_selectedPlan.Title}").Bold()
            | new Badge(_selectedPlan.Status.ToString()).Variant(statusVariant)
            | new Badge(_selectedPlan.Project).Variant(BadgeVariant.Outline)
            | new Badge(_selectedPlan.Level).Variant(_config.GetBadgeVariant(_selectedPlan.Level))
            | new Spacer().Width(Size.Grow())
            | Text.Rich()
                .Bold($"{currentIndex + 1}/{_allPlans.Count}", word: true)
                .Muted("plans", word: true)
            | new Button("Make PR").Icon(Icons.GitPullRequest).Primary().OnClick(() =>
            {
                _jobService.StartJob("MakePr", _selectedPlan.FolderPath);
                _planService.TransitionState(_selectedPlan.FolderName, PlanStatus.Building);
                _refreshPlans();
            });

        // Content sections
        var content = Layout.Vertical().Width(Size.Auto().Max(Size.Units(200))).Gap(4);

        // PRs section
        if (_selectedPlan.Prs.Count > 0)
        {
            var prsLayout = Layout.Vertical().Gap(1);
            foreach (var pr in _selectedPlan.Prs)
            {
                var prCapture = pr;
                prsLayout |= new Button(pr).Link().OnClick(() => client.OpenUrl(prCapture));
            }
            content |= new Expandable(
                header: $"Pull Requests ({_selectedPlan.Prs.Count})",
                content: prsLayout
            ).Open(false);
        }

        // Verifications section
        if (_selectedPlan.Verifications.Count > 0)
        {
            var verificationsTable = new Table(
                new TableRow(
                    new TableCell("Status").IsHeader(),
                    new TableCell("Name").IsHeader()
                )
                { IsHeader = true }
            );
            foreach (var v in _selectedPlan.Verifications)
            {
                var verificationPath = Path.Combine(_selectedPlan.FolderPath, "verification", $"{v.Name}.md");
                var hasReport = File.Exists(verificationPath);
                var nameCapture = v.Name;
                object nameCell = hasReport
                    ? new Button(v.Name).Inline().OnClick(() => openVerification.Set(nameCapture))
                    : (object)Text.Block(v.Name);

                verificationsTable |= new TableRow(
                    new TableCell(new Badge(v.Status).Variant(
                        v.Status == "Pass" ? BadgeVariant.Success
                        : v.Status == "Fail" ? BadgeVariant.Destructive
                        : BadgeVariant.Outline)),
                    new TableCell(nameCell)
                );
            }
            content |= new Expandable(
                header: $"Verifications ({_selectedPlan.Verifications.Count})",
                content: verificationsTable
            ).Open(false);

            if (openVerification.Value is { } verName)
            {
                var reportPath = Path.Combine(_selectedPlan.FolderPath, "verification", $"{verName}.md");
                var reportContent = File.Exists(reportPath)
                    ? File.ReadAllText(reportPath)
                    : $"No report found for {verName}.";
                content |= new Sheet(
                    onClose: () => openVerification.Set(null),
                    content: new Markdown(reportContent).DangerouslyAllowLocalFiles(),
                    title: verName
                ).Width(Size.Half());
            }
        }

        // Commits section
        if (_selectedPlan.Commits.Count > 0)
        {
            var repoPaths = _selectedPlan.Repos.Count > 0
                ? _selectedPlan.Repos
                : _config.GetProject(_selectedPlan.Project)?.RepoPaths ?? [];
            var commitRows = _selectedPlan.Commits.Select(commit =>
            {
                var title = repoPaths
                    .Select(repo => _gitService.GetCommitTitle(repo, commit))
                    .FirstOrDefault(t => t != null) ?? "";
                var shortHash = commit.Length > 7 ? commit[..7] : commit;
                return new CommitRow(commit, shortHash, title);
            }).ToList();

            var commitsTable = new Table(
                new TableRow(
                    new TableCell("Commit").IsHeader(),
                    new TableCell("Message").IsHeader()
                )
                { IsHeader = true }
            );
            foreach (var row in commitRows)
            {
                commitsTable |= new TableRow(
                    new TableCell(new Button(row.ShortHash).Inline().OnClick(() => openCommit.Set(row.Hash))),
                    new TableCell(row.Title)
                );
            }
            content |= new Expandable(
                header: $"Commits ({_selectedPlan.Commits.Count})",
                content: commitsTable
            ).Open(false);

            if (openCommit.Value is { } commitHash)
            {
                var repoPaths2 = _selectedPlan.Repos.Count > 0
                    ? _selectedPlan.Repos
                    : _config.GetProject(_selectedPlan.Project)?.RepoPaths ?? [];

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
                var sheetContent = Layout.Vertical().Gap(4).Padding(2);
                sheetContent |= Layout.Horizontal().Gap(2)
                    | new Badge(shortHash).Variant(BadgeVariant.Outline)
                    | Text.Block(commitTitle ?? "Commit not found").Bold();

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
                    sheetContent |= filesLayout;
                }

                if (!string.IsNullOrWhiteSpace(commitDiff))
                {
                    sheetContent |= Text.Block("Diff").Bold();
                    sheetContent |= new DiffView().Diff(commitDiff).Split();
                }

                content |= new Sheet(
                    onClose: () => openCommit.Set(null),
                    content: sheetContent,
                    title: $"Commit {shortHash}"
                ).Width(Size.Half());
            }
        }

        // Artifacts section
        var artifacts = GetArtifacts(_selectedPlan.FolderPath);
        if (artifacts.Count > 0)
        {
            var artifactsLayout = Layout.Vertical().Gap(2);

            // Run Sample button
            if (artifacts.TryGetValue("sample", out var sampleFiles))
            {
                var sampleDir = Path.Combine(_selectedPlan.FolderPath, "artifacts", "sample");
                var csproj = Directory.GetFiles(sampleDir, "*.csproj", SearchOption.AllDirectories).FirstOrDefault();
                if (csproj != null)
                {
                    var projectDir = Path.GetDirectoryName(csproj)!;
                    artifactsLayout |= new Button("Run Sample").Icon(Icons.Play).Outline().OnClick(() =>
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = "cmd.exe",
                            Arguments = $"/k dotnet run --browse --find-available-port",
                            WorkingDirectory = projectDir,
                            UseShellExecute = true,
                            CreateNoWindow = false
                        });
                    });
                }
            }

            // Screenshots — horizontal thumbnail layout
            if (artifacts.TryGetValue("screenshots", out var screenshotFiles))
            {
                var screenshotsLayout = Layout.Horizontal().Gap(2).Wrap(Wrap.Wrap);
                foreach (var file in screenshotFiles)
                {
                    var imageUrl = $"/ivy/local-file?path={Uri.EscapeDataString(file)}";
                    var fileCapture = file;
                    screenshotsLayout |= new Image(imageUrl) { ObjectFit = ImageFit.Contain, Alt = Path.GetFileName(file) }
                        .Height(Size.Units(15)).Width(Size.Units(22))
                        .OnClick(() => openArtifact.Set(fileCapture));
                }
                artifactsLayout |= screenshotsLayout;
            }

            // Videos — clickable buttons
            if (artifacts.TryGetValue("videos", out var videoFiles))
            {
                var videosLayout = Layout.Horizontal().Gap(2).Wrap(Wrap.Wrap);
                foreach (var file in videoFiles)
                {
                    var fileName = Path.GetFileName(file);
                    var fileCapture = file;
                    videosLayout |= new Button(fileName).Ghost().Icon(Icons.Play).OnClick(() =>
                        openArtifact.Set(fileCapture));
                }
                artifactsLayout |= videosLayout;
            }

            var totalArtifacts = (artifacts.GetValueOrDefault("screenshots")?.Count ?? 0)
                + (artifacts.GetValueOrDefault("videos")?.Count ?? 0);
            content |= new Expandable(
                header: $"Artifacts ({totalArtifacts})",
                content: artifactsLayout
            ).Open(false);

            if (openArtifact.Value is { } artifactPath)
            {
                var ext = Path.GetExtension(artifactPath).ToLowerInvariant();
                var isImg = new[] { ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".svg", ".webp" }.Contains(ext);
                var isVideo = new[] { ".webm", ".mp4", ".avi", ".mov" }.Contains(ext);

                object sheetContent;
                if (isImg)
                {
                    var imageUrl = $"/ivy/local-file?path={Uri.EscapeDataString(artifactPath)}";
                    sheetContent = new Image(imageUrl) { ObjectFit = ImageFit.Contain, Alt = Path.GetFileName(artifactPath) };
                }
                else if (isVideo)
                {
                    sheetContent = Text.Block($"Video file: {Path.GetFileName(artifactPath)}");
                }
                else
                {
                    var fileContent = File.Exists(artifactPath) ? File.ReadAllText(artifactPath) : "File not found.";
                    var language = FileApp.GetLanguage(Path.GetExtension(artifactPath));
                    sheetContent = new Markdown($"```{language.ToString().ToLowerInvariant()}\n{fileContent}\n```");
                }

                content |= new Sheet(
                    onClose: () => openArtifact.Set(null),
                    content: sheetContent,
                    title: Path.GetFileName(artifactPath)
                ).Width(Size.Half());
            }
        }

        // Plan content
        content |= Text.Block("Plan").Bold();
        content |= new Markdown(_selectedPlan.LatestRevisionContent)
            .DangerouslyAllowLocalFiles()
            .OnLinkClick(url =>
            {
                if (url.StartsWith("file:///", StringComparison.OrdinalIgnoreCase))
                {
                    var filePath = url.Substring("file:///".Length);
                    openFile.Set(filePath);
                }
            });

        // File viewer sheet
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
                var fileContent = File.Exists(filePath2) ? File.ReadAllText(filePath2) : "File not found.";
                var language = FileApp.GetLanguage(Path.GetExtension(filePath2));
                sheetContent = new Markdown($"```{language.ToString().ToLowerInvariant()}\n{fileContent}\n```");
            }

            content |= new Sheet(
                onClose: () => openFile.Set(null),
                content: sheetContent,
                title: Path.GetFileName(filePath2)
            ).Width(Size.Half());
        }

        // Action bar
        var actionBar = Layout.Horizontal().Align(Align.Center).Gap(2).Padding(1)
            | new Button("Back to Draft").Icon(Icons.Pencil).Outline().OnClick(() =>
            {
                _planService.TransitionState(_selectedPlan.FolderName, PlanStatus.Draft);
                _refreshPlans();
            }).ShortcutKey("d")
            | new Button("Discard").Icon(Icons.Trash).Outline().OnClick(() =>
            {
                _planService.TransitionState(_selectedPlan.FolderName, PlanStatus.Skipped);
                _refreshPlans();
            })
            | new Button("Previous").Icon(Icons.ChevronLeft).Outline().OnClick(() => GoToPrevious()).ShortcutKey("p")
            | new Button("Next").Icon(Icons.ChevronRight, Align.Right).Outline().OnClick(() => GoToNext()).ShortcutKey("n")
            | new Button().Icon(Icons.EllipsisVertical).Ghost().WithDropDown(
                new MenuItem("Set Completed", Icon: Icons.CircleCheck, Tag: "SetCompleted").OnSelect(() =>
                {
                    _planService.TransitionState(_selectedPlan.FolderName, PlanStatus.Completed);
                    _refreshPlans();
                }),
                new MenuItem("Open in Explorer", Icon: Icons.FolderOpen, Tag: "OpenInExplorer").OnSelect(() =>
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = _selectedPlan.FolderPath,
                        UseShellExecute = true
                    });
                }),
                new MenuItem("Copy Path to Clipboard", Icon: Icons.ClipboardCopy, Tag: "CopyPath").OnSelect(() =>
                {
                    copyToClipboard(_selectedPlan.FolderPath);
                    client.Toast("Copied path to clipboard", "Path Copied");
                }),
                new MenuItem("Open plan.yaml", Icon: Icons.FileText, Tag: "OpenPlanYaml").OnSelect(() =>
                {
                    var yamlPath = System.IO.Path.Combine(_selectedPlan.FolderPath, "plan.yaml");
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "notepad.exe",
                        Arguments = yamlPath,
                        UseShellExecute = true
                    });
                })
            );

        return new HeaderLayout(
            header: header,
            content: new FooterLayout(
                footer: actionBar,
                content: content
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
}
