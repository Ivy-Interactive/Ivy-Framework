using Ivy;
using Ivy.Core;
using Ivy.Hooks;
using Ivy.Tendril.Apps.Plans;
using Ivy.Tendril.Services;

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
        var navigator = UseNavigation();
        var client = UseService<IClientProvider>();
        var copyToClipboard = UseClipboard();

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
            | new Badge(_selectedPlan.Queue).Variant(BadgeVariant.Outline)
            | new Badge(_selectedPlan.Level).Variant(_config.GetBadgeVariant(_selectedPlan.Level))
            | new Spacer().Width(Size.Grow())
            | Text.Rich()
                .Bold($"{currentIndex + 1}/{_allPlans.Count}", word: true)
                .Muted("plans", word: true);

        // Content sections
        var content = Layout.Vertical().Width(Size.Auto().Max(Size.Units(200))).Gap(4);

        // Verifications section
        if (_selectedPlan.Verifications.Count > 0)
        {
            var verificationsLayout = Layout.Vertical().Gap(1);
            verificationsLayout |= Text.Block("Verifications").Bold();
            foreach (var v in _selectedPlan.Verifications)
            {
                var variant = v.Status switch
                {
                    "Pass" => BadgeVariant.Success,
                    "Fail" => BadgeVariant.Destructive,
                    _ => BadgeVariant.Outline
                };
                verificationsLayout |= Layout.Horizontal().Gap(2)
                    | new Badge(v.Status).Variant(variant).Small()
                    | Text.Block(v.Name);
            }
            content |= verificationsLayout;
        }

        // Commits section
        if (_selectedPlan.Commits.Count > 0)
        {
            var commitsLayout = Layout.Vertical().Gap(1);
            commitsLayout |= Text.Block("Commits").Bold();
            var repoPaths = _config.GetProject(_selectedPlan.Queue)?.RepoPaths ?? [];
            foreach (var commit in _selectedPlan.Commits)
            {
                var title = repoPaths
                    .Select(repo => _gitService.GetCommitTitle(repo, commit))
                    .FirstOrDefault(t => t != null);
                var shortHash = commit.Length > 7 ? commit[..7] : commit;
                var commitCapture = commit;
                var row = Layout.Horizontal().Gap(2)
                    | new Button(shortHash).Ghost().Small().OnClick(() =>
                        navigator.Navigate<CommitApp>(new CommitAppArgs(commitCapture, _selectedPlan.Queue)))
                    | Text.Block(title != null ? $"— {title}" : "");
                commitsLayout |= row;
            }
            content |= commitsLayout;
        }

        // PRs section
        if (_selectedPlan.Prs.Count > 0)
        {
            var prsLayout = Layout.Vertical().Gap(1);
            prsLayout |= Text.Block("Pull Requests").Bold();
            foreach (var pr in _selectedPlan.Prs)
            {
                prsLayout |= Text.Block($"  {pr}");
            }
            content |= prsLayout;
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
                    navigator.Navigate<FileApp>(new FileAppArgs(filePath));
                }
            });

        // Action bar
        var actionBar = Layout.Horizontal().Align(Align.Center).Gap(2).Padding(1)
            | new Button("Back to Draft").Icon(Icons.Pencil).Outline().OnClick(() =>
            {
                _planService.TransitionState(_selectedPlan.FolderName, PlanStatus.Draft);
                _refreshPlans();
            })
            | new Button("Discard").Icon(Icons.Trash).Outline().OnClick(() =>
            {
                _planService.TransitionState(_selectedPlan.FolderName, PlanStatus.Skipped);
                _refreshPlans();
            })
            | new Button("Previous").Icon(Icons.ChevronLeft).Outline().OnClick(() => GoToPrevious()).ShortcutKey("p")
            | new Button("Next").Icon(Icons.ChevronRight, Align.Right).Outline().OnClick(() => GoToNext()).ShortcutKey("n")
            | new Spacer().Width(Size.Grow())
            | new Button("Make PR").Icon(Icons.GitPullRequest).Primary().OnClick(() =>
            {
                _planService.TransitionState(_selectedPlan.FolderName, PlanStatus.Completed);
                _jobService.StartJob("MakePr", _selectedPlan.FolderPath);
                _refreshPlans();
                client.Toast("PR job started", "Make PR");
            })
            | new Button().Icon(Icons.EllipsisVertical).Ghost().WithDropDown(
                new MenuItem("Copy Path to Clipboard", Icon: Icons.ClipboardCopy, Tag: "CopyPath").OnSelect(() =>
                {
                    copyToClipboard(_selectedPlan.FolderPath);
                    client.Toast("Copied path to clipboard", "Path Copied");
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
