using System.Collections.Concurrent;
using System.Reactive.Disposables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Ivy.Services;

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
    private readonly ConcurrentDictionary<Guid, (Func<byte[], Task> handler, string mimeType, string fileName)> _uploads = new();

    public (IDisposable cleanup, string url) AddUpload(Func<byte[], Task> handler, string mimeType, string fileName)
    {
        var uploadId = Guid.NewGuid();
        _uploads[uploadId] = (handler, mimeType, fileName);

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

        var (handler, expectedContentType, expectedFileName) = upload;

        if (file == null || file.Length == 0)
        {
            return new BadRequestObjectResult("Empty file.");
        }

        // Optional sanity checks; do not block upload if mismatched, just basic validation could be enforced here
        // If strict validation is desired, uncomment the checks below
        // if (!string.IsNullOrWhiteSpace(expectedContentType) && !string.Equals(file.ContentType, expectedContentType, StringComparison.OrdinalIgnoreCase))
        // {
        //     return new BadRequestObjectResult($"Unexpected content type. Expected '{expectedContentType}', got '{file.ContentType}'.");
        // }

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var fileBytes = memoryStream.ToArray();

        await handler(fileBytes);

        return new OkResult();
    }

    public void Dispose()
    {
        _uploads.Clear();
    }
}

public interface IUploadService
{
    (IDisposable cleanup, string url) AddUpload(Func<byte[], Task> handler, string mimeType, string fileName);

    Task<IActionResult> Upload(string uploadId, IFormFile file);
}