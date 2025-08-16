using Ivy.Samples.Shared.Apps.Demos;

namespace Ivy.Samples.Shared;

public static class SamplesServer
{
    public static async Task RunAsync(ServerArgs? args = null)
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");
        var server = new Server(args);
        server.UseHotReload();
        server.AddAppsFromAssembly(typeof(SamplesServer).Assembly);

        var version = typeof(Server).Assembly.GetName().Version!.ToString().EatRight(".0");
        server.SetMetaTitle($"Ivy Samples {version}");

        var chromeSettings = new ChromeSettings()
            .Header(
                Layout.Vertical().Padding(2)
                | new IvyLogo()
                | Text.Muted($"Version {version}")
            )
            .DefaultApp<HelloApp>()
            .UseTabs(preventDuplicates: true);

        server.UseChrome(() => new DefaultSidebarChrome(chromeSettings));
        server.Services.AddSingleton<SampleDbContextFactory>();
        await server.RunAsync();
    }
}