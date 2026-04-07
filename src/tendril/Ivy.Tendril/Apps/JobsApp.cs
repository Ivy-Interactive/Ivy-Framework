using System.Reactive.Disposables;
using System.Text.RegularExpressions;
using Ivy.Tendril.Apps.Jobs;
using Ivy.Tendril.Apps.Plans;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps;

[App(title: "Jobs", icon: Icons.Activity, group: new[] { "Tools" }, order: MenuOrder.Jobs)]
public class JobsApp : ViewBase
{
    public override object? Build()
    {
        var jobService = UseService<IJobService>();
        var planService = UseService<IPlanReaderService>();
        var client = UseService<IClientProvider>();
        var refreshToken = UseRefreshToken();
        var showPlan = UseState<string?>(null);
        var openFile = UseState<string?>(null);
        var config = UseService<IConfigService>();
        UseEffect(() =>
        {
            void OnNotification(JobNotification notification)
            {
                if (notification.IsSuccess)
                    client.Toast(notification.Message, notification.Title);
                else
                    client.Toast(notification.Message, notification.Title).Destructive();
            }

            jobService.NotificationReady += OnNotification;
            return Disposable.Create(() => jobService.NotificationReady -= OnNotification);
        });

        UseEffect(() =>
        {
            void OnJobsChanged() => refreshToken.Refresh();
            jobService.JobsChanged += OnJobsChanged;
            return Disposable.Create(() => jobService.JobsChanged -= OnJobsChanged);
        });

        UseInterval(() =>
        {
            if (jobService.GetJobs().Any(j => j.Status == JobStatus.Running))
            {
                refreshToken.Refresh();
            }
        }, TimeSpan.FromSeconds(5));

        var projectColors = config.Projects
            .Select(p => new { p.Name, Color = config.GetProjectColor(p.Name) })
            .Where(x => x.Color.HasValue)
            .ToDictionary(x => x.Name, x => x.Color!.Value.ToString());

        var jobs = jobService.GetJobs();
        var rows = jobs.Select(j => new JobItemRow
        {
            Id = j.Id,
            Status = j.Status,
            PlanId = ExtractPlanId(j.PlanFile),
            Plan = j.PlanFile,
            Type = j.Type,
            Project = j.Project,
            Timer = FormatTimer(j),
            Cost = j.Cost.HasValue ? $"${j.Cost.Value:F2}" : "",
            LastOutput = FormatLastOutput(j),
            LastOutputTimestamp = j.LastOutputAt,
            StatusMessage = j.StatusMessage ?? ""
        })
        .OrderByDescending(r => r.LastOutputTimestamp ?? DateTime.MinValue)
        .ToList();

        var statusGroups = jobs
            .GroupBy(j => j.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .ToArray();

        var statusSegments = statusGroups
            .Select(g => new ProgressSegment(
                Value: g.Count,
                Color: GetStatusColor(g.Status),
                Label: g.Status.ToString()
            ))
            .ToArray();

        var jobsProgress = new StackedProgress(statusSegments).ShowLabels();

        var dataTable = rows.AsQueryable()
            .ToDataTable(idSelector: t => t.Id)
            .RefreshToken(refreshToken)
            .Width(Size.Full())
            .Height(Size.Full())
            .Header(t => t.Status, "Status")
            .Header(t => t.Type, "Type")
            .Header(t => t.PlanId, "Plan")
            .Header(t => t.Plan, "Plan")
            .Header(t => t.Project, "Project")
            .Header(t => t.Timer, "Timer")
            .Header(t => t.Cost, "Cost")
            .Header(t => t.LastOutput, "Last Output")
            .Header(t => t.StatusMessage, "Status Message")
            .Width(t => t.Status, Size.Px(90))
            .Width(t => t.PlanId, Size.Px(90))
            .Width(t => t.Type, Size.Px(90))
            .Width(t => t.Plan, Size.Fraction(0.5f))
            .Width(t => t.Project, Size.Px(90))
            .Width(t => t.Timer, Size.Px(90))
            .Width(t => t.LastOutput, Size.Px(90))
            .Width(t => t.Cost, Size.Px(90))
            .Width(t => t.StatusMessage, Size.Fraction(0.5f))
            .Renderer(t => t.Status, new LabelsDisplayRenderer
            {
                BadgeColorMapping = StatusMappings.JobStatusColors.ToDictionary(
                    kvp => kvp.Key.ToString(),
                    kvp => kvp.Value.ToString()
                )
            })
            .Renderer(t => t.Type, new LabelsDisplayRenderer
            {
                BadgeColorMapping = StatusMappings.JobTypeColors.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.ToString()
                )
            })
            .Renderer(t => t.Project, new LabelsDisplayRenderer
            {
                BadgeColorMapping = projectColors
            })
            .Renderer(t => t.PlanId, new LinkDisplayRenderer())
            .Hidden(t => t.Id)
            .Hidden(t => t.LastOutputTimestamp)
            .Filterable(t => t.Timer, false)
            .Filterable(t => t.LastOutput, false)
            .Sortable(t => t.Timer, false)
            .Sortable(t => t.LastOutput, false)
            .Config(c =>
            {
                c.AllowSorting = true;
                c.AllowFiltering = true;
                c.ShowSearch = false;
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
                    if (!string.IsNullOrEmpty(planId))
                    {
                        var job = jobs.FirstOrDefault(j => ExtractPlanId(j.PlanFile) == planId);
                        if (job != null && !string.IsNullOrEmpty(job.PlanFile))
                        {
                            var fullPath = Path.Combine(planService.PlansDirectory, job.PlanFile);
                            if (Directory.Exists(fullPath))
                                showPlan.Set(fullPath);
                        }
                    }
                }
                return ValueTask.CompletedTask;
            })
            .RowActions(
                new MenuItem(Label: "View Plan", Icon: Icons.FileText, Tag: "view-plan").Tooltip("Open the associated plan"),
                new MenuItem(Label: "Stop", Icon: Icons.Square, Tag: "stop-job").Tooltip("Stop this running job"),
                new MenuItem(Label: "Rerun", Icon: Icons.RotateCw, Tag: "rerun-job").Tooltip("Rerun this job"),
                new MenuItem(Label: "Delete", Icon: Icons.Trash, Tag: "delete-job").Tooltip("Delete this job")
            )
            .OnRowAction(e =>
            {
                var tag = e.Value.Tag?.ToString();
                var id = e.Value.Id?.ToString();
                var job = jobs.FirstOrDefault(j => j.Id == id);

                if (job != null)
                {
                    if (tag == "view-plan")
                    {
                        if (!string.IsNullOrEmpty(job.PlanFile))
                        {
                            var fullPath = Path.Combine(planService.PlansDirectory, job.PlanFile);
                            if (Directory.Exists(fullPath))
                                showPlan.Set(fullPath);
                        }
                    }
                    else if (tag == "stop-job")
                    {
                        if (job.Status is JobStatus.Running or JobStatus.Queued)
                        {
                            jobService.StopJob(job.Id);
                            refreshToken.Refresh();
                        }
                    }
                    else if (tag == "rerun-job")
                    {
                        if (job.Status is JobStatus.Failed or JobStatus.Timeout or JobStatus.Stopped)
                        {
                            if (job.Type is "ExecutePlan" or "ExpandPlan" && job.Args.Length > 0)
                            {
                                var folderName = Path.GetFileName(job.Args[0]);
                                planService.TransitionState(folderName, Plans.PlanStatus.Building);
                            }
                            else if (job.Type == "UpdatePlan" && job.Args.Length > 0)
                            {
                                var folderName = Path.GetFileName(job.Args[0]);
                                planService.TransitionState(folderName, Plans.PlanStatus.Updating);
                            }
                            jobService.StartJob(job.Type, job.Args);
                            refreshToken.Refresh();
                        }
                    }
                    else if (tag == "delete-job")
                    {
                        if (job.Status != JobStatus.Running)
                        {
                            jobService.DeleteJob(job.Id);
                            refreshToken.Refresh();
                        }
                    }
                }
                return ValueTask.CompletedTask;
            })
            .HeaderRight(ctx => Layout.Horizontal().Gap(2)
                | jobsProgress
                | new Button().Icon(Icons.EllipsisVertical).Ghost().WithDropDown(
                    new MenuItem("Clear Completed", Icon: Icons.Trash, Tag: "ClearCompleted").OnSelect(() =>
                    {
                        jobService.ClearCompletedJobs();
                        refreshToken.Refresh();
                    }),
                    new MenuItem("Clear Failed", Icon: Icons.Trash, Tag: "ClearFailed").OnSelect(() =>
                    {
                        jobService.ClearFailedJobs();
                        refreshToken.Refresh();
                    })
                ));

