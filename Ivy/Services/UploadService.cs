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

/// <summary>
/// Context for an upload endpoint created by UseUpload, providing the client-facing URL
/// and a server-side cancel function to abort an in-flight upload by fileId.
/// </summary>
public record UploadContext(string UploadUrl, Action<Guid> Cancel)
{
    /// <summary>Gets or sets the accepted file types using MIME types or file extensions.</summary>
    public string? Accept { get; init; }

    /// <summary>Gets or sets the maximum file size in bytes.</summary>
    public long? MaxFileSize { get; init; }

    /// <summary>Gets or sets the maximum number of files that can be uploaded.</summary>
    public int? MaxFiles { get; init; }
}

/// <summary>
/// Extension methods for configuring UploadContext.
/// </summary>
public static class UploadContextExtensions
{
    /// <summary>Sets the accepted file types for the upload state using MIME types or file extensions.</summary>
    /// <param name="state">The upload context state to configure.</param>
    /// <param name="accept">A comma-separated list of accepted file types (e.g., "image/*", ".pdf,.doc", "text/plain").</param>
    public static Core.Hooks.IState<UploadContext> Accept(this Core.Hooks.IState<UploadContext> state, string accept)
    {
        state.Set(state.Value with { Accept = accept });
        return state;
    }

    /// <summary>Sets the maximum file size in bytes for the upload state.</summary>
    /// <param name="state">The upload context state to configure.</param>
    /// <param name="maxFileSize">The maximum file size in bytes.</param>
    public static Core.Hooks.IState<UploadContext> MaxFileSize(this Core.Hooks.IState<UploadContext> state, long maxFileSize)
    {
        state.Set(state.Value with { MaxFileSize = maxFileSize });
        return state;
    }

    /// <summary>Sets the maximum number of files that can be uploaded for the upload state.</summary>
    /// <param name="state">The upload context state to configure.</param>
    /// <param name="maxFiles">The maximum number of files allowed.</param>
    public static Core.Hooks.IState<UploadContext> MaxFiles(this Core.Hooks.IState<UploadContext> state, int maxFiles)
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

/// <summary>
/// Common contract for uploaded file metadata used by both generic and non-generic file upload records.
/// </summary>
public interface IFileUpload
{
    Guid Id { get; }
    string FileName { get; }
    string ContentType { get; }
    long Length { get; }
    float Progress { get; set; }
    FileUploadStatus Status { get; set; }
}

/// <summary>
/// Represents a file uploaded through a file input control.
/// </summary>
public record FileUpload : IFileUpload
{
    /// <summary>Gets the identifier for this file upload, set by the server.</summary>
    public Guid Id { get; init; }

    /// <summary>Gets the name of the uploaded file including its extension.</summary>
    public string FileName { get; init; } = string.Empty;

    /// <summary>Gets the MIME type of the uploaded file.</summary>
    public string ContentType { get; init; } = string.Empty;

    /// <summary>Gets the size of the uploaded file in bytes.</summary>
    public long Length { get; init; }

    /// <summary>
    /// Value from 0.0 to 1.0 indicating upload progress.
    /// </summary>
    public float Progress { get; set; } = 0.0f;

    /// <summary>
    /// Gets the current state of the file upload.
    /// </summary>
    public FileUploadStatus Status { get; set; } = FileUploadStatus.Pending;
}

/// <summary>
/// Generic variant of FileUpload allowing an associated typed payload to be tracked alongside the upload metadata.
/// </summary>
/// <typeparam name="T">The type of the associated payload.</typeparam>
public record FileUpload<T> : FileUpload
{
    /// <summary>
    /// Optional typed content associated with this upload (e.g., raw bytes, parsed model).
    /// </summary>
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

/// <summary>
/// Interface for handling file uploads with custom logic.
/// </summary>
public interface IUploadHandler
{
    /// <summary>
    /// Handles the file upload asynchronously.
    /// </summary>
    /// <param name="fileUpload">The file upload metadata.</param>
    /// <param name="stream">The file content stream.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
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

/// <summary>
/// Delegate for handling file uploads with stream and cancellation support.
/// </summary>
public delegate Task UploadDelegate(FileUpload fileUpload, Stream stream, CancellationToken cancellationToken);


/// <summary>
/// Interface for managing file upload state.
/// Sinks are simple state controllers that update FileUpload state for single or multiple files.
/// Cleanup logic should be handled by the upload handler, not the sink.
/// </summary>
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

    /// <summary>
    /// Requests cancellation of an in-flight upload by its fileId.
    /// Safe to call if no upload is in-flight for the given id.
    /// </summary>
    void Cancel(Guid fileId);
}
