using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace Ivy.Middleware;

/// <summary>
/// Middleware that converts path-based URLs to appId query parameters for backward compatibility.
/// For example: /onboarding/getting-started/chat-tutorial-app -> /?appId=onboarding/getting-started/chat-tutorial-app
/// </summary>
public class PathToAppIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PathToAppIdMiddleware> _logger;

    // Patterns that should NOT be converted (existing endpoints)
    private static readonly string[] ExcludedPaths =
    {
        "/messages",      // SignalR hub
        "/webhook",       // Webhook endpoints
        "/auth",          // Auth endpoints
        "/assets",        // Static assets
        "/fonts",         // Font files
        "/_framework",    // Blazor framework files
        "/favicon.ico",   // Favicon
        "/manifest.json", // PWA manifest
        "/service-worker.js", // Service worker
        "/index.html"     // Direct index.html access
    };

    // File extensions that should be served as static files
    private static readonly string[] StaticFileExtensions =
    {
        ".js", ".css", ".html", ".json", ".ico", ".png", ".jpg", ".jpeg",
        ".gif", ".svg", ".woff", ".woff2", ".ttf", ".eot", ".map"
    };

    public PathToAppIdMiddleware(RequestDelegate next, ILogger<PathToAppIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower() ?? "";
        var originalPath = context.Request.Path.Value ?? "";

        // Skip if path is empty or just "/"
        if (string.IsNullOrEmpty(path) || path == "/")
        {
            await _next(context);
            return;
        }

        // Skip if path starts with any excluded pattern
        if (ExcludedPaths.Any(excluded => path.StartsWith(excluded)))
        {
            await _next(context);
            return;
        }

        // Skip if path has a static file extension
        if (StaticFileExtensions.Any(ext => path.EndsWith(ext)))
        {
            await _next(context);
            return;
        }

        // Skip if already has appId query parameter
        if (context.Request.Query.ContainsKey("appId"))
        {
            await _next(context);
            return;
        }

        // Convert path to appId
        // Remove leading slash and use the rest as appId
        var appId = originalPath.TrimStart('/');

        // Only convert if the path looks like an app ID (contains at least one segment)
        if (!string.IsNullOrEmpty(appId) && !appId.Contains('.'))
        {
            _logger.LogDebug("Converting path '{Path}' to appId '{AppId}'", originalPath, appId);

            // Preserve existing query parameters
            var queryString = context.Request.QueryString.HasValue
                ? context.Request.QueryString.Value + "&"
                : "?";

            // Rewrite the request to root with appId parameter
            context.Request.Path = "/";
            context.Request.QueryString = new QueryString($"{queryString}appId={Uri.EscapeDataString(appId)}");
        }

        await _next(context);
    }
}

public static class PathToAppIdMiddlewareExtensions
{
    public static IApplicationBuilder UsePathToAppId(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<PathToAppIdMiddleware>();
    }
}
