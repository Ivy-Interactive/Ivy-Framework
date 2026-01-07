using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Ivy.Docs.Shared.Middleware;

public static class SitemapMiddlewareExtensions
{
    public static IApplicationBuilder UseSitemap(this IApplicationBuilder app)
    {
        return app.UseMiddleware<SitemapMiddleware>();
    }
}

public class SitemapMiddleware(RequestDelegate next, Server server)
{
    private const string BaseUrl = "https://docs.ivy.app";

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant();

        if (path == "/robots.txt")
        {
            await ServeRobotsTxt(context);
            return;
        }

        if (path == "/sitemap.xml")
        {
            await ServeSitemapXml(context);
            return;
        }

        await next(context);
    }

    private static async Task ServeRobotsTxt(HttpContext context)
    {
        var content = $"""
            User-agent: *
            Allow: /

            Sitemap: {BaseUrl}/sitemap.xml
            """;

        context.Response.ContentType = "text/plain; charset=utf-8";
        context.Response.StatusCode = 200;
        await context.Response.WriteAsync(content);
    }

    private async Task ServeSitemapXml(HttpContext context)
    {
        server.AppRepository.Reload();
        var apps = server.AppRepository.All()
            .Where(app => app.IsVisible)
            .ToList();

        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

        foreach (var app in apps)
        {
            var url = $"{BaseUrl}/{app.Id}";
            sb.AppendLine($"  <url><loc>{url}</loc></url>");
        }

        sb.AppendLine("</urlset>");

        context.Response.ContentType = "application/xml; charset=utf-8";
        context.Response.StatusCode = 200;
        await context.Response.WriteAsync(sb.ToString());
    }
}
