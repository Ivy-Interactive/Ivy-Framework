using System.Reflection;
using Ivy.Docs.Helpers.Middleware;
using Microsoft.AspNetCore.Builder;

namespace Ivy.Docs.Shared.Middleware;

public static class SsrMarkdownMiddlewareExtensions
{
    private static readonly Assembly Assembly = typeof(SsrMarkdownMiddlewareExtensions).Assembly;
    private const string ResourcePrefix = "Ivy.Docs.Shared.Generated.";

    public static IApplicationBuilder UseSsrMarkdown(this IApplicationBuilder app)
    {
        return app.UseSsrMarkdown(Assembly, ResourcePrefix);
    }
}
