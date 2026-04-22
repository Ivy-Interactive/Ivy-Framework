using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Ivy.Test.Hooks;

public class UseObservableStreamTests
{
    [Fact]
    public void Merge_CombinesMultipleSources()
    {
        var received = new List<DataTableCellUpdate>();
        var sender = new CapturingSender();
        var stream = new WriteStream<DataTableCellUpdate>("s1", sender, buffer: false);

        var subject1 = new Subject<DataTableCellUpdate>();
        var subject2 = new Subject<DataTableCellUpdate>();
        var subject3 = new Subject<DataTableCellUpdate>();

        using var sub = Observable.Merge(subject1, subject2, subject3)
            .Subscribe(data => stream.Write(data));

        subject1.OnNext(new DataTableCellUpdate("r1", "A", 1));
        subject2.OnNext(new DataTableCellUpdate("r2", "B", 2));
        subject3.OnNext(new DataTableCellUpdate("r3", "C", 3));

        Assert.Equal(3, sender.Messages.Count);
    }

    [Fact]
    public void Merge_PreservesOrderWithinSource()
    {
        var sender = new CapturingSender();
        var stream = new WriteStream<DataTableCellUpdate>("s1", sender, buffer: false);

        var subject = new Subject<DataTableCellUpdate>();
        using var sub = Observable.Merge(subject)
            .Subscribe(data => stream.Write(data));

        subject.OnNext(new DataTableCellUpdate("r1", "Col", "first"));
        subject.OnNext(new DataTableCellUpdate("r1", "Col", "second"));
        subject.OnNext(new DataTableCellUpdate("r1", "Col", "third"));

        Assert.Equal(3, sender.Messages.Count);
    }

    [Fact]
    public void Disposal_UnsubscribesFromAllSources()
    {
        var sender = new CapturingSender();
        var stream = new WriteStream<DataTableCellUpdate>("s1", sender, buffer: false);

        var subject1 = new Subject<DataTableCellUpdate>();
        var subject2 = new Subject<DataTableCellUpdate>();

        var sub = Observable.Merge(subject1, subject2)
            .Subscribe(data => stream.Write(data));

        subject1.OnNext(new DataTableCellUpdate("r1", "Col", "val"));
        Assert.Single(sender.Messages);

        sub.Dispose();

        subject1.OnNext(new DataTableCellUpdate("r2", "Col", "val"));
        subject2.OnNext(new DataTableCellUpdate("r3", "Col", "val"));
        Assert.Single(sender.Messages);
    }

    [Fact]
    public void Buffer_FlushesOnSubscription()
    {
        var sender = new CapturingSender();
        var stream = new WriteStream<DataTableCellUpdate>("buf-test", sender, buffer: true);

        var subject = new Subject<DataTableCellUpdate>();
        using var sub = Observable.Merge(subject)
            .Subscribe(data => stream.Write(data));

        subject.OnNext(new DataTableCellUpdate("r1", "Col", "buffered1"));
        subject.OnNext(new DataTableCellUpdate("r2", "Col", "buffered2"));

        Assert.Empty(sender.Messages);

        StreamRegistry.NotifySubscribed("buf-test");

        Assert.Equal(2, sender.Messages.Count);

        subject.OnNext(new DataTableCellUpdate("r3", "Col", "live"));
        Assert.Equal(3, sender.Messages.Count);
    }

    [Fact]
    public void EmptySources_NoErrors()
    {
        var sender = new CapturingSender();
        var stream = new WriteStream<DataTableCellUpdate>("s1", sender, buffer: false);

        using var sub = Observable.Merge(Array.Empty<IObservable<DataTableCellUpdate>>())
            .Subscribe(data => stream.Write(data));

        Assert.Empty(sender.Messages);
    }

    [Fact]
    public void CompletedSource_DoesNotAffectOtherSources()
    {
        var sender = new CapturingSender();
        var stream = new WriteStream<DataTableCellUpdate>("s1", sender, buffer: false);

        var subject1 = new Subject<DataTableCellUpdate>();
        var subject2 = new Subject<DataTableCellUpdate>();

        using var sub = Observable.Merge(subject1, subject2)
            .Subscribe(data => stream.Write(data));

        subject1.OnNext(new DataTableCellUpdate("r1", "Col", "val"));
        subject1.OnCompleted();

        subject2.OnNext(new DataTableCellUpdate("r2", "Col", "val"));
        Assert.Equal(2, sender.Messages.Count);
    }

    private class CapturingSender : IClientSender
    {
        public List<(string Method, object? Data)> Messages { get; } = [];
        public void Send(string method, object? data) => Messages.Add((method, data));
    }
}
