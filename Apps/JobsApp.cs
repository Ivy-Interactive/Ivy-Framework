using Ivy;
using Ivy.Tendril.Apps.Jobs;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps;

[App(title: "Jobs", icon: Icons.Activity, group: new[] { "Tools" }, order: 20)]
public class JobsApp : ViewBase
{
    public override object? Build()
    {
        var jobService = UseService<JobService>();
        var client = UseService<IClientProvider>();
        var refreshToken = UseRefreshToken();
        UseInterval(() =>
        {
            while (jobService.PendingNotifications.TryDequeue(out var notification))
            {
                if (notification.IsSuccess)
                    client.Toast(notification.Message, notification.Title);
                else
                    client.Toast(notification.Message, notification.Title).Destructive();
            }
            refreshToken.Refresh();
        }, TimeSpan.FromSeconds(5));

        var jobs = jobService.GetJobs();
        var rows = jobs.Select(j => new JobItemRow
        {
            Id = j.Id,
            Status = j.Status,
            Plan = j.PlanFile,
            Type = j.Type,
            Project = j.Project,
            Timer = FormatTimer(j),
            Cost = j.Cost.HasValue ? $"${j.Cost.Value:F2}" : "",
            LastOutput = FormatLastOutput(j),
            StatusMessage = j.StatusMessage ?? ""
        }).ToList();

        var dataTable = rows.AsQueryable()
            .ToDataTable(idSelector: t => t.Id)
            .RefreshToken(refreshToken)
            .Width(Size.Full())
            .Height(Size.Full())
            .Header(t => t.Status, "Status")
            .Header(t => t.Plan, "Plan")
            .Header(t => t.Type, "Type")
            .Header(t => t.Project, "Project")
            .Header(t => t.Timer, "Timer")
            .Header(t => t.Cost, "Cost")
            .Header(t => t.LastOutput, "Last Output")
            .Header(t => t.StatusMessage, "Status Message")
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
                        if (job.Status == "Running")
                        {
                            jobService.StopJob(job.Id);
                            refreshToken.Refresh();
                        }
                    }
                    else if (tag == "delete-job")
                    {
                        if (job.Status != "Running")
                        {
                            jobService.DeleteJob(job.Id);
                            refreshToken.Refresh();
                        }
                    }
                }
                return ValueTask.CompletedTask;
            });

        return dataTable;
    }

    private static string FormatLastOutput(JobItem job)
    {
        if (job.LastOutputAt.HasValue && job.Status == "Running")
        {
            var elapsed = DateTime.UtcNow - job.LastOutputAt.Value;
            return FormatTimeSpan(elapsed);
        }
        return "-";
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
