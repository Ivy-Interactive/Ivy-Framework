using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

// ReSharper disable once CheckNamespace
namespace Ivy;

[Route("ivy/local-file")]
public class LocalFileController(Server server) : Controller
{
    private static readonly FileExtensionContentTypeProvider ContentTypeProvider = CreateContentTypeProvider();

    private static FileExtensionContentTypeProvider CreateContentTypeProvider()
    {
        var provider = new FileExtensionContentTypeProvider();

        // Add mappings not included by default
        provider.Mappings[".webp"] = "image/webp";
        provider.Mappings[".avif"] = "image/avif";
        provider.Mappings[".webm"] = "video/webm";
        provider.Mappings[".md"] = "text/markdown";
        provider.Mappings[".jsonl"] = "application/jsonl";

        return provider;
    }

    [HttpGet]
    public IActionResult GetFile([FromQuery] string? path)
    {
        if (!server.Args.DangerouslyAllowLocalFiles)
            return NotFound();

        if (string.IsNullOrWhiteSpace(path))
            return BadRequest("Path is required");

        var fullPath = Path.GetFullPath(path);
        if (!System.IO.File.Exists(fullPath))
            return NotFound();

        if (!ContentTypeProvider.TryGetContentType(fullPath, out var contentType))
            contentType = "application/octet-stream";

        var fileName = Path.GetFileName(fullPath);
        Response.Headers.ContentDisposition = $"inline; filename=\"{fileName}\"";

        return PhysicalFile(fullPath, contentType);
    }
}
