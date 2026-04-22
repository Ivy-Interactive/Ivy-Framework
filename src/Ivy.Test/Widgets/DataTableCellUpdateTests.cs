using System.Reactive.Subjects;
using System.Text.Json;

namespace Ivy.Test.Widgets;

public class DataTableCellUpdateTests
{
    [Fact]
    public void Record_CreatesWithCorrectProperties()
    {
        var update = new DataTableCellUpdate("row1", "Status", "Active");

        Assert.Equal("row1", update.RowId);
        Assert.Equal("Status", update.ColumnName);
        Assert.Equal("Active", update.Value);
    }

    [Fact]
    public void Record_SupportsNullValue()
    {
        var update = new DataTableCellUpdate(42, "Notes", null);

        Assert.Equal(42, update.RowId);
        Assert.Equal("Notes", update.ColumnName);
        Assert.Null(update.Value);
    }

    [Fact]
    public void Record_SupportsNumericRowId()
    {
        var update = new DataTableCellUpdate(123, "Progress", 0.75);

        Assert.Equal(123, update.RowId);
        Assert.Equal("Progress", update.ColumnName);
        Assert.Equal(0.75, update.Value);
    }

    [Fact]
    public void Record_SupportsEqualityComparison()
    {
        var a = new DataTableCellUpdate("row1", "Col", "val");
        var b = new DataTableCellUpdate("row1", "Col", "val");

        Assert.Equal(a, b);
    }

    [Fact]
    public void Record_InequalityOnDifferentValues()
    {
        var a = new DataTableCellUpdate("row1", "Col", "val1");
        var b = new DataTableCellUpdate("row1", "Col", "val2");

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void WriteStream_SendsDataToClient()
    {
        var messages = new List<(string method, object? data)>();
        var sender = new CapturingSender(messages);
        var stream = new WriteStream<DataTableCellUpdate>("test-stream", sender, buffer: false);

        var update = new DataTableCellUpdate("row1", "Status", "Done");
        stream.Write(update);

        Assert.Single(messages);
        Assert.Equal("StreamData", messages[0].method);
    }

    [Fact]
    public void WriteStream_BuffersUntilSubscribed()
    {
        var messages = new List<(string method, object? data)>();
        var sender = new CapturingSender(messages);
        var stream = new WriteStream<DataTableCellUpdate>("test-stream", sender, buffer: true);

        stream.Write(new DataTableCellUpdate("row1", "Col", "val1"));
        stream.Write(new DataTableCellUpdate("row2", "Col", "val2"));

        Assert.Empty(messages);

        StreamRegistry.NotifySubscribed("test-stream");

        Assert.Equal(2, messages.Count);
    }

    [Fact]
    public void WriteStream_DisposedStreamDropsWrites()
    {
        var messages = new List<(string method, object? data)>();
        var sender = new CapturingSender(messages);
        var stream = new WriteStream<DataTableCellUpdate>("test-stream", sender, buffer: false);

        stream.Dispose();
        stream.Write(new DataTableCellUpdate("row1", "Col", "val"));

        Assert.Empty(messages);
    }

    [Fact]
    public void WriteStream_SerializesUpdateAsJson()
    {
        var messages = new List<(string method, object? data)>();
        var sender = new CapturingSender(messages);
        var stream = new WriteStream<DataTableCellUpdate>("test-stream", sender, buffer: false);

        stream.Write(new DataTableCellUpdate("row1", "Status", "Active"));

        Assert.Single(messages);
        var json = JsonSerializer.Serialize(messages[0].data);
        Assert.Contains("test-stream", json);
        Assert.Contains("rowId", json);
        Assert.Contains("Status", json);
    }

    [Fact]
    public void MergedObservables_WriteToStream()
    {
        var messages = new List<(string method, object? data)>();
        var sender = new CapturingSender(messages);
        var stream = new WriteStream<DataTableCellUpdate>("test-stream", sender, buffer: false);

        var subject1 = new Subject<DataTableCellUpdate>();
        var subject2 = new Subject<DataTableCellUpdate>();

        var merged = System.Reactive.Linq.Observable.Merge(subject1, subject2);
        using var subscription = merged.Subscribe(data => stream.Write(data));

        subject1.OnNext(new DataTableCellUpdate("row1", "Status", "Active"));
        subject2.OnNext(new DataTableCellUpdate("row2", "Progress", 50));

        Assert.Equal(2, messages.Count);
    }

    [Fact]
    public void MergedObservables_DisposalStopsWrites()
    {
        var messages = new List<(string method, object? data)>();
        var sender = new CapturingSender(messages);
        var stream = new WriteStream<DataTableCellUpdate>("test-stream", sender, buffer: false);

        var subject = new Subject<DataTableCellUpdate>();
        var merged = System.Reactive.Linq.Observable.Merge(subject);
        var subscription = merged.Subscribe(data => stream.Write(data));

        subject.OnNext(new DataTableCellUpdate("row1", "Col", "val"));
        Assert.Single(messages);

        subscription.Dispose();
        subject.OnNext(new DataTableCellUpdate("row2", "Col", "val"));
        Assert.Single(messages);
    }

    private class CapturingSender(List<(string method, object? data)> messages) : IClientSender
    {
        public void Send(string method, object? data) => messages.Add((method, data));
    }
}
