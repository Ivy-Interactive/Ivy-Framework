namespace Ivy.Samples.Shared.Apps.Concepts;

/// <summary>
/// Standalone repro of <c>Ivy.Tendril.Apps.Plans.Dialogs.CreateIssueDialog</c> for investigating dialog + select + dependent UseQuery behavior (e.g. overlay / z-index issues).
/// </summary>
[App(icon: Icons.Github, searchHints: ["dialog", "github", "issue", "select", "usequery", "tendril", "create issue"])]
public class CreateIssueDialogSampleApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Vertical().Gap(4)
               | Text.H1("Create Issue Dialog (Tendril repro)")
               | Text.P(
                   "Same structure as Tendril CreateIssueDialog: modal, repository select (AutoFocus), "
                   + "assignee and labels loaded via UseQuery when the repo changes, multi-select labels, comment. "
                   + "GitHub and jobs are mocked.")
               | new CreateIssueDialogSampleLauncher();
    }
}

public class CreateIssueDialogSampleLauncher : ViewBase
{
    public override object? Build()
    {
        var dialogOpen = UseState(false);
        var selectedRepoState = UseState<string?>(null);
        var issueAssigneeState = UseState<string?>(null);
        var issueLabelsState = UseState(Array.Empty<string>());
        var issueCommentState = UseState("");

        return Layout.Vertical().Gap(3)
               | new Button("Open Create Issue Dialog", _ => dialogOpen.Set(true))
                   .Primary()
                   .Icon(Icons.Github)
               | new CreateIssueDialogReplica(
                   dialogOpen,
                   selectedRepoState,
                   issueAssigneeState,
                   issueLabelsState,
                   issueCommentState);
    }
}

/// <summary>
/// Mirrors Tendril <c>CreateIssueDialog</c> layout and data flow; uses mock repos instead of IGithubService/IJobService.
/// </summary>
public class CreateIssueDialogReplica(
    IState<bool> dialogOpen,
    IState<string?> selectedRepoState,
    IState<string?> issueAssigneeState,
    IState<string[]> issueLabelsState,
    IState<string> issueCommentState) : ViewBase
{
    private readonly IState<bool> _dialogOpen = dialogOpen;
    private readonly IState<string?> _selectedRepoState = selectedRepoState;
    private readonly IState<string?> _issueAssigneeState = issueAssigneeState;
    private readonly IState<string[]> _issueLabelsState = issueLabelsState;
    private readonly IState<string> _issueCommentState = issueCommentState;

    private const string SamplePlanFolder = "/sample/plan/path";
    private const int SamplePlanId = 42;

    public override object? Build()
    {
        var client = UseService<IClientProvider>();

        var assigneesQuery = UseQuery<string[], string>(
            _selectedRepoState.Value ?? "",
            async (repoName, ct) =>
            {
                if (string.IsNullOrEmpty(repoName)) return Array.Empty<string>();
                await Task.Delay(50, ct);
                var repos = MockGithub.Repos;
                var selectedRepo = repos.FirstOrDefault(r => r.DisplayName == repoName);
                if (selectedRepo is null) return Array.Empty<string>();
                return selectedRepo.Assignees;
            },
            initialValue: Array.Empty<string>()
        );

        var labelsQuery = UseQuery<string[], string>(
            _selectedRepoState.Value ?? "",
            async (repoName, ct) =>
            {
                if (string.IsNullOrEmpty(repoName)) return Array.Empty<string>();
                await Task.Delay(50, ct);
                var repos = MockGithub.Repos;
                var selectedRepo = repos.FirstOrDefault(r => r.DisplayName == repoName);
                if (selectedRepo is null) return Array.Empty<string>();
                return selectedRepo.Labels;
            },
            initialValue: Array.Empty<string>()
        );

        if (!_dialogOpen.Value) return null;

        var repositoryOptions = MockGithub.Repos.Select(r => r.DisplayName).ToArray();
        var assignees = assigneesQuery.Value ?? Array.Empty<string>();
        var labels = labelsQuery.Value ?? Array.Empty<string>();

        return new Dialog(
            _ => _dialogOpen.Set(false),
            new DialogHeader($"Create GitHub Issue #{SamplePlanId}"),
            new DialogBody(
                Layout.Vertical().Gap(3)
                    | _selectedRepoState.ToSelectInput(repositoryOptions.ToOptions())
                        .AutoFocus().WithField().Label("Repository").Required()
                    | _issueAssigneeState.ToSelectInput(assignees.ToOptions())
                        .Nullable().WithField().Label("Assignee")
                    | _issueLabelsState.ToSelectInput(labels.ToOptions())
                        .Placeholder("Select labels...").WithField().Label("Labels")
                    | _issueCommentState.ToTextInput().Multiline().WithField().Label("Comment")
            ),
            new DialogFooter(
                new Button("Cancel").Outline().OnClick(() => _dialogOpen.Set(false)),
                new Button("Create Issue").Primary().OnClick(() =>
                {
                    if (_selectedRepoState.Value is { } repo)
                    {
                        var selectedRepo = MockGithub.Repos.FirstOrDefault(r => r.DisplayName == repo);
                        if (selectedRepo is not null)
                        {
                            var repoPath = selectedRepo.FullName;
                            var assignee = _issueAssigneeState.Value ?? "";
                            var labelCsv = string.Join(",", _issueLabelsState.Value ?? Array.Empty<string>());
                            client.Toast(
                                $"CreateIssue (mock): repo={repoPath}, assignee={assignee}, labels={labelCsv}, "
                                + $"comment={_issueCommentState.Value ?? ""}, folder={SamplePlanFolder}");
                        }
                    }
                    _dialogOpen.Set(false);
                })
            )
        ).Width(Size.Rem(30));
    }
}

file static class MockGithub
{
    public static readonly MockRepo[] Repos =
    [
        new(
            DisplayName: "ivy-framework/Ivy",
            FullName: "Ivy-Interactive/Ivy-Framework",
            Assignees: ["alice", "bob"],
            Labels: ["bug", "enhancement", "docs"]),
        new(
            DisplayName: "demo/sample-repo",
            FullName: "demo/sample-repo",
            Assignees: ["charlie"],
            Labels: ["triage", "good first issue"]),
    ];

    public sealed record MockRepo(
        string DisplayName,
        string FullName,
        string[] Assignees,
        string[] Labels);
}