        var layout = Layout.Vertical().Height(Size.Full());

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
                onClose: () => showPlan.Set(null),
                content: sheetContent,
                title: plan?.Title ?? folderName
            ).Width(Size.Half()).Resizable();

            if (fileLinkSheet is not null)
            {
                return layout | new Fragment(dataTable, planSheet, fileLinkSheet);
            }

            return layout | new Fragment(dataTable, planSheet);
        }

        return layout | dataTable;
    }

    private static string ExtractPlanId(string planFile)
    {
        if (string.IsNullOrEmpty(planFile)) return "";
        var match = Regex.Match(planFile, @"^(\d{5})-");
        return match.Success ? match.Groups[1].Value : "";
    }

    private static string FormatLastOutput(JobItem job)
    {
        if (job.LastOutputAt.HasValue && job.Status == JobStatus.Running)
        {
            var elapsed = DateTime.UtcNow - job.LastOutputAt.Value;
            return FormatTimeSpan(elapsed);
        }
        return "-";
    }

    private static string FormatTimer(JobItem job)
    {
        if (job.Status == JobStatus.Running && job.StartedAt.HasValue)
        {
            var elapsed = DateTime.UtcNow - job.StartedAt.Value;
            return FormatTimeSpan(elapsed);
        }

        if (job.Status is JobStatus.Completed or JobStatus.Failed or JobStatus.Timeout or JobStatus.Stopped && job.DurationSeconds.HasValue)
        {
            return FormatTimeSpan(TimeSpan.FromSeconds(job.DurationSeconds.Value));
        }

        return "-";
    }

    private static string FormatTimeSpan(TimeSpan span)
    {
        if (span.TotalHours >= 1)
            return $"{(int)span.TotalHours}h {span.Minutes:D2}m";
        return $"{span.Minutes}m {span.Seconds:D2}s";
    }

    private static Colors GetStatusColor(JobStatus status) =>
        StatusMappings.JobStatusColors.TryGetValue(status, out var color) ? color : Colors.Slate;
}
