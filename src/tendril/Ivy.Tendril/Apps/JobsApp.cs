using System.Reactive.Disposables;
using System.Text.RegularExpressions;
using Ivy.Tendril.Apps.Jobs;
using Ivy.Tendril.Apps.Plans;
using Ivy.Tendril.Helpers;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps;

[App(title: "Jobs", icon: Icons.Activity, group: ["Apps"], order: MenuOrder.Jobs)]
public class JobsApp : ViewBase
{
    public override object Build()
    {
        var jobService = UseService<IJobService>();
        var planService = UseService<IPlanReaderService>();
        var client = UseService<IClientProvider>();
        var refreshToken = UseRefreshToken();
        var showPlan = UseState<string?>(null);
        var showOutput = UseState<string?>(null);
        var showPrompt = UseState<string?>(null);
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
            void OnJobsChanged()
            {
                refreshToken.Refresh();
            }

            jobService.JobsChanged += OnJobsChanged;
            return Disposable.Create(() => jobService.JobsChanged -= OnJobsChanged);
        });

        UseInterval(() =>
        {
            if (jobService.GetJobs().Any(j => j.Status == JobStatus.Running)) refreshToken.Refresh();
        }, TimeSpan.FromSeconds(5));

        var projectColors = config.Projects
            .Select(p => new { p.Name, Color = config.GetProjectColor(p.Name) })
            .Where(x => x.Color.HasValue)
            .ToDictionary(x => x.Name, x => x.Color!.Value.ToString());

        var jobs = jobService.GetJobs();
        var rows = jobs.Select(j =>
        {
            var planId = ExtractPlanId(j.PlanFile);
            var displayPlanId = planId;

            return new JobItemRow
            {
                Id = j.Id,
                Status = j.Status,
                PlanId = displayPlanId,
                Plan = GetPromptDisplay(j, planService),
                Type = j.Type,
                Project = string.Join(", ", ProjectHelper.ParseProjects(j.Project)),
                Timer = FormatTimer(j),
                Cost = j.Cost.HasValue ? $"${j.Cost.Value:F2}" : "",
                Tokens = j.Tokens.HasValue ? FormatHelper.FormatTokens(j.Tokens.Value) : "",
                LastOutput = FormatLastOutput(j),
                LastOutputTimestamp = j.LastOutputAt,
                StatusMessage = GetStatusMessage(j)
            };
        })
            .OrderBy(r => r.Id)
            .ToList();

        var statusGroups = jobs
            .GroupBy(j => j.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .ToArray();

        var statusSegments = statusGroups
            .Select(g => new ProgressSegment(
                g.Count,
                GetStatusColor(g.Status),
                g.Status.ToString()
            ))
            .ToArray();

        var jobsProgress = new StackedProgress(statusSegments).ShowLabels();

        var dataTable = rows.AsQueryable()
            .ToDataTable(t => t.Id)
            .RefreshToken(refreshToken)
            .Width(Size.Full())
            .Height(Size.Full())
            .Header(t => t.Status, "Status")
            .Header(t => t.Type, "Type")
            .Header(t => t.PlanId, "Plan")
            .Header(t => t.Plan, "Prompt/Title")
            .Header(t => t.Project, "Project")
            .Header(t => t.Timer, "Timer")
            .Header(t => t.Cost, "Cost")
            .Header(t => t.Tokens, "Tokens")
            .Header(t => t.LastOutput, "Last Output")
            .Header(t => t.StatusMessage, "Status")
            .Width(t => t.Status, Size.Px(100))
            .Width(t => t.PlanId, Size.Px(100))
            .Width(t => t.Type, Size.Px(100))
            .Width(t => t.Plan, Size.Auto())
            .Width(t => t.Project, Size.Px(100))
            .Width(t => t.Timer, Size.Px(100))
            .Width(t => t.LastOutput, Size.Px(100))
            .Width(t => t.Cost, Size.Px(100))
            .Width(t => t.Tokens, Size.Px(100))
            .Width(t => t.StatusMessage, Size.Px(250))
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
            .Config(c =>
            {
                c.AllowSorting = false;
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
                else if (e.Value.ColumnName == "LastOutput")
                {
                    var id = e.Value.RowId?.ToString();
                    if (!string.IsNullOrEmpty(id))
                        showOutput.Set(id);
                }

                return ValueTask.CompletedTask;
            })
            .RowActions(
                new MenuItem("View Plan", Icon: Icons.FileText, Tag: "view-plan").Tooltip("Open the associated plan"),
                new MenuItem("Show Prompt", Icon: Icons.MessageSquare, Tag: "show-prompt").Tooltip(
                    "Show the full prompt text"),
                new MenuItem("Stop", Icon: Icons.Square, Tag: "stop-job").Tooltip("Stop this running job"),
                new MenuItem("Rerun", Icon: Icons.RotateCw, Tag: "rerun-job").Tooltip("Rerun this job"),
                new MenuItem("Delete", Icon: Icons.Trash, Tag: "delete-job").Tooltip("Delete this job")
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
                    else if (tag == "show-prompt")
                    {
                        var fullPrompt = GetFullPrompt(job);
                        if (!string.IsNullOrEmpty(fullPrompt))
                            showPrompt.Set(fullPrompt);
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
                                planService.TransitionState(folderName, PlanStatus.Building);
                            }
                            else if (job.Type == "UpdatePlan" && job.Args.Length > 0)
                            {
                                var folderName = Path.GetFileName(job.Args[0]);
                                planService.TransitionState(folderName, PlanStatus.Updating);
                            }

                            jobService.DeleteJob(job.Id);
                            jobService.StartJob(job.Type, job.Args);
                            refreshToken.Refresh();
                        }
                    }
                    else if (tag == "delete-job")
                    {
                        if (job.Status is JobStatus.Running or JobStatus.Queued)
                        {
                            jobService.StopJob(job.Id);
                        }

                        jobService.DeleteJob(job.Id);
                        refreshToken.Refresh();
                    }
                }

                return ValueTask.CompletedTask;
            })
            .HeaderRight(_ => Layout.Horizontal().Gap(2)
                              | jobsProgress
                              | new Button().Icon(Icons.EllipsisVertical).Ghost().WithDropDown(
                                  new MenuItem("Clear Completed", Icon: Icons.Trash, Tag: "ClearCompleted")
                                      .OnSelect(() =>
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
                openFile.Value, () => openFile.Set(null), repoPaths, config);

            var planSheet = new Sheet(
                () => showPlan.Set(null),
                sheetContent,
                plan?.Title ?? folderName
            ).Width(Size.Half()).Resizable();

            if (fileLinkSheet is not null) return layout | new Fragment(dataTable, planSheet, fileLinkSheet);

            return layout | new Fragment(dataTable, planSheet);
        }

        if (showOutput.Value is { } jobId)
        {
            var job = jobService.GetJob(jobId);
            var outputText = job is not null && job.OutputLines.Count > 0
                ? string.Join("\n", job.OutputLines)
                : "No output available.";

            var outputSheet = new Sheet(
                () => showOutput.Set(null),
                new Markdown($"```\n{outputText}\n```"),
                job is not null ? $"{job.Type} — {ExtractPlanId(job.PlanFile)}" : "Job Output"
            ).Width(Size.Half()).Resizable();

            return layout | new Fragment(dataTable, outputSheet);
        }

        if (showPrompt.Value is { } promptText)
        {
            var promptSheet = new Sheet(
                () => showPrompt.Set(null),
                new Markdown($"```\n{promptText}\n```"),
                "Full Prompt"
            ).Width(Size.Half()).Resizable();

            return layout | new Fragment(dataTable, promptSheet);
        }

        return layout | dataTable;
    }

    private static string? GetFullPrompt(JobItem job)
    {
        if (job.Type == "MakePlan")
        {
            for (var i = 0; i < job.Args.Length - 1; i++)
                if (job.Args[i].Equals("-Description", StringComparison.OrdinalIgnoreCase))
                    return job.Args[i + 1];
        }

        return job.PlanFile;
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
        if (job is { Status: JobStatus.Running, StartedAt: not null })
        {
            var elapsed = DateTime.UtcNow - job.StartedAt.Value;
            return FormatTimeSpan(elapsed);
        }

        if (job.Status is JobStatus.Completed or JobStatus.Failed or JobStatus.Timeout or JobStatus.Stopped &&
            job.DurationSeconds.HasValue) return FormatTimeSpan(TimeSpan.FromSeconds(job.DurationSeconds.Value));

        return "-";
    }

    private static string FormatTimeSpan(TimeSpan span)
    {
        if (span.TotalHours >= 1)
            return $"{(int)span.TotalHours}h {span.Minutes:D2}m";
        return $"{span.Minutes}m {span.Seconds:D2}s";
    }

    private const int PromptDisplayMaxLength = 150;

    private static string CleanPromptText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;
        var replaced = text.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ");
        var collapsed = Regex.Replace(replaced, @"\s+", " ");
        return collapsed.Trim();
    }

    private static string GetPromptDisplay(JobItem j, IPlanReaderService planService)
    {
        // MakePlan jobs: use the -Description arg for display (PlanFile may now hold the folder name)
        if (j.Type == "MakePlan")
        {
            var desc = CleanPromptText(GetFullPrompt(j) ?? j.PlanFile);
            return desc.Length > PromptDisplayMaxLength ? desc[..PromptDisplayMaxLength] + "..." : desc;
        }

        // For other jobs, try to read the plan title
        if (!string.IsNullOrEmpty(j.PlanFile))
        {
            var fullPath = Path.Combine(planService.PlansDirectory, j.PlanFile);
            var plan = planService.GetPlanByFolder(fullPath);
            if (plan != null && !string.IsNullOrEmpty(plan.Title))
            {
                var title = CleanPromptText(plan.Title);
                return title.Length > PromptDisplayMaxLength ? title[..PromptDisplayMaxLength] + "..." : title;
            }
        }

        // Fallback to folder name
        var pf = CleanPromptText(j.PlanFile);
        return pf.Length > PromptDisplayMaxLength ? pf[..PromptDisplayMaxLength] + "..." : pf;
    }

    private static string GetStatusMessage(JobItem job)
    {
        if (!string.IsNullOrEmpty(job.StatusMessage))
            return job.StatusMessage;

        return job.Status switch
        {
            JobStatus.Blocked => "Waiting for dependency plan(s) to complete",
            JobStatus.Failed => "Job encountered an error during execution",
            JobStatus.Timeout => "Job exceeded the configured timeout",
            JobStatus.Queued => "Waiting for a job slot to become available",
            _ => ""
        };
    }

    private static Colors GetStatusColor(JobStatus status)
    {
        return StatusMappings.JobStatusColors.GetValueOrDefault(status, Colors.Slate);
    }
}
