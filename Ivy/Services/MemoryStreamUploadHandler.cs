using System.Collections.Immutable;
using System.Text;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;

namespace Ivy.Services;

public static class MemoryStreamUploadHandler
{
    public static IUploadHandler Create(IAnyState anyState, Encoding? encoding = null, int chunkSize = 8192, float progressThreshold = 0.05f)
    {
        var stateType = anyState.GetStateType();

        // Handle nullable value types - unwrap to get the underlying type
        var underlyingType = Nullable.GetUnderlyingType(stateType) ?? stateType;

        // Check for FileUpload<byte[]>
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

        // Check for ImmutableArray<FileUpload<T>>
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
            $@"Unsupported state type: {stateType}. Supported types are: FileUpload<byte[]>?, FileUpload<string>?, ImmutableArray<FileUpload<byte[]>>, ImmutableArray<FileUpload<string>>",
            nameof(anyState));
    }

    public static IUploadHandler Create(IState<FileUpload<byte[]>?> singleState, int chunkSize = 8192, float progressThreshold = 0.05f)
        => new MemoryStreamUploadHandlerImpl<byte[]>(new SingleFileSink<byte[]>(singleState), bytes => bytes, chunkSize, progressThreshold);

    public static IUploadHandler Create(IState<FileUpload<string>?> singleState, Encoding? encoding = null, int chunkSize = 8192, float progressThreshold = 0.05f)
        => new MemoryStreamUploadHandlerImpl<string>(
            new SingleFileSink<string>(singleState),
            bytes => (encoding ?? Encoding.UTF8).GetString(bytes),
            chunkSize,
            progressThreshold);

    public static IUploadHandler Create(IState<ImmutableArray<FileUpload<byte[]>>> manyState, int chunkSize = 8192, float progressThreshold = 0.05f)
        => new MemoryStreamUploadHandlerImpl<byte[]>(new MultipleFileSink<byte[]>(manyState), bytes => bytes, chunkSize, progressThreshold);

    public static IUploadHandler Create(IState<ImmutableArray<FileUpload<string>>> manyState, Encoding? encoding = null, int chunkSize = 8192, float progressThreshold = 0.05f)
        => new MemoryStreamUploadHandlerImpl<string>(
            new MultipleFileSink<string>(manyState),
            bytes => (encoding ?? Encoding.UTF8).GetString(bytes),
            chunkSize,
            progressThreshold);
}

internal sealed class MemoryStreamUploadHandlerImpl<T> : IUploadHandler
{
    private readonly IFileUploadSink<T> _sink;
    private readonly int _chunkSize;
    private readonly Func<byte[], T> _converter;
    private readonly float _progressThreshold;

    internal MemoryStreamUploadHandlerImpl(IFileUploadSink<T> sink, Func<byte[], T> converter, int chunkSize = 8192, float progressThreshold = 0.05f)
    {
        _sink = sink;
        _chunkSize = chunkSize;
        _converter = converter;
        _progressThreshold = progressThreshold;
    }


    public async Task HandleUploadAsync(FileUpload fileUpload, Stream stream, CancellationToken cancellationToken)
    {
        Guid key = fileUpload.Id;
        try
        {
            key = _sink.Start(fileUpload);

            var (bytes, _) = await ReadAllWithProgressAsync(
                stream,
                _chunkSize,
                fileUpload.Length,
                p => _sink.Progress(key, p),
                _progressThreshold,
                cancellationToken
            );

            var content = _converter(bytes);
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

    private static async Task<(byte[] bytes, long totalRead)> ReadAllWithProgressAsync(
        Stream stream,
        int chunkSize,
        long totalLength,
        Action<float> onProgress,
        float progressThreshold,
        CancellationToken ct)
    {
        using var memoryStream = new MemoryStream();
        var buffer = new byte[chunkSize];
        long processedBytes = 0L;
        int bytesRead;
        float lastReportedProgress = 0f;

        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
        {
            ct.ThrowIfCancellationRequested();
            await memoryStream.WriteAsync(buffer, 0, bytesRead, ct);
            processedBytes += bytesRead;
            var progress = totalLength > 0 ? (float)processedBytes / totalLength : 0f;

            // Only report progress if it changed by the configured threshold
            if (progress - lastReportedProgress >= progressThreshold)
            {
                onProgress(progress);
                lastReportedProgress = progress;
            }
        }

        return (memoryStream.ToArray(), processedBytes);
    }
}