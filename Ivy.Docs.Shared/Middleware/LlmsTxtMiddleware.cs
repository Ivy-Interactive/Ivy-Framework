using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Ivy.Docs.Shared.Middleware;

/// <summary>
/// Extension methods for registering LlmsTxt middleware.
/// </summary>
public static class LlmsTxtMiddlewareExtensions
{
    /// <summary>
    /// Adds middleware for serving .llms.txt files from embedded resources.
    /// </summary>
    public static IApplicationBuilder UseLlmsTxt(this IApplicationBuilder app)
    {
        return app.UseMiddleware<LlmsTxtMiddleware>();
    }
}

/// <summary>
/// Middleware for serving LLM-friendly documentation files.
/// Intercepts requests ending with .llms.txt and returns embedded resources.
/// </summary>
public class LlmsTxtMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly Assembly Assembly = typeof(LlmsTxtMiddleware).Assembly;
    private static readonly string ResourcePrefix = "Ivy.Docs.Shared.Generated.";

    // Cache for embedded resources (they don't change at runtime)
    private static readonly Dictionary<string, byte[]> ContentCache = new();
    private static readonly object CacheLock = new();

    public LlmsTxtMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value;

        // Only handle .llms.txt requests
        if (string.IsNullOrEmpty(path) || !path.EndsWith(".llms.txt", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        // Remove leading slash and .llms.txt extension
        var basePath = path.TrimStart('/');
        if (basePath.Length <= ".llms.txt".Length)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Invalid path");
            return;
        }
        basePath = basePath[..^".llms.txt".Length];

        // Convert URL path to resource name
        var resourceName = ConvertPathToResourceName(basePath);

        // Try to get from cache first
        var content = GetOrLoadContent(resourceName);
        if (content == null)
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsync($"LLM documentation not found for: {basePath}");
            return;
        }

        // Set response headers
        context.Response.ContentType = "text/plain; charset=utf-8";
        context.Response.ContentLength = content.Length;
        context.Response.Headers.CacheControl = "public, max-age=3600"; // Cache for 1 hour

        await context.Response.Body.WriteAsync(content);
    }

    private static byte[]? GetOrLoadContent(string resourceName)
    {
        lock (CacheLock)
        {
            if (ContentCache.TryGetValue(resourceName, out var cached))
                return cached;
        }

        using var stream = Assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
            return null;

        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        var content = ms.ToArray();

        lock (CacheLock)
        {
            ContentCache.TryAdd(resourceName, content);
        }

        return content;
    }

    /// <summary>
    /// Converts a URL path to an embedded resource name.
    /// </summary>
    private static string ConvertPathToResourceName(string urlPath)
    {
        // Split path and capitalize each segment (PascalCase)
        var segments = urlPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var pascalSegments = segments.Select(ToPascalCase);

        return ResourcePrefix + string.Join(".", pascalSegments) + ".llms.txt";
    }

    /// <summary>
    /// Converts a string to PascalCase.
    /// </summary>
    private static string ToPascalCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        // Handle kebab-case and snake_case
        var parts = input.Split(['-', '_'], StringSplitOptions.RemoveEmptyEntries);
        var result = new StringBuilder();

        foreach (var part in parts)
        {
            if (part.Length > 0)
            {
                result.Append(char.ToUpperInvariant(part[0]));
                if (part.Length > 1)
                    result.Append(part[1..]);
            }
        }

        return result.ToString();
    }
}

