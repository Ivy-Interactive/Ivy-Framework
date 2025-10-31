using System.Collections.Concurrent;
using System.Reactive.Disposables;
using System.Diagnostics;
using System.Text.Json.Serialization;
using Ivy.Core.Hooks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Ivy.Services;

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
    public T? Content { get; init; }
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

[ApiController]
[Route("upload")]
public class UploadController(AppSessionStore sessionStore) : Controller
{
    [HttpPost("{connectionId}/{uploadId}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload([FromRoute] string connectionId, [FromRoute] string uploadId, [FromForm] IFormFile file)
    {
        if (string.IsNullOrEmpty(connectionId))
        {
            return BadRequest("connectionId is required.");
        }
        if (string.IsNullOrEmpty(uploadId))
        {
            return BadRequest("uploadId is required.");
        }
        if (sessionStore.Sessions.TryGetValue(connectionId, out var session))
        {
            var uploadService = session.AppServices.GetRequiredService<IUploadService>();
            return await uploadService.Upload(uploadId, file);
        }
        return NotFound($"Session for connectionId '{connectionId}' not found.");
    }
}

public class UploadService(string connectionId) : IUploadService, IDisposable
{
    private readonly ConcurrentDictionary<Guid, (UploadDelegate handler, CancellationTokenSource cts, string? mimeType, string? fileName)> _uploads = new();
    private readonly ConcurrentDictionary<Guid, CancellationTokenSource> _inflightUploads = new();

    public (IDisposable cleanup, string url) AddUpload(UploadDelegate handler, string? defaultContentType = null, string? defaultFileName = null)
    {
        var uploadId = Guid.NewGuid();
        var cts = new CancellationTokenSource();
        _uploads[uploadId] = (handler, cts, defaultContentType, defaultFileName);

        var cleanup = Disposable.Create(() =>
        {
            _uploads.TryRemove(uploadId, out var upload);
            upload.cts?.Dispose();
        });

        return (cleanup, $"/upload/{connectionId}/{uploadId}");
    }

    public async Task<IActionResult> Upload(string uploadId, IFormFile file)
    {
        var sw = Stopwatch.StartNew();

        if (!Guid.TryParse(uploadId, out var guid) || !_uploads.TryGetValue(guid, out var upload))
        {
            Console.WriteLine($"[UploadService] Invalid or unknown uploadId: '{uploadId}'");
            return new BadRequestObjectResult($"Invalid or unknown uploadId: '{uploadId}'.");
        }

        var (handler, cts, defaultContentType, defaultFileName) = upload;

        if (file.Length == 0)
        {
            Console.WriteLine($"[UploadService] Empty file received for uploadId: {uploadId}");
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

        // Ensure request stream is disposed deterministically to avoid leaking handles
        try
        {
            using var uploadStream = file.OpenReadStream();
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, timeoutCts.Token);
            // Track this upload by fileId so we can cancel on demand
            _inflightUploads[fileUpload.Id] = linkedCts;

            Console.WriteLine($"[UploadService] Begin upload connection={connectionId} uploadId={uploadId} fileId={fileUpload.Id} name='{fileUpload.FileName}' length={fileUpload.Length} contentType='{fileUpload.ContentType}'");
            await handler(fileUpload, uploadStream, linkedCts.Token);
            sw.Stop();
            Console.WriteLine($"[UploadService] Completed upload connection={connectionId} fileId={fileUpload.Id} elapsed={sw.ElapsedMilliseconds}ms");
        }
        catch (Exception)
        {
            sw.Stop();
            Console.WriteLine($"[UploadService] Failed upload connection={connectionId} uploadId={uploadId} elapsed={sw.ElapsedMilliseconds}ms");
            // Bubble up to framework after handler updates status; controller will return 500
            throw;
        }
        finally
        {
            // Remove tracking for this fileId
            _inflightUploads.TryRemove(fileUpload.Id, out _);
        }

        return new OkResult();
    }

    public void CancelUpload(Guid fileId)
    {
        if (_inflightUploads.TryGetValue(fileId, out var cts))
        {
            try
            {
                Console.WriteLine($"[UploadService] Cancellation requested connection={connectionId} fileId={fileId}");
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
    (IDisposable cleanup, string url) AddUpload(UploadDelegate handler, string? defaultContentType = null, string? defaultFileName = null);

    Task<IActionResult> Upload(string uploadId, IFormFile file);

    /// <summary>
    /// Requests cancellation of an in-flight upload by its fileId.
    /// Safe to call if no upload is in-flight for the given id.
    /// </summary>
    void CancelUpload(Guid fileId);
}
