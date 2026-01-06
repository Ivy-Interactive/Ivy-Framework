using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Ivy.Docs.Shared.Middleware;

public static class LlmsTxtMiddlewareExtensions
{
    public static IApplicationBuilder UseLlmsTxt(this IApplicationBuilder app)
    {
        return app.UseMiddleware<LlmsTxtMiddleware>();
    }
}

public class LlmsTxtMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly Assembly Assembly = typeof(LlmsTxtMiddleware).Assembly;
    private static readonly string ResourcePrefix = "Ivy.Docs.Shared.Generated.";
    private static readonly Dictionary<string, byte[]> ContentCache = new();
    private static readonly object CacheLock = new();

    public LlmsTxtMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value;

        if (string.IsNullOrEmpty(path) || !path.EndsWith(".llms.txt", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var basePath = path.TrimStart('/');
        if (basePath.Length <= ".llms.txt".Length)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Invalid path");
            return;
        }
        basePath = basePath[..^".llms.txt".Length];

        var resourceName = ConvertPathToResourceName(basePath);

        var content = GetOrLoadContent(resourceName);
        if (content == null)
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsync($"LLM documentation not found for: {basePath}");
            return;
        }

        context.Response.ContentType = "text/plain; charset=utf-8";
        context.Response.ContentLength = content.Length;
        context.Response.Headers.CacheControl = "public, max-age=3600";

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

    private static string ConvertPathToResourceName(string urlPath)
    {
        var segments = urlPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var pascalSegments = segments.Select(ToPascalCase);

        return ResourcePrefix + string.Join(".", pascalSegments) + ".llms.txt";
    }

    private static string ToPascalCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

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

