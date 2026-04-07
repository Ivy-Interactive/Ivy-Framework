using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
            app.UseAssets(server.Args, app.Services.GetRequiredService<ILogger<Server>>(), "Assets", "tendril-docs/assets");
        });

        var version = DocsAssembly.GetName().Version?.ToString()?.EatRight(".0") ?? "0.0.1";
        server.SetMetaTitle($"Tendril Docs {version}");

        var appShellSettings = new AppShellSettings()
            .Header(
                Layout.Horizontal(
                    new Image("/tendril-docs/assets/Tendril.svg").Width(Size.Units(15)).Height(Size.Auto()),
                    Layout.Vertical(
                        Text.Block("Tendril"),
                        Text.Muted($"v{version}")
                    ).Gap(0)
                ).Gap(2).Padding(2).AlignContent(Align.BottomLeft)
            )
            .DefaultApp<Apps.GettingStarted.IntroductionApp>()
            .UsePages();
        server.UseAppShell(() => new DefaultSidebarAppShell(appShellSettings));

        await server.RunAsync();
    }
}
