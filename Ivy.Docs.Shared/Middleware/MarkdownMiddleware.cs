using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Ivy.Docs.Shared.Middleware;

public static class MarkdownMiddlewareExtensions
{
    public static IApplicationBuilder UseMarkdownFiles(this IApplicationBuilder app)
    {
        return app.UseMiddleware<MarkdownMiddleware>();
    }
}

public class MarkdownMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly Assembly Assembly = typeof(MarkdownMiddleware).Assembly;
    private static readonly string ResourcePrefix = "Ivy.Docs.Shared.Generated.";
    private static readonly ConcurrentDictionary<string, byte[]?> ContentCache = new();

    public MarkdownMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value;

        if (string.IsNullOrEmpty(path) || !path.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var basePath = path.TrimStart('/');
        if (basePath.Length <= ".md".Length)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Invalid path");
            return;
        }
        basePath = basePath[..^".md".Length];

        var resourceName = ConvertPathToResourceName(basePath);

        var content = GetOrLoadContent(resourceName);
        if (content == null)
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsync($"Markdown documentation not found for: {basePath}");
            return;
        }

        context.Response.ContentType = "text/markdown; charset=utf-8";
        context.Response.ContentLength = content.Length;
        context.Response.Headers.CacheControl = "public, max-age=3600";

        await context.Response.Body.WriteAsync(content);
    }

    private static byte[]? GetOrLoadContent(string resourceName)
    {
        return ContentCache.GetOrAdd(resourceName, name =>
        {
            using var stream = Assembly.GetManifestResourceStream(name);
            if (stream == null)
                return null;

            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            return ms.ToArray();
        });
    }

    private static string ConvertPathToResourceName(string urlPath)
    {
        var segments = urlPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var pascalSegments = segments.Select(ToPascalCase);

        return ResourcePrefix + string.Join(".", pascalSegments) + ".md";
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


