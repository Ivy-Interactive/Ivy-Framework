using System.Buffers;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;

using Ivy.Core.Helpers;
using Ivy.Services;

namespace Ivy.Docs.Shared.Helpers;

public static class SlowMemoryStreamUploadHandler
{
    public static IUploadHandler Create(
        IAnyState anyState,
        Encoding? encoding = null,
        int chunkSize = 8192,
        float progressThreshold = 0.05f)
    {
        var stateType = anyState.GetStateType();
        var underlyingType = Nullable.GetUnderlyingType(stateType) ?? stateType;

        if (underlyingType.IsGenericType && underlyingType.GetGenericTypeDefinition() == typeof(FileUpload<>))
        {
            var contentType = underlyingType.GetGenericArguments()[0];

            if (contentType == typeof(byte[]))
            {
                return Create(anyState.As<FileUpload<byte[]>?>(), chunkSize, progressThreshold);
            }

            if (contentType == typeof(string))
            {
                return Create(anyState.As<FileUpload<string>?>(), encoding, chunkSize, progressThreshold);
            }
        }

        if (underlyingType.IsGenericType && underlyingType.GetGenericTypeDefinition() == typeof(ImmutableArray<>))
        {
            var elementType = underlyingType.GetGenericArguments()[0];

            if (elementType.IsGenericType && elementType.GetGenericTypeDefinition() == typeof(FileUpload<>))
            {
                var contentType = elementType.GetGenericArguments()[0];

                if (contentType == typeof(byte[]))
                {
                    return Create(anyState.As<ImmutableArray<FileUpload<byte[]>>>(), chunkSize, progressThreshold);
                }

                if (contentType == typeof(string))
                {
                    return Create(anyState.As<ImmutableArray<FileUpload<string>>>(), encoding, chunkSize, progressThreshold);
                }
            }
        }

        throw new ArgumentException(
            $@"Unsupported state type: {stateType}. Supported types are: FileUpload<byte[]>?, FileUpload<string>?, ImmutableArray<FileUpload<byte[]>>, ImmutableArray<FileUpload<string>>>",
            nameof(anyState));
    }

    public static IUploadHandler Create(
        IState<FileUpload<byte[]>?> singleState,
        int chunkSize = 8192,
        float progressThreshold = 0.05f)
        => new SlowMemoryStreamUploadHandlerImpl<byte[]>(
            new SingleFileSink<byte[]>(singleState),
            bytes => bytes,
            chunkSize,
            progressThreshold);

    public static IUploadHandler Create(
        IState<FileUpload<string>?> singleState,
        Encoding? encoding = null,
        int chunkSize = 8192,
        float progressThreshold = 0.05f)
        => new SlowMemoryStreamUploadHandlerImpl<string>(
            new SingleFileSink<string>(singleState),
            bytes => (encoding ?? Encoding.UTF8).GetString(bytes),
            chunkSize,
            progressThreshold);

    public static IUploadHandler Create(
        IState<ImmutableArray<FileUpload<byte[]>>> manyState,
        int chunkSize = 8192,
        float progressThreshold = 0.05f)
        => new SlowMemoryStreamUploadHandlerImpl<byte[]>(
            new MultipleFileSink<byte[]>(manyState),
            bytes => bytes,
            chunkSize,
            progressThreshold);

    public static IUploadHandler Create(
        IState<ImmutableArray<FileUpload<string>>> manyState,
        Encoding? encoding = null,
        int chunkSize = 8192,
        float progressThreshold = 0.05f)
        => new SlowMemoryStreamUploadHandlerImpl<string>(
            new MultipleFileSink<string>(manyState),
            bytes => (encoding ?? Encoding.UTF8).GetString(bytes),
            chunkSize,
            progressThreshold);
}

internal sealed class SlowMemoryStreamUploadHandlerImpl<T>(
    IFileUploadSink<T> sink,
    Func<byte[], T> converter,
    int chunkSize = 8192,
    float progressThreshold = 0.05f) : IUploadHandler
{
    private const int BytesPerSecond = 1 * 1024 * 1024;

    private readonly IFileUploadSink<T> _sink = sink;
    private readonly Func<byte[], T> _converter = converter;
    private readonly int _chunkSize = chunkSize;
    private readonly float _progressThreshold = progressThreshold;

    public async Task HandleUploadAsync(FileUpload fileUpload, Stream stream, CancellationToken cancellationToken)
    {
        Guid key = fileUpload.Id;
        try
        {
            key = _sink.Start(fileUpload);

            var result = await ReadAllWithProgressAndThrottleAsync(
                stream,
                _chunkSize,
                fileUpload.Length,
                BytesPerSecond,
                p => _sink.Progress(key, p),
                _progressThreshold,
                cancellationToken);

            var content = _converter(result.bytes);
            _sink.Complete(key, content);
        }
        catch (OperationCanceledException)
        {
            _sink.Aborted(key);
            throw;
        }
        catch (Exception)
        {
            _sink.Failed(key);
            throw;
        }
    }

    private static async Task<(byte[] bytes, long totalRead)> ReadAllWithProgressAndThrottleAsync(
        Stream stream,
        int chunkSize,
        long totalLength,
        int bytesPerSecond,
        Action<float> onProgress,
        float progressThreshold,
        CancellationToken ct)
    {
        using var memoryStream = new MemoryStream();
        byte[] buffer = ArrayPool<byte>.Shared.Rent(chunkSize);
        try
        {
            var stopwatch = Stopwatch.StartNew();
            long processedBytes = 0;
            float lastReportedProgress = 0f;

            int bytesRead;
            while ((bytesRead = await stream.ReadAsync(buffer.AsMemory(0, chunkSize), ct)) > 0)
            {
                ct.ThrowIfCancellationRequested();
                await memoryStream.WriteAsync(buffer.AsMemory(0, bytesRead), ct);
                processedBytes += bytesRead;

                if (totalLength > 0)
                {
                    var progress = (float)processedBytes / totalLength;
                    if (progress - lastReportedProgress >= progressThreshold)
                    {
                        onProgress(progress);
                        lastReportedProgress = progress;
                    }
                }

                await ThrottleAsync(stopwatch, processedBytes, bytesPerSecond, ct);
            }

            if (totalLength <= 0 && processedBytes > 0)
            {
                onProgress(1f);
            }

            return (memoryStream.ToArray(), processedBytes);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private static async Task ThrottleAsync(
        Stopwatch stopwatch,
        long processedBytes,
        int bytesPerSecond,
        CancellationToken cancellationToken)
    {
        if (bytesPerSecond <= 0)
        {
            return;
        }

        double expectedMilliseconds = (processedBytes / (double)bytesPerSecond) * 1000.0;
        double elapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds;

        if (expectedMilliseconds <= elapsedMilliseconds)
        {
            return;
        }

        var delay = TimeSpan.FromMilliseconds(expectedMilliseconds - elapsedMilliseconds);
        if (delay > TimeSpan.Zero)
        {
            await Task.Delay(delay, cancellationToken);
        }
    }
}

