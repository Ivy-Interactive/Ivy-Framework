using Ivy.Samples.Shared.Apps.Demos;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;

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

        if (server.Configuration.GetValue<string>("OpenAi:ApiKey") is { } openAiApiKey &&
           server.Configuration.GetValue<string>("OpenAi:Endpoint") is { } openAiEndpoint)
        {
            var openAiClient = new OpenAIClient(new System.ClientModel.ApiKeyCredential(openAiApiKey), new OpenAIClientOptions
            {
                Endpoint = new Uri(openAiEndpoint)
            });

            var openAiChatClient = openAiClient.GetChatClient("gpt-4o");
            var chatClient = openAiChatClient.AsIChatClient();
            server.Services.AddSingleton<IChatClient>(chatClient);
        }

        await server.RunAsync();
    }
}