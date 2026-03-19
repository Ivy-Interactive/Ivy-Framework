using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ivy.Core.Apps;

namespace Ivy.Core.Server.Middleware;

public class PathToAppIdMiddleware(RequestDelegate next, ILogger<PathToAppIdMiddleware> logger, global::Ivy.Server server)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";

        // Skip if an endpoint has already been matched (e.g., MVC controller, gRPC service)
        if (context.GetEndpoint() != null)
        {
            await next(context);
            return;
        }

        // Convert path to appId: remove leading slash and trim trailing slash so /hooks/core/ === /hooks/core
        var appId = path.TrimStart('/').TrimEnd('/');

        if (AppRoutingHelpers.ValidateAppId(appId, server.ReservedPaths) != AppIdValidationResult.Valid)
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


        // Only convert if the path looks like an app ID (contains at least one segment)
        if (!string.IsNullOrEmpty(appId))
        {
            logger.LogDebug("Converting path '{Path}' to appId '{AppId}'", path, appId);

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
