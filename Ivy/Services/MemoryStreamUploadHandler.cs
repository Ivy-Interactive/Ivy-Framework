using System.Collections.Immutable;
using System.Text;
using Ivy.Core.Hooks;

namespace Ivy.Services;

public class MemoryStreamUploadHandler<T> : IUploadHandler
{
    private readonly IFileUploadSink<T> _sink;
    private readonly int _chunkSize;
    private readonly Func<byte[], T> _converter;

    private MemoryStreamUploadHandler(IFileUploadSink<T> sink, Func<byte[], T> converter, int chunkSize = 8192)
    {
        _sink = sink;
        _chunkSize = chunkSize;
        _converter = converter;
    }

    public static IUploadHandler Create(IState<FileUpload<byte[]>?> singleState, int chunkSize = 8192)
        => new MemoryStreamUploadHandler<byte[]>(new SingleFileSink<byte[]>(singleState), bytes => bytes, chunkSize);

    public static IUploadHandler Create(IState<FileUpload<string>?> singleState, Encoding? encoding = null, int chunkSize = 8192)
        => new MemoryStreamUploadHandler<string>(
            new SingleFileSink<string>(singleState),
            bytes => (encoding ?? Encoding.UTF8).GetString(bytes),
            chunkSize);

    public static IUploadHandler Create(IState<ImmutableArray<FileUpload<byte[]>>> manyState, int chunkSize = 8192)
        => new MemoryStreamUploadHandler<byte[]>(new MultipleFileSink<byte[]>(manyState), bytes => bytes, chunkSize);

    public static IUploadHandler Create(IState<ImmutableArray<FileUpload<string>>> manyState, Encoding? encoding = null, int chunkSize = 8192)
        => new MemoryStreamUploadHandler<string>(
            new MultipleFileSink<string>(manyState),
            bytes => (encoding ?? Encoding.UTF8).GetString(bytes),
            chunkSize);


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
                cancellationToken
            );

            var content = _converter(bytes);
            _sink.Complete(key, content);
        }
        catch (OperationCanceledException)
        {
            _sink.Aborted(key);
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
        CancellationToken ct)
    {
        using var memoryStream = new MemoryStream();
        var buffer = new byte[chunkSize];
        long processedBytes = 0L;
        int bytesRead;
        float lastReportedProgress = 0f;
        const float progressThreshold = 0.05f; // Only report every 5%

        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
        {
            ct.ThrowIfCancellationRequested();
            await memoryStream.WriteAsync(buffer, 0, bytesRead, ct);
            processedBytes += bytesRead;
            var progress = totalLength > 0 ? (float)processedBytes / totalLength : 0f;

            // Only report progress if it changed by at least 5%
            if (progress - lastReportedProgress >= progressThreshold)
            {
                onProgress(progress);
                lastReportedProgress = progress;
            }
        }

        return (memoryStream.ToArray(), processedBytes);
    }
}