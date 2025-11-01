using System.Collections.Immutable;
using Ivy.Core.Hooks;

namespace Ivy.Services;

public class MemoryStreamUploadHandler : IUploadHandler
{
    private readonly IFileUploadSink<byte[]> _sink;
    private readonly int _chunkSize;

    // Backwards-compatible constructor for single-file state
    public MemoryStreamUploadHandler(IState<FileUpload<byte[]>?> singleState, int chunkSize = 8192)
        : this(new SingleFileSink(singleState), chunkSize)
    { }

    // Internal constructor used by factory methods
    private MemoryStreamUploadHandler(IFileUploadSink<byte[]> sink, int chunkSize = 8192)
    {
        _sink = sink;
        _chunkSize = chunkSize;
    }

    // Factory for single state
    public static IUploadHandler Create(IState<FileUpload<byte[]>?> singleState, int chunkSize = 8192)
        => new MemoryStreamUploadHandler(singleState, chunkSize);

    // Factory for ImmutableArray multi state
    public static IUploadHandler Create(IState<ImmutableArray<FileUpload<byte[]>>> manyState, int chunkSize = 8192)
        => new MemoryStreamUploadHandler(new ImmutableArraySink(manyState), chunkSize);


    public async Task HandleUploadAsync(FileUpload fileUpload, Stream stream, CancellationToken cancellationToken)
    {
        Guid key = fileUpload.Id;
        try
        {
            Console.WriteLine($"[UploadHandler] Start reading fileId={fileUpload.Id} name='{fileUpload.FileName}' length={fileUpload.Length}");

            key = _sink.Start(fileUpload);

            var (bytes, processedBytes) = await ReadAllWithProgressAsync(
                stream,
                _chunkSize,
                fileUpload.Length,
                p => _sink.Progress(key, p),
                cancellationToken
            );

            _sink.Complete(key, bytes);
            Console.WriteLine($"[UploadHandler] Finished fileId={fileUpload.Id} bytes={processedBytes}");
        }
        catch (OperationCanceledException)
        {
            _sink.Aborted(key);
            Console.WriteLine($"[UploadHandler] Aborted fileId={fileUpload.Id}");
        }
        catch (Exception)
        {
            _sink.Failed(key);
            Console.WriteLine($"[UploadHandler] Failed fileId={fileUpload.Id}");
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

    private interface IFileUploadSink<in TContent>
    {
        Guid Start(FileUpload file);
        void Progress(Guid key, float progress);
        void Complete(Guid key, TContent content);
        void Aborted(Guid key);
        void Failed(Guid key);
    }

    private sealed class SingleFileSink : IFileUploadSink<byte[]>
    {
        private readonly IState<FileUpload<byte[]>?> _state;
        public SingleFileSink(IState<FileUpload<byte[]>?> state) => _state = state;

        public Guid Start(FileUpload file)
        {
            var typed = new FileUpload<byte[]>
            {
                Id = file.Id,
                FileName = file.FileName,
                ContentType = file.ContentType,
                Length = file.Length,
                Status = FileUploadStatus.Loading,
                Progress = 0f
            };
            _state.Set(typed);
            return file.Id;
        }

        public void Progress(Guid key, float progress)
        {
            _state.SetProgress(progress);
        }

        public void Complete(Guid key, byte[] content)
        {
            var current = _state.Value;
            if (current != null && current.Id == key)
            {
                _state.Set(current with { Content = content, Status = FileUploadStatus.Finished, Progress = 1f });
            }
            else if (current == null)
            {
                // Fallback if state was cleared
                _state.Set(new FileUpload<byte[]>
                {
                    Id = key,
                    Content = content,
                    Status = FileUploadStatus.Finished,
                    Progress = 1f
                });
            }
        }

        public void Aborted(Guid key) => _state.SetStatus(FileUploadStatus.Aborted);
        public void Failed(Guid key) => _state.SetStatus(FileUploadStatus.Failed);
    }

    private sealed class ImmutableArraySink : IFileUploadSink<byte[]>
    {
        private readonly IState<ImmutableArray<FileUpload<byte[]>>> _state;
        public ImmutableArraySink(IState<ImmutableArray<FileUpload<byte[]>>> state) => _state = state;

        private static int IndexOfById(ImmutableArray<FileUpload<byte[]>> list, Guid key)
        {
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i].Id == key) return i;
            }
            return -1;
        }

        public Guid Start(FileUpload file)
        {
            var typed = new FileUpload<byte[]>
            {
                Id = file.Id,
                FileName = file.FileName,
                ContentType = file.ContentType,
                Length = file.Length,
                Status = FileUploadStatus.Loading,
                Progress = 0f
            };
            Console.WriteLine($"[ImmutableArraySink] Start: Adding file {file.Id} to state");
            _state.Set(list => list.Add(typed));
            Console.WriteLine($"[ImmutableArraySink] Start: State updated, current count: {_state.Value.Length}");
            return file.Id;
        }

        public void Progress(Guid key, float progress)
        {
            Console.WriteLine($"[ImmutableArraySink] Progress: File {key}, progress: {progress:P0}");
            var oldValue = _state.Value;
            var newValue = _state.Set(list =>
            {
                var idx = IndexOfById(list, key);
                if (idx >= 0)
                {
                    var updated = list[idx] with { Progress = progress };
                    return list.SetItem(idx, updated);
                }
                return list;
            });
            var actuallyChanged = oldValue != newValue;
            Console.WriteLine($"[ImmutableArraySink] Progress: State changed? {actuallyChanged}, old count: {oldValue.Length}, new count: {newValue.Length}");
            if (newValue.Length > 0)
            {
                Console.WriteLine($"[ImmutableArraySink] Progress: First item progress after update: {newValue[0].Progress:P0}, status: {newValue[0].Status}");
            }
        }

        public void Complete(Guid key, byte[] content)
        {
            Console.WriteLine($"[ImmutableArraySink] Complete: File {key}, content length: {content.Length}");
            _state.Set(list =>
            {
                var idx = IndexOfById(list, key);
                Console.WriteLine($"[ImmutableArraySink] Complete: Found file at index {idx}");
                if (idx >= 0)
                {
                    var updated = list[idx] with { Content = content, Status = FileUploadStatus.Finished, Progress = 1f };
                    Console.WriteLine($"[ImmutableArraySink] Complete: Updated file status={updated.Status}, progress={updated.Progress}");
                    return list.SetItem(idx, updated);
                }
                Console.WriteLine($"[ImmutableArraySink] Complete: File not found in list!");
                return list;
            });
            Console.WriteLine($"[ImmutableArraySink] Complete: State updated, current count: {_state.Value.Length}");
        }

        public void Aborted(Guid key)
        {
            _state.Set(list =>
            {
                var idx = IndexOfById(list, key);
                if (idx >= 0)
                {
                    var updated = list[idx] with { Status = FileUploadStatus.Aborted };
                    return list.SetItem(idx, updated);
                }
                return list;
            });
        }

        public void Failed(Guid key)
        {
            _state.Set(list =>
            {
                var idx = IndexOfById(list, key);
                if (idx >= 0)
                {
                    var updated = list[idx] with { Status = FileUploadStatus.Failed };
                    return list.SetItem(idx, updated);
                }
                return list;
            });
        }
    }

}
