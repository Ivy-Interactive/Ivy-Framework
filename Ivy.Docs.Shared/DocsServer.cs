using Ivy.Chrome;

namespace Ivy.Docs.Shared;

public static class DocsServer
{
    public static async Task RunAsync(ServerArgs? args = null)
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");
        var server = new Server(args);
        server.AddAppsFromAssembly(typeof(DocsServer).Assembly);
        server.UseHotReload();

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