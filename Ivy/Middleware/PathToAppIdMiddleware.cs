using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ivy.Middleware;

/// <summary>
/// Middleware that converts path-based URLs to appId query parameters for backward compatibility.
/// For example: /onboarding/getting-started/chat-tutorial-app -> /?appId=onboarding/getting-started/chat-tutorial-app
/// </summary>
public class PathToAppIdMiddleware(RequestDelegate next, ILogger<PathToAppIdMiddleware> logger)
{
    private class RoutingConstantData
    {
        [JsonPropertyName("excludedPaths")]
        public string[] ExcludedPaths { get; set; } = [];

        [JsonPropertyName("staticFileExtensions")]
        public string[] StaticFileExtensions { get; set; } = [];
    }

    private static readonly RoutingConstantData RoutingConstants;

    static PathToAppIdMiddleware()
    {
        using var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("RoutingConstants")!;
        RoutingConstants = JsonSerializer.Deserialize<RoutingConstantData>(stream)!;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower() ?? "";
        var originalPath = context.Request.Path.Value ?? "";

        // Skip if path is empty or just "/"
        if (string.IsNullOrEmpty(path) || path == "/")
        {
            await next(context);
            return;
        }

        // Skip if path starts with any excluded pattern (must be exact segment match)
        if (RoutingConstants.ExcludedPaths.Any(excluded => path == excluded || path.StartsWith(excluded + "/")))
        {
            await next(context);
            return;
        }

        // Skip if path has a static file extension
        if (RoutingConstants.StaticFileExtensions.Any(ext => path.EndsWith(ext)))
        {
            await next(context);
            return;
        }

        // Skip if already has appId query parameter
        if (context.Request.Query.ContainsKey("appId"))
        {
            await next(context);
            return;
        }

        // Convert path to appId
        // Remove leading slash and use the rest as appId
        var appId = originalPath.TrimStart('/');

        // Only convert if the path looks like an app ID (contains at least one segment)
        if (!string.IsNullOrEmpty(appId) && !appId.Contains('.'))
        {
            logger.LogDebug("Converting path '{Path}' to appId '{AppId}'", originalPath, appId);

            // Preserve existing query parameters
            var queryString = context.Request.QueryString.HasValue
                ? context.Request.QueryString.Value + "&"
                : "?";

            // Rewrite the request to root with appId parameter
            context.Request.Path = "/";
            context.Request.QueryString = new QueryString($"{queryString}appId={System.Web.HttpUtility.UrlEncode(appId)}");
        }

        await next(context);
    }
}

public static class PathToAppIdMiddlewareExtensions
{
    public static IApplicationBuilder UsePathToAppId(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<PathToAppIdMiddleware>();
    }
}
