using Ivy.Chrome;
using Ivy.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ivy.Docs.Shared;

public static class DocsServer
{
    public static async Task RunAsync(ServerArgs? args = null)
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");
        var server = new Server(args);
        server.AddAppsFromAssembly(typeof(DocsServer).Assembly);
        server.UseHotReload();

        // Register GitHub services for documentation contributors functionality
        server.Services.AddDistributedMemoryCache();
        server.Services.AddHttpClient();
        server.Services.AddSingleton<ICacheService, CacheService>();

        // Register GitHubService with a factory to handle the dependencies manually
        server.Services.AddSingleton<IGitHubService>(serviceProvider =>
        {
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient();
            var cacheService = serviceProvider.GetRequiredService<ICacheService>();
            var logger = serviceProvider.GetService<ILogger<GitHubService>>() ??
                        new NullLogger<GitHubService>();

            // Create a simple configuration that provides the GitHub settings
            var configDict = new Dictionary<string, string>
            {
                {"GitHub:RepositoryOwner", "Ivy-Interactive"},
                {"GitHub:RepositoryName", "Ivy-Framework"},
                {"GitHub:Token", Environment.GetEnvironmentVariable("GITHUB_TOKEN") ?? ""}
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configDict)
                .Build();

            return new GitHubService(httpClient, cacheService, logger, configuration);
        });

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