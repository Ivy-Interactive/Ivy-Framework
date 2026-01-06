using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Ivy.Docs.Shared.Middleware;

public static class SsrMarkdownMiddlewareExtensions
{
    public static IApplicationBuilder UseSsrMarkdown(this IApplicationBuilder app)
    {
        return app.UseMiddleware<SsrMarkdownMiddleware>();
    }
}

public class SsrMarkdownMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly Assembly Assembly = typeof(SsrMarkdownMiddleware).Assembly;
    private static readonly string ResourcePrefix = "Ivy.Docs.Shared.Generated.";
    private static readonly Dictionary<string, string> ContentCache = new();
    private static readonly object CacheLock = new();

    public SsrMarkdownMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value;

        if (ShouldSkip(context, path))
        {
            await _next(context);
            return;
        }

        var appId = context.Request.Query["appId"].FirstOrDefault();
        if (string.IsNullOrEmpty(appId))
        {
            await _next(context);
            return;
        }

        var originalBodyStream = context.Response.Body;
        using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;

        await _next(context);

        memoryStream.Seek(0, SeekOrigin.Begin);
        var html = await new StreamReader(memoryStream).ReadToEndAsync();

        if (context.Response.ContentType?.Contains("text/html") == true)
        {
            var markdownContent = GetMarkdownContent(appId);
            if (!string.IsNullOrEmpty(markdownContent))
            {
                var plainTextContent = $"<pre style=\"white-space: pre-wrap; font-family: system-ui, sans-serif; padding: 20px; line-height: 1.6;\">{System.Web.HttpUtility.HtmlEncode(markdownContent)}</pre>";
                html = html.Replace("<div id=\"root\"></div>", $"<div id=\"root\">{plainTextContent}</div>");
            }
        }

        context.Response.Body = originalBodyStream;

        if (!context.Response.HasStarted)
        {
            var bytes = Encoding.UTF8.GetBytes(html);
            context.Response.ContentLength = bytes.Length;
            await context.Response.Body.WriteAsync(bytes);
        }
        else
        {
            await context.Response.WriteAsync(html);
        }
    }

    private static bool ShouldSkip(HttpContext context, string? path)
    {
        if (string.IsNullOrEmpty(path))
            return true;

        if (context.WebSockets.IsWebSocketRequest)
            return true;

        if (path.Contains('.') && !path.EndsWith(".html"))
            return true;

        if (context.Request.Headers.Accept.Any(h => h?.Contains("application/json") == true))
            return true;

        if (path.StartsWith("/ivy/", StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }

    private static string? GetMarkdownContent(string appId)
    {
        lock (CacheLock)
        {
            if (ContentCache.TryGetValue(appId, out var cached))
                return cached;
        }

        var resourceName = ConvertAppIdToResourceName(appId);
        using var stream = Assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
            return null;

        using var reader = new StreamReader(stream);
        var content = reader.ReadToEnd();

        lock (CacheLock)
        {
            ContentCache.TryAdd(appId, content);
        }

        return content;
    }

    private static string ConvertAppIdToResourceName(string appId)
    {
        var segments = appId.Split('/', StringSplitOptions.RemoveEmptyEntries);
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
