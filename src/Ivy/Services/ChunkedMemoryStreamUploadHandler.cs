using System.Collections.Immutable;
using System.Text;
using Ivy.Core.Hooks;

namespace Ivy.Services;

public static class ChunkedMemoryStreamUploadHandler
{
    public static IUploadHandler Create(IState<FileUpload<byte[]>?> singleState, int chunkSize = 8192)
        => new ChunkedMemoryStreamUploadHandlerImpl(singleState, chunkSize);

    public static IUploadHandler CreateArray(IState<ImmutableArray<FileUpload<byte[]>>> arrayState, int chunkSize = 8192)
        => new ChunkedArrayUploadHandlerImpl(arrayState, chunkSize);
}

internal sealed class ChunkedMemoryStreamUploadHandlerImpl : IUploadHandler
{
    private readonly IState<FileUpload<byte[]>?> _state;
    private readonly int _chunkSize;
    private long _totalAccumulated;

    internal ChunkedMemoryStreamUploadHandlerImpl(IState<FileUpload<byte[]>?> state, int chunkSize = 8192)
    {
        _state = state;
        _chunkSize = chunkSize;
        _totalAccumulated = 0;
    }

    public async Task HandleUploadAsync(FileUpload fileUpload, Stream stream, CancellationToken cancellationToken)
    {
        try
        {
            // Read the new chunk
            var chunkBytes = await ReadChunkAsync(stream, _chunkSize, cancellationToken);

            var current = _state.Value;
            byte[] newContent;

            if (current?.Content != null)
            {
                // Append to existing content
                newContent = new byte[current.Content.Length + chunkBytes.Length];
                Array.Copy(current.Content, 0, newContent, 0, current.Content.Length);
                Array.Copy(chunkBytes, 0, newContent, current.Content.Length, chunkBytes.Length);
                _totalAccumulated = newContent.Length;
            }
            else
            {
                // First chunk - start new file
                newContent = chunkBytes;
                _totalAccumulated = chunkBytes.Length;
            }

            // Update state with accumulated content
            var updated = new FileUpload<byte[]>
            {
                Id = current?.Id ?? fileUpload.Id,
                FileName = fileUpload.FileName,
                ContentType = fileUpload.ContentType,
                Length = _totalAccumulated,
                Content = newContent,
                Status = FileUploadStatus.Loading,
                Progress = 1.0f // Each chunk completes fully
            };

            _state.Set(updated);
        }
        catch (OperationCanceledException)
        {
            var current = _state.Value;
            if (current != null)
            {
                _state.Set(current with { Status = FileUploadStatus.Aborted });
            }
            throw;
        }
        catch (Exception)
        {
            var current = _state.Value;
            if (current != null)
            {
                _state.Set(current with { Status = FileUploadStatus.Failed });
            }
            throw;
        }
    }

    private static async Task<byte[]> ReadChunkAsync(Stream stream, int chunkSize, CancellationToken ct)
    {
        using var memoryStream = new MemoryStream();
        var buffer = new byte[chunkSize];
        int bytesRead;

        while ((bytesRead = await stream.ReadAsync(buffer, ct)) > 0)
        {
            ct.ThrowIfCancellationRequested();
            await memoryStream.WriteAsync(buffer.AsMemory(0, bytesRead), ct);
        }

        return memoryStream.ToArray();
    }
}

internal sealed class ChunkedArrayUploadHandlerImpl : IUploadHandler
{
    private readonly IState<ImmutableArray<FileUpload<byte[]>>> _state;
    private readonly int _chunkSize;

    internal ChunkedArrayUploadHandlerImpl(IState<ImmutableArray<FileUpload<byte[]>>> state, int chunkSize = 8192)
    {
        _state = state;
        _chunkSize = chunkSize;
    }

    public async Task HandleUploadAsync(FileUpload fileUpload, Stream stream, CancellationToken cancellationToken)
    {
        // Read the chunk
        var chunkBytes = await ReadChunkAsync(stream, _chunkSize, cancellationToken);

        // Add as new entry in array
        var chunk = new FileUpload<byte[]>
        {
            Id = fileUpload.Id,
            FileName = fileUpload.FileName,
            ContentType = fileUpload.ContentType,
            Length = chunkBytes.Length,
            Content = chunkBytes,
            Status = FileUploadStatus.Finished,
            Progress = 1.0f
        };

        _state.Set(list => list.Add(chunk));
    }

    private static async Task<byte[]> ReadChunkAsync(Stream stream, int chunkSize, CancellationToken ct)
    {
        using var memoryStream = new MemoryStream();
        var buffer = new byte[chunkSize];
        int bytesRead;

        while ((bytesRead = await stream.ReadAsync(buffer, ct)) > 0)
        {
            ct.ThrowIfCancellationRequested();
            await memoryStream.WriteAsync(buffer.AsMemory(0, bytesRead), ct);
        }

        return memoryStream.ToArray();
    }
}
