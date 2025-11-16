using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
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

        // First check if routing already found an endpoint (this works if UseRouting already executed)
        var endpoint = context.GetEndpoint();
        if (endpoint != null)
        {
            logger.LogDebug("Path '{Path}' already has matched endpoint, skipping conversion", originalPath);
            await next(context);
            return;
        }

        // If endpoint is not found yet, check EndpointDataSource to see if this path matches any registered endpoint
        // This is needed because in minimal API, MapControllers() registers endpoints but routing happens later
        var endpointDataSource = context.RequestServices.GetService<EndpointDataSource>();
        if (endpointDataSource != null)
        {
            // Check if any endpoint matches this path
            foreach (var ep in endpointDataSource.Endpoints)
            {
                if (ep is RouteEndpoint routeEp)
                {
                    var routePath = GetRoutePath(routeEp.RoutePattern);

                    // Check if current path matches this endpoint (exact or starts with route + "/")
                    if (!string.IsNullOrEmpty(routePath) &&
                        (originalPath.Equals(routePath, StringComparison.OrdinalIgnoreCase) ||
                         originalPath.StartsWith(routePath + "/", StringComparison.OrdinalIgnoreCase)))
                    {
                        logger.LogDebug("Path '{Path}' matches endpoint route '{Route}', skipping conversion",
                            originalPath, routePath);
                        await next(context);
                        return;
                    }
                }
            }
        }

        // Get all excluded paths from routing-constants.json
        var excludedPaths = RoutingConstants.ExcludedPaths;

        // Skip if path starts with any excluded pattern (must be exact segment match)
        foreach (var excluded in excludedPaths)
        {
            if (path == excluded || path.StartsWith(excluded + "/"))
            {
                logger.LogDebug("Path '{Path}' matches excluded pattern, skipping conversion", originalPath);
                await next(context);
                return;
            }
        }

        // Skip if path has a static file extension
        var staticExtensions = RoutingConstants.StaticFileExtensions;
        foreach (var ext in staticExtensions)
        {
            if (path.EndsWith(ext))
            {
                await next(context);
                return;
            }
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

    /// <summary>
    /// Extracts the route path from a RoutePattern, building from segments if RawText is empty.
    /// </summary>
    private static string? GetRoutePath(RoutePattern routePattern)
    {
        var routePath = routePattern.RawText;

        // Build path from segments if RawText is empty
        if (string.IsNullOrEmpty(routePath))
        {
            var segments = new List<string>();
            foreach (var segment in routePattern.PathSegments)
            {
                var segmentType = segment.GetType().Name;
                if (segmentType == "RoutePatternLiteralSegment")
                {
                    var contentProp = segment.GetType().GetProperty("Content");
                    if (contentProp?.GetValue(segment) is string content)
                    {
                        segments.Add(content);
                    }
                }
                else if (segmentType.Contains("Parameter"))
                {
                    // For routes with parameters like /test-route/{id}, check if path starts with /test-route
                    break;
                }
            }
            routePath = segments.Count > 0 ? "/" + string.Join("/", segments) : null;
        }

        return routePath;
    }
}

public static class PathToAppIdMiddlewareExtensions
{
    public static IApplicationBuilder UsePathToAppId(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<PathToAppIdMiddleware>();
    }
}
