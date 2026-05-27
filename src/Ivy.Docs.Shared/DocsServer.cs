using Ivy.Helpers;
using Ivy.Docs.Shared.Middleware;
using Ivy.Docs.Shared.Services;

namespace Ivy.Docs.Shared;

public static class DocsServer
{
    public static async Task RunAsync(ServerArgs? args = null)
    {
        var server = new Server(args);
        server.UseCulture("en-US");
        server.AddAppsFromAssembly(typeof(DocsServer).Assembly);
        server.ReservePaths("/sitemap.xml", "/robots.txt", "/agents.md", "/llms.txt");
        server.UseHotReload();

        server.UseWebApplication(app =>
        {
            app.UseSitemap();
            app.UseSsrMarkdown();
            app.UseMarkdownFiles();
        });

        server.Services.AddHttpClient<IvyDocsQuestionsClient>();
        server.Services.AddTransient<IIvyDocsQuestionsClient>(sp => sp.GetRequiredService<IvyDocsQuestionsClient>());

        var versionLabel = ServerVersionHelper.GetVersionLabel();

        server.SetMetaTitle($"Ivy Docs {versionLabel}");

        var appShellSettings = new AppShellSettings()
            .Header(
                Layout.Vertical().Padding(2)
                | new IvyLogo()
                | Text.Muted(versionLabel)
            )
            .DefaultApp<Apps.Onboarding.GettingStarted.IntroductionApp>()
            .UsePages();
        server.UseAppShell(() => new DefaultSidebarAppShell(appShellSettings));

        await server.RunAsync();
    }
}
