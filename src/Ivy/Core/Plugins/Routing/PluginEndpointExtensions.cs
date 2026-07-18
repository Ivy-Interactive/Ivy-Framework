using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;

namespace Ivy.Core.Plugins.Routing;

public static class PluginEndpointExtensions
{
    private static readonly FileExtensionContentTypeProvider ContentTypeProvider = new();

    /// <summary>
    /// Serves static files from the plugin directory at the given sub-path.
    /// Must be called on an <see cref="IEndpointRouteBuilder"/> inside a <c>UseEndpoints</c> callback.
    /// </summary>
    public static IEndpointRouteBuilder MapStaticAssets(this IEndpointRouteBuilder endpoints, string subPath)
    {
        var directory = endpoints is PluginEndpointRouteBuilderWithDirectory withDir
            ? withDir.PluginDirectory
            : throw new InvalidOperationException(
                "MapStaticAssets can only be called on an endpoint builder provided by UseEndpoints.");

        return MapStaticAssets(endpoints, subPath, directory);
    }

    public static IEndpointRouteBuilder MapStaticAssets(this IEndpointRouteBuilder endpoints, string subPath, string directory)
    {
        var normalizedSubPath = subPath.TrimStart('/').TrimEnd('/');
        var baseDir = Path.GetFullPath(directory);

        endpoints.MapGet($"{normalizedSubPath}/{{**filePath}}", (string filePath) =>
        {
            if (string.IsNullOrEmpty(filePath) || Path.IsPathRooted(filePath))
                return Results.NotFound();

            var fullPath = Path.GetFullPath(Path.Join(baseDir, filePath));
            if (!fullPath.StartsWith(baseDir + Path.DirectorySeparatorChar))
                return Results.NotFound();

            if (!File.Exists(fullPath))
                return Results.NotFound();

            if (!ContentTypeProvider.TryGetContentType(fullPath, out var contentType))
                contentType = "application/octet-stream";

            return Results.File(fullPath, contentType);
        });

        return endpoints;
    }
}
