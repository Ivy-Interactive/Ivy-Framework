using Ivy.Shared;

namespace Ivy.Samples.Apps.Concepts;

[App(icon: Icons.RefreshCw)]
public class ObservableApp : SampleBase
{
    protected override object? BuildSample()
    {
        var progress = this.UseState(0);

        var timeObservable = this.UseStatic(() => Observable.Interval(TimeSpan.FromSeconds(1)).Select(_ => DateTime.Now.ToString("HH:mm:ss")));

        this.UseEffect(() =>
        {
            return Observable.Interval(TimeSpan.FromMilliseconds(100)).Take(101).Do(e => progress.Set((int)e)).Subscribe();
        });

        return Layout.Vertical(
            timeObservable,
            new Progress(value: progress.Value),
            Text.Literal($"Progress: {progress.Value}%"),
            timeObservable
        );
    }
}
