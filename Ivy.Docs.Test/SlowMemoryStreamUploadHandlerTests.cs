using System.Diagnostics;
using System.IO;
using System.Linq;
using Ivy.Core.Hooks;
using Ivy.Docs.Shared.Helpers;
using Ivy.Services;

namespace Ivy.Docs.Test;

public class SlowMemoryStreamUploadHandlerTests
{
    [Fact]
    public async Task HandleUploadAsync_ThrottlesToOneMegabytePerSecond()
    {
        var dataLength = 2 * 1024 * 1024; // 2 MB
        var data = Enumerable.Repeat<byte>(0x2A, dataLength).ToArray();
        await using var stream = new MemoryStream(data);

        using var state = new State<FileUpload<byte[]>?>(null);
        var handler = SlowMemoryStreamUploadHandler.Create(state);
        var upload = new FileUpload
        {
            Id = Guid.NewGuid(),
            FileName = "demo.bin",
            ContentType = "application/octet-stream",
            Length = dataLength
        };

        var stopwatch = Stopwatch.StartNew();
        await handler.HandleUploadAsync(upload, stream, CancellationToken.None);
        stopwatch.Stop();

        Assert.True(stopwatch.Elapsed >= TimeSpan.FromSeconds(1.8));

        var stored = state.Value;
        Assert.NotNull(stored);
        Assert.Equal(FileUploadStatus.Finished, stored!.Status);
        Assert.Equal(1f, stored.Progress);
        Assert.NotNull(stored.Content);
        Assert.Equal(dataLength, stored.Content!.Length);
        Assert.True(stored.Content.SequenceEqual(data));
    }

    [Fact]
    public async Task HandleUploadAsync_CancellationAbortsUpload()
    {
        var dataLength = 2 * 1024 * 1024; // 2 MB
        var data = Enumerable.Repeat<byte>(0x7F, dataLength).ToArray();
        await using var stream = new MemoryStream(data);

        using var state = new State<FileUpload<byte[]>?>(null);
        var handler = SlowMemoryStreamUploadHandler.Create(state);
        var upload = new FileUpload
        {
            Id = Guid.NewGuid(),
            FileName = "cancel.bin",
            ContentType = "application/octet-stream",
            Length = dataLength
        };

        using var cts = new CancellationTokenSource(millisecondsDelay: 100);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            handler.HandleUploadAsync(upload, stream, cts.Token));

        var stored = state.Value;
        Assert.NotNull(stored);
        Assert.Equal(FileUploadStatus.Aborted, stored!.Status);
        Assert.True(stored.Progress <= 1f);
    }
}

