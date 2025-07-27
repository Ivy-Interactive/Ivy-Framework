using Ivy.Core;
using Ivy.Shared;
using Ivy.Views;

namespace Ivy.Helpers;

public static class JobSchedulerExtensions
{
    public static ViewBase ToView(this JobScheduler scheduler)
    {
        var jobs = scheduler.GetRootJobs();

        return Layout.Vertical()
               | jobs.Select(job => job.ToView());
    }

    private static ViewBase ToView(this Job job)
    {
        object GetIcon() => job.State switch
        {
            JobState.Waiting => Icons.Clock.ToIcon().Color(Colors.Gray).Small(),
            JobState.Running => Icons.LoaderCircle.ToIcon().Small().WithAnimation(AnimationType.Rotate).Duration(1),
            JobState.Finished => Icons.Check.ToIcon().Small().Color(Colors.Primary),
            JobState.Failed => Icons.X.ToIcon().Small().Color(Colors.Red).WithTooltip("Failed"),
            JobState.Cancelled => Icons.X.ToIcon().Small().Color(Colors.Yellow).WithTooltip("Cancelled"),
            _ => Icons.None.ToIcon().Small()
        };

        Progress? GetProgress() =>
            (Math.Abs(job.Progress - 1.0) < 0.0001 || job.Progress == 0) ? null
                : new Progress(Convert.ToInt32(Math.Round(job.Progress * 100.0))).Width(Size.Units(50));

        object? GetError() => job.State != JobState.Failed ? null : new ErrorTeaserView(job.CompletionSource.Task.Exception!);

        return Layout.Vertical()
               | (Layout.Horizontal().Align(Align.Left).Gap(2)
                  | GetIcon()
                  | Text.Muted(job.Title)
                  | new Spacer().Width(Size.Grow())
                  | GetProgress()
               )
               | GetError()
               | (job.Children.Any()
                   ? Layout.Horizontal().Gap(2).Visible(job.State != JobState.Finished)
                     | new Separator().Orientation(Orientation.Vertical).Width(4)
                     | (Layout.Vertical() | job.Children.Select(child => child.ToView()))
                   : null);
    }
}