using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Reactive.Disposables;
using System.Text.Json.Serialization;
using Ivy.Client;
using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Views.Builders;
using Ivy.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Ivy.Services;

public record UploadContext(string UploadUrl, Action<Guid> Cancel)
{
    public string? Accept { get; init; }

    public long? MaxFileSize { get; init; }

    public int? MaxFiles { get; init; }
}

public static class UploadContextExtensions
{
    public static IState<UploadContext> Accept(this IState<UploadContext> state, string accept)
    {
        state.Set(state.Value with { Accept = accept });
        return state;
    }

    public static IState<UploadContext> MaxFileSize(this IState<UploadContext> state, long maxFileSize)
    {
        state.Set(state.Value with { MaxFileSize = maxFileSize });
        return state;
    }

    public static IState<UploadContext> MaxFiles(this IState<UploadContext> state, int maxFiles)
    {
        state.Set(state.Value with { MaxFiles = maxFiles });
        return state;
    }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FileUploadStatus
{
    Pending,
    Aborted,
    Loading,
    Failed,
    Finished
}

public interface IFileUpload
{
    Guid Id { get; }
    string FileName { get; }
    string ContentType { get; }
    long Length { get; }
    float Progress { get; set; }
    FileUploadStatus Status { get; set; }
}

public record FileUpload : IFileUpload
{
    public Guid Id { get; init; }

    public string FileName { get; init; } = string.Empty;

    public string ContentType { get; init; } = string.Empty;

    public long Length { get; init; }

    public float Progress { get; set; } = 0.0f;

    public FileUploadStatus Status { get; set; } = FileUploadStatus.Pending;
}

public record FileUpload<T> : FileUpload
{
    [JsonIgnore]
    [ScaffoldColumn(false)]
    public T? Content { get; init; }

    public DetailsBuilder<FileUpload<T>> ToDetails()
    {
        return new DetailsBuilder<FileUpload<T>>(this)
            .Builder(e => e.Length, e => e.Func((long x) => Utils.FormatBytes(x)))
            .Builder(e => e.Progress, e => e.Func((float x) => x.ToString("P0")))
            .Remove(e => e.Id);
    }
}

public interface IUploadHandler
{
    Task HandleUploadAsync(FileUpload fileUpload, Stream stream, CancellationToken cancellationToken);
}

public static class FileUploadExtensions
{
    public static void SetProgress<T>(this IState<FileUpload<T>?> fileInputState, float progress)
    {
        var file = fileInputState.Value;
        if (file != null)
        {
            fileInputState.Set(file with { Progress = progress });
        }
    }

    public static void SetStatus<T>(this IState<FileUpload<T>?> fileInputState, FileUploadStatus status)
    {
        var file = fileInputState.Value;
        if (file != null)
        {
            fileInputState.Set(file with { Status = status });
        }
    }
}

public delegate Task UploadDelegate(FileUpload fileUpload, Stream stream, CancellationToken cancellationToken);


public interface IFileUploadSink<in TContent>
{
    Guid Start(FileUpload file);
    void Progress(Guid key, float progress);
    void Complete(Guid key, TContent content);
    void Aborted(Guid key);
    void Failed(Guid key);
}

public sealed class SingleFileSink<T>(IState<FileUpload<T>?> state) : IFileUploadSink<T>
{
    public Guid Start(FileUpload file)
    {
        var typed = new FileUpload<T>
        {
            Id = file.Id,
            FileName = file.FileName,
            ContentType = file.ContentType,
            Length = file.Length,
            Status = FileUploadStatus.Loading,
            Progress = 0f
        };
        state.Set(typed);
        return file.Id;
    }

    public void Progress(Guid key, float progress)
    {
        state.SetProgress(progress);
    }

