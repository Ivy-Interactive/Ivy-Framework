using System.Reflection;
using Ivy.Docs.Helpers.Middleware;
using Microsoft.AspNetCore.Builder;

namespace Ivy.Docs.Shared.Middleware;

public static class MarkdownMiddlewareExtensions
{
    private static readonly Assembly Assembly = typeof(MarkdownMiddlewareExtensions).Assembly;
    private const string ResourcePrefix = "Ivy.Docs.Shared.Generated.";

    public static IApplicationBuilder UseMarkdownFiles(this IApplicationBuilder app)
    {
        return app.UseMarkdownFiles(Assembly, ResourcePrefix);
    }
}
