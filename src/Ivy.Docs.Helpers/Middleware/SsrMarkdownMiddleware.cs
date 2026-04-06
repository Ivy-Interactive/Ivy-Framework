using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Ivy.Docs.Helpers.Middleware;

public static class SsrMarkdownMiddlewareExtensions
{
    public static IApplicationBuilder UseSsrMarkdown(this IApplicationBuilder app, Assembly assembly, string resourcePrefix)
    {
        return app.UseMiddleware<SsrMarkdownMiddleware>(assembly, resourcePrefix);
    }
}

public class SsrMarkdownMiddleware
{
    private readonly RequestDelegate _next;
    private readonly Assembly _assembly;
    private readonly string _resourcePrefix;
    private readonly string[] _manifestResourceNames;
    private readonly ConcurrentDictionary<string, string> _contentCache = new();

    public SsrMarkdownMiddleware(RequestDelegate next, Assembly assembly, string resourcePrefix)
    {
        _next = next;
        _assembly = assembly;
        _resourcePrefix = resourcePrefix;
        _manifestResourceNames = assembly.GetManifestResourceNames();
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

        if (IsBot(context))
        {
            var markdownContent = GetMarkdownContent(appId);
            if (!string.IsNullOrEmpty(markdownContent))
            {
                await ServeBotResponse(context, markdownContent);
                return;
            }
        }

        var markdown = GetMarkdownContent(appId);
        if (!string.IsNullOrEmpty(markdown))
        {
            var originalBodyStream = context.Response.Body;
            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            await _next(context);

            memoryStream.Seek(0, SeekOrigin.Begin);
            var html = await new StreamReader(memoryStream).ReadToEndAsync();

            var encodedMarkdown = System.Web.HttpUtility.HtmlEncode(markdown);
            var noscriptFallback =
                "<noscript>" +
                "<style>#root { display: none; }</style>" +
                "<pre style=\"white-space: pre-wrap; font-family: system-ui, sans-serif; padding: 20px; line-height: 1.6;\">" + encodedMarkdown + "</pre>" +
                "</noscript>";

            html = html.Replace("</body>", noscriptFallback + "</body>");

            context.Response.Body = originalBodyStream;
            var bytes = Encoding.UTF8.GetBytes(html);
            context.Response.ContentLength = bytes.Length;
            await context.Response.Body.WriteAsync(bytes);
            return;
        }

        await _next(context);
    }

    private static readonly string[] KnownBots =
    [
        "GPTBot", "ChatGPT-User", "OAI-SearchBot",
        "ClaudeBot", "Claude-User", "Claude-SearchBot", "anthropic-ai",
        "PerplexityBot", "Perplexity-User",
        "Googlebot", "Google-Extended", "Bingbot", "Slurp", "DuckDuckBot", "DuckAssistBot",
        "Baiduspider", "YandexBot",
        "facebookexternalhit", "Meta-ExternalAgent", "Twitterbot", "LinkedInBot",
        "Discordbot", "TelegramBot", "Slackbot", "WhatsApp",
        "Applebot", "Applebot-Extended", "Amazonbot",
        "github-actions", "GitHub-Hookshot", "Copilot", "GitHubCopilot",
        "cohere-ai", "AI2Bot", "Bytespider", "CCBot", "DataForSeoBot",
        "Diffbot", "ImagesiftBot", "Omgilibot", "Timpibot", "VelenPublicWebCrawler",
        "Webzio-Extended", "YouBot"
    ];

    private static bool IsBot(HttpContext context)
    {
        var userAgent = context.Request.Headers.UserAgent.ToString();
        if (string.IsNullOrEmpty(userAgent))
            return false;

        return KnownBots.Any(bot => userAgent.Contains(bot, StringComparison.OrdinalIgnoreCase));
    }

    private static async Task ServeBotResponse(HttpContext context, string markdownContent)
    {
        var html = $"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
                <meta charset="UTF-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
            </head>
            <body>
            <pre style="white-space: pre-wrap; font-family: system-ui, sans-serif; padding: 20px; line-height: 1.6;">{System.Web.HttpUtility.HtmlEncode(markdownContent)}</pre>
            </body>
            </html>
            """;

        context.Response.ContentType = "text/html; charset=utf-8";
        context.Response.StatusCode = 200;
        var bytes = Encoding.UTF8.GetBytes(html);
        context.Response.ContentLength = bytes.Length;
        await context.Response.Body.WriteAsync(bytes);
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

    private string? GetMarkdownContent(string appId)
    {
        return _contentCache.GetOrAdd(appId, id =>
        {
            var resourceName = ConvertAppIdToResourceName(id);
            var resolvedName =
                _manifestResourceNames.FirstOrDefault(r => r.Equals(resourceName, StringComparison.OrdinalIgnoreCase)) ??
                resourceName;
            using var stream = _assembly.GetManifestResourceStream(resolvedName);
            if (stream == null)
                return null!;

            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        });
    }

    private string ConvertAppIdToResourceName(string appId)
    {
        var segments = appId.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var pascalSegments = segments.Select(ToPascalCase);
        return _resourcePrefix + string.Join(".", pascalSegments) + ".md";
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
