using System.Collections.Concurrent;
using System.Reactive.Disposables;
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
/// Represents a file uploaded through a file input control.
/// </summary>
public record FileUpload
{
    /// <summary>Gets the identifier for this file upload, set by the client.</summary>
    public object? Id { get; set; }

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

public static class FileUploadExtensions
{
    public static void SetProgress(this IState<FileUpload?> fileInputState, float progress)
    {
        var file = fileInputState.Value;
        if (file != null)
        {
            fileInputState.Set(file with { Progress = progress });
        }
    }

    public static void SetStatus(this IState<FileUpload?> fileInputState, FileUploadStatus status)
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
        if (file == null)
        {
            return BadRequest("file is required.");
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
        if (!Guid.TryParse(uploadId, out var guid) || !_uploads.TryGetValue(guid, out var upload))
        {
            return new BadRequestObjectResult($"Invalid or unknown uploadId: '{uploadId}'.");
        }

        var (handler, cts, defaultContentType, defaultFileName) = upload;

        if (file.Length == 0)
        {
            return new BadRequestObjectResult("Empty file.");
        }

        var actualMimeType = file.ContentType.NullIfEmpty() ?? defaultContentType ?? "application/octet-stream";
        var actualFileName = file.FileName.NullIfEmpty() ?? defaultFileName ?? "upload";

        var fileUpload = new FileUpload
        {
            FileName = actualFileName,
            ContentType = actualMimeType,
            Length = file.Length
        };

        await handler(fileUpload, file.OpenReadStream(), cts.Token);

        return new OkResult();
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
}