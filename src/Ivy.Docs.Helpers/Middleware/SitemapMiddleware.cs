using System.Text;
using Ivy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Ivy.Docs.Helpers.Middleware;

public static class SitemapMiddlewareExtensions
{
    public static IApplicationBuilder UseSitemap(this IApplicationBuilder app)
    {
        return app.UseMiddleware<SitemapMiddleware>();
    }
}

public class SitemapMiddleware(RequestDelegate next, Server server)
{
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
        var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}";
        var content = $"""
            User-agent: *
            Allow: /

            Sitemap: {baseUrl}/sitemap.xml
            """;

        context.Response.ContentType = "text/plain; charset=utf-8";
        context.Response.StatusCode = 200;
        await context.Response.WriteAsync(content);
    }

    private async Task ServeSitemapXml(HttpContext context)
    {
        var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}";
        var apps = server.AppRepository.All()
            .Where(app => app.IsVisible)
            .ToList();

        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

        foreach (var app in apps)
        {
            var url = $"{baseUrl}/{app.Id}";
            sb.AppendLine($"  <url><loc>{url}</loc></url>");
        }

        sb.AppendLine("</urlset>");

        context.Response.ContentType = "application/xml; charset=utf-8";
        context.Response.StatusCode = 200;
        await context.Response.WriteAsync(sb.ToString());
    }
}
