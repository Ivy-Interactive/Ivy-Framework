using System.Reactive.Linq;

namespace Ivy.Samples.Shared.Apps.Concepts;

[App(icon: Icons.RefreshCw, searchHints: ["reactive", "streams", "events", "subscription", "rx", "realtime"])]
public class ObservableApp : SampleBase
{
    protected override object? BuildSample()
    {
        var progress = UseState(0);

        var timeObservableRef = UseRef(() => Observable.Interval(TimeSpan.FromSeconds(1)).Select(_ => DateTime.Now.ToString("HH:mm:ss")));

        UseEffect(() =>
        {
            return Observable.Interval(TimeSpan.FromMilliseconds(100)).Take(101).Do(e => progress.Set((int)e)).Subscribe();
        });

        var timeObservable = timeObservableRef.Value;

        return Layout.Vertical(
            timeObservable,
            new Progress(value: progress.Value),
            Text.Literal($"Progress: {progress.Value}%"),
            timeObservable
        );
    }
}
