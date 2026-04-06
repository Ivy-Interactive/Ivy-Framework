using System.Reflection;
using Ivy.Docs.Helpers.Middleware;

namespace Ivy.Tendril.Docs;

public static class TendrilDocsServer
{
    private static readonly Assembly DocsAssembly = typeof(TendrilDocsServer).Assembly;
    private const string ResourcePrefix = "Ivy.Tendril.Docs.Generated.";

    public static async Task RunAsync(ServerArgs? args = null)
    {
        var server = new Server(args);
        server.UseCulture("en-US");
        server.AddAppsFromAssembly(DocsAssembly);
        server.ReservePaths("/sitemap.xml", "/robots.txt");
        server.UseHotReload();

        server.UseWebApplication(app =>
        {
            app.UseSitemap();
            app.UseSsrMarkdown(DocsAssembly, ResourcePrefix);
            app.UseMarkdownFiles(DocsAssembly, ResourcePrefix);
        });

        var version = DocsAssembly.GetName().Version?.ToString()?.EatRight(".0") ?? "0.0.1";
        server.SetMetaTitle($"Tendril Docs {version}");

        var appShellSettings = new AppShellSettings()
            .Header(
                Layout.Vertical().Padding(2)
                | Text.H1("Tendril")
                | Text.Muted($"Version {version}")
            )
            .DefaultApp<Apps.GettingStarted.Overview.IntroductionApp>()
            .UsePages();
        server.UseAppShell(() => new DefaultSidebarAppShell(appShellSettings));

        await server.RunAsync();
    }
}
