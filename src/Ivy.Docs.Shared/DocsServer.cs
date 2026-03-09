using Ivy.Docs.Shared.Middleware;
using Ivy.Docs.Shared.Services;
using Ivy.Docs.Shared.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace Ivy.Docs.Shared;

public static class DocsServer
{
    public static async Task RunAsync(ServerArgs? args = null)
    {
        var server = new Server(args);
        server.UseCulture("en-US");
        server.AddAppsFromAssembly(typeof(DocsServer).Assembly);
        server.UseHotReload();

        server.UseWebApplication(app =>
        {
            app.UseSitemap();
            app.UseSsrMarkdown();
            app.UseMarkdownFiles();
        });

        server.Services.AddHttpClient<IvyDocsQuestionsClient>();
        server.Services.AddScoped<IIvyDocsQuestionsClient>(sp => sp.GetRequiredService<IvyDocsQuestionsClient>());

        var version = typeof(Server).Assembly.GetName().Version!.ToString().EatRight(".0");
        server.SetMetaTitle($"Ivy Docs {version}");

        var chromeSettings = new ChromeSettings()
            .Header(
                Layout.Vertical().Padding(2)
                | new IvyLogo()
                | Text.Muted($"Version {version}")
            )
            .DefaultApp<Apps.Onboarding.GettingStarted.IntroductionApp>()
            .UsePages();
        server.UseChrome(() => new DefaultSidebarChrome(chromeSettings));

        await server.RunAsync();
    }
}
