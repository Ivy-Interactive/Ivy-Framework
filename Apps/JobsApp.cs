using Ivy;
using Ivy.Tendril.Apps.Jobs;
using Ivy.Tendril.Apps.Jobs.Dialogs;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps;

[App(title: "Jobs", icon: Icons.Activity, group: new[] { "Tools" }, order: 20)]
public class JobsApp : ViewBase
{
    public override object? Build()
    {
        var jobService = UseService<JobService>();
        var refreshToken = UseRefreshToken();
        var selectedJobId = UseState<string?>(null);
        var dialogMode = UseState<string?>(null);

        UseInterval(() => refreshToken.Refresh(), TimeSpan.FromSeconds(5));

        var jobs = jobService.GetJobs();
        var rows = jobs.Select(j => new JobItemRow
        {
            Id = j.Id,
            Status = j.Status,
            Plan = j.PlanFile,
            Type = j.Type,
            Queue = j.Queue,
            Timer = FormatTimer(j)
        }).ToList();

        var dataTable = rows.AsQueryable()
            .ToDataTable(idSelector: t => t.Id)
            .RefreshToken(refreshToken)
            .Width(Size.Full())
            .Height(Size.Full())
            .Header(t => t.Status, "Status")
            .Header(t => t.Plan, "Plan")
            .Header(t => t.Type, "Type")
            .Header(t => t.Queue, "Queue")
            .Header(t => t.Timer, "Timer")
            .Renderer(t => t.Status, new LabelsDisplayRenderer())
            .Hidden(t => t.Id)
            .Config(c =>
            {
                c.AllowSorting = false;
                c.AllowFiltering = false;
                c.ShowSearch = false;
                c.SelectionMode = SelectionModes.None;
                c.ShowIndexColumn = false;
                c.BatchSize = 50;
            })
            .RowActions(
                new MenuItem(Label: "View Output", Icon: Icons.Terminal, Tag: "view-output"),
                new MenuItem(Label: "View Plan", Icon: Icons.FileText, Tag: "view-plan"),
                new MenuItem(Label: "Stop", Icon: Icons.Square, Tag: "stop-job"),
                new MenuItem(Label: "Delete", Icon: Icons.Trash, Tag: "delete-job")
            )
            .OnRowAction(e =>
            {
                var tag = e.Value.Tag?.ToString();
                var id = e.Value.Id?.ToString();
                var job = jobs.FirstOrDefault(j => j.Id == id);

                if (job != null)
                {
                    if (tag == "stop-job")
                    {
                        // Only allow stopping running jobs
                        if (job.Status == "Running")
                        {
                            jobService.StopJob(job.Id);
                            refreshToken.Refresh();
                        }
                    }
                    else if (tag == "delete-job")
                    {
                        // Only allow deleting non-running jobs
                        if (job.Status != "Running")
                        {
                            jobService.DeleteJob(job.Id);
                            refreshToken.Refresh();
                        }
                    }
                    else
                    {
                        selectedJobId.Set(job.Id);
                        dialogMode.Set(tag);
                    }
                }
                return ValueTask.CompletedTask;
            });

        var elements = new List<object>
        {
            dataTable
        };

        if (dialogMode.Value != null && selectedJobId.Value is { } jobId)
        {
            var job = jobService.GetJob(jobId);
            if (job != null)
            {
                void CloseDialog()
                {
                    dialogMode.Set(null);
                    selectedJobId.Set(null);
                }

                if (dialogMode.Value == "view-output" && !string.IsNullOrEmpty(job.ScriptPath))
                {
                    elements.Add(new ViewJobOutputDialog(true, job, jobService, CloseDialog));
                }
                else
                {
                    elements.Add(new ViewJobPlanDialog(true, job, CloseDialog));
                }
            }
        }

        return new Fragment(elements.ToArray());
    }

    private static string FormatTimer(JobItem job)
    {
        if (job.Status == "Running" && job.StartedAt.HasValue)
        {
            var elapsed = DateTime.UtcNow - job.StartedAt.Value;
            return FormatTimeSpan(elapsed);
        }

        if ((job.Status == "Completed" || job.Status == "Failed") && job.DurationSeconds.HasValue)
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
}
