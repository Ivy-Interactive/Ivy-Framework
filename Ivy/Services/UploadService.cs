using System.Collections.Concurrent;
using System.Reactive.Disposables;
using Ivy.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Ivy.Services;

public record FileUpload(string FileName, string ContentType, long Length, DateTime? LastModified, Stream Stream)
    : FileBase(FileName, ContentType, Length, LastModified);

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
    private readonly ConcurrentDictionary<Guid, (Func<FileUpload, Task> handler, string? mimeType, string? fileName)> _uploads = new();

    public (IDisposable cleanup, string url) AddUpload(Func<FileUpload, Task> handler, string? defaultContentType = null, string? defaultFileName = null)
    {
        var uploadId = Guid.NewGuid();
        _uploads[uploadId] = (handler, defaultContentType, defaultFileName);

        var cleanup = Disposable.Create(() =>
        {
            _uploads.TryRemove(uploadId, out _);
        });

        return (cleanup, $"/upload/{connectionId}/{uploadId}");
    }

    public async Task<IActionResult> Upload(string uploadId, IFormFile file)
    {
        if (!Guid.TryParse(uploadId, out var guid) || !_uploads.TryGetValue(guid, out var upload))
        {
            return new BadRequestObjectResult($"Invalid or unknown uploadId: '{uploadId}'.");
        }

        var (handler, defaultContentType, defaultFileName) = upload;

        if (file.Length == 0)
        {
            return new BadRequestObjectResult("Empty file.");
        }

        var actualMimeType = file.ContentType.NullIfEmpty() ?? defaultContentType ?? "application/octet-stream";
        var actualFileName = file.FileName.NullIfEmpty() ?? defaultFileName ?? "upload";

        // Note: IFormFile.OpenReadStream() returns a Stream that's valid during reqnuest
        var fileUpload = new FileUpload(
            FileName: actualFileName,
            ContentType: actualMimeType,
            Length: file.Length,
            Stream: file.OpenReadStream(),
            LastModified: DateTime.UtcNow
        );

        await handler(fileUpload);

        return new OkResult();
    }

    public void Dispose()
    {
        _uploads.Clear();
    }
}

public interface IUploadService
{
    (IDisposable cleanup, string url) AddUpload(Func<FileUpload, Task> handler, string? defaultContentType = null, string? defaultFileName = null);

    Task<IActionResult> Upload(string uploadId, IFormFile file);
}