    public void Complete(Guid key, T content)
    {
        var current = state.Value;
        if (current != null && current.Id == key)
        {
            state.Set(current with { Content = content, Status = FileUploadStatus.Finished, Progress = 1f });
        }
        else if (current == null)
        {
            // Fallback if state was cleared
            state.Set(new FileUpload<T>
            {
                Id = key,
                Content = content,
                Status = FileUploadStatus.Finished,
                Progress = 1f
            });
        }
    }

    public void Aborted(Guid key) => state.SetStatus(FileUploadStatus.Aborted);
    public void Failed(Guid key) => state.SetStatus(FileUploadStatus.Failed);
}

public sealed class MultipleFileSink<T>(IState<ImmutableArray<FileUpload<T>>> state) : IFileUploadSink<T>
{
    private static int IndexOfById(ImmutableArray<FileUpload<T>> list, Guid key)
    {
        for (int i = 0; i < list.Length; i++)
        {
            if (list[i].Id == key) return i;
        }
        return -1;
    }

    public Guid Start(FileUpload file)
    {
        var typed = new FileUpload<T>
        {
            Id = file.Id,
            FileName = file.FileName,
            ContentType = file.ContentType,
            Length = file.Length,
            Status = FileUploadStatus.Loading,
            Progress = 0f
        };
        state.Set(list => list.Add(typed));
        return file.Id;
    }

    public void Progress(Guid key, float progress)
    {
        state.Set(list =>
        {
            var idx = IndexOfById(list, key);
            if (idx >= 0)
            {
                var updated = list[idx] with { Progress = progress };
                return list.SetItem(idx, updated);
            }
            return list;
        });
    }

    public void Complete(Guid key, T content)
    {
        state.Set(list =>
        {
            var idx = IndexOfById(list, key);
            if (idx >= 0)
            {
                var updated = list[idx] with { Content = content, Status = FileUploadStatus.Finished, Progress = 1f };
                return list.SetItem(idx, updated);
            }
            return list;
        });
    }

    public void Aborted(Guid key)
    {
        state.Set(list =>
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
        state.Set(list =>
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

[ApiController]
[Route("ivy/upload")]
public class UploadController(AppSessionStore sessionStore, Server server) : Controller
{
    [HttpPost("{connectionId}/{uploadId}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload([FromRoute] string connectionId, [FromRoute] string uploadId, [FromForm] IFormFile file)
    {
        if (string.IsNullOrEmpty(connectionId))
        {
            return BadRequest("connectionId is required.");
        }
        if (!sessionStore.Sessions.TryGetValue(connectionId, out var session))
        {
            return NotFound($"Session for connectionId '{connectionId}' not found.");
        }

        if (await this.ValidateAuthIfRequired(server, session.AppServices) is { } errorResult)
        {
            return errorResult;
        }

        if (string.IsNullOrEmpty(uploadId))
        {
            return BadRequest("uploadId is required.");
        }

        var uploadService = session.AppServices.GetRequiredService<IUploadService>();
        return await uploadService.Upload(uploadId, file);
    }
}

public class UploadService(string connectionId, IClientProvider clientProvider) : IUploadService, IDisposable
{
    private readonly ConcurrentDictionary<Guid, (UploadDelegate handler, CancellationTokenSource cts, string? mimeType, string? fileName, Func<(string? accept, long? maxFileSize)> getValidation)> _uploads = new();
    private readonly ConcurrentDictionary<Guid, CancellationTokenSource> _inflightUploads = new();

    public (IDisposable cleanup, string url) AddUpload(UploadDelegate handler, string? defaultContentType = null, string? defaultFileName = null, string? accept = null, long? maxFileSize = null)
    {
        var uploadId = Guid.NewGuid();
        var cts = new CancellationTokenSource();
        _uploads[uploadId] = (handler, cts, defaultContentType, defaultFileName, () => (accept, maxFileSize));

        var cleanup = Disposable.Create(() =>
        {
            _uploads.TryRemove(uploadId, out var upload);
            upload.cts?.Dispose();
        });

        return (cleanup, $"/ivy/upload/{connectionId}/{uploadId}");
    }

    public (IDisposable cleanup, string url) AddUpload(UploadDelegate handler, Func<(string? accept, long? maxFileSize)> getValidation, string? defaultContentType = null, string? defaultFileName = null)
    {
        var uploadId = Guid.NewGuid();
        var cts = new CancellationTokenSource();
        _uploads[uploadId] = (handler, cts, defaultContentType, defaultFileName, getValidation);

        var cleanup = Disposable.Create(() =>
        {
            _uploads.TryRemove(uploadId, out var upload);
            upload.cts?.Dispose();
        });

        return (cleanup, $"/ivy/upload/{connectionId}/{uploadId}");
    }

    public async Task<IActionResult> Upload(string uploadId, IFormFile file)
    {
        if (!Guid.TryParse(uploadId, out var guid) || !_uploads.TryGetValue(guid, out var upload))
        {
            return new BadRequestObjectResult($"Invalid or unknown uploadId: '{uploadId}'.");
        }

        var (handler, cts, defaultContentType, defaultFileName, getValidation) = upload;
        var (accept, maxFileSize) = getValidation();

        if (file.Length == 0)
        {
            return new BadRequestObjectResult("Empty file.");
        }

        var actualMimeType = file.ContentType.NullIfEmpty() ?? defaultContentType ?? "application/octet-stream";
        var actualFileName = file.FileName.NullIfEmpty() ?? defaultFileName ?? "upload";

        // Generate a unique id per uploaded file to avoid collisions
        // when multiple files are uploaded using the same upload endpoint.
        var fileUpload = new FileUpload
        {
            Id = Guid.NewGuid(),
            FileName = actualFileName,
            ContentType = actualMimeType,
            Length = file.Length
        };

        // Validate file size
        if (maxFileSize.HasValue)
        {
            var sizeValidation = Widgets.Inputs.FileInputValidation.ValidateFileSize(fileUpload, maxFileSize);
            if (!sizeValidation.IsValid)
            {
                // Send toast notification for file size error
                clientProvider.Toast(sizeValidation.ErrorMessage ?? "File is too large", "File too large");
                return new OkResult(); // Return OK to prevent frontend error handling
            }
        }

        // Validate file type
        if (!string.IsNullOrWhiteSpace(accept))
        {
            var typeValidation = Widgets.Inputs.FileInputValidation.ValidateFileType(fileUpload, accept);
            if (!typeValidation.IsValid)
            {
                // Send toast notification for file type error
                clientProvider.Toast(typeValidation.ErrorMessage ?? "File type is not allowed", "Invalid file type");
                return new OkResult(); // Return OK to prevent frontend error handling
            }
        }

        // Ensure request stream is disposed deterministically to avoid leaking handles
        try
        {
            await using var uploadStream = file.OpenReadStream();
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, timeoutCts.Token);
            // Track this upload by fileId so we can cancel on demand
            _inflightUploads[fileUpload.Id] = linkedCts;

            await handler(fileUpload, uploadStream, linkedCts.Token);
        }
        finally
        {
            // Remove tracking for this fileId
            _inflightUploads.TryRemove(fileUpload.Id, out _);
        }

        return new OkResult();
    }

    public void Cancel(Guid fileId)
    {
        if (_inflightUploads.TryGetValue(fileId, out var cts))
        {
            try
            {
                cts.Cancel();
            }
            catch (ObjectDisposedException)
            {
                // If already disposed, ignore
            }
        }
    }

    public void Dispose()
    {
        _uploads.Clear();
    }
}

public interface IUploadService
{
    (IDisposable cleanup, string url) AddUpload(UploadDelegate handler, Func<(string? accept, long? maxFileSize)> getValidation, string? defaultContentType = null, string? defaultFileName = null);

    Task<IActionResult> Upload(string uploadId, IFormFile file);

    void Cancel(Guid fileId);
}
