using Microsoft.Extensions.DependencyInjection;
using Ivy.Samples.Shared.Helpers;
using Ivy.Samples.Shared.Apps.Demos;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;

namespace Ivy.Samples.Shared;

public static class SamplesServer
{
    public static async Task RunAsync(ServerArgs? args = null)
    {
        var server = new Server(args);
        server.UseCulture("en-US");
        server.UseHotReload();
        server.AddAppsFromAssembly(typeof(SamplesServer).Assembly);

        var version = typeof(Server).Assembly.GetName().Version!.ToString().EatRight(".0");

        // For staging deployments (version = "0"): show build date from DLL timestamp
        string versionLabel;
        if (version == "0")
        {
            var buildDate = DateTime.UtcNow.ToString("MMM d · HH:mm UTC");
            versionLabel = $"Deployed {buildDate}";
        }
        else
        {
            versionLabel = $"Version {version}";
        }

        server.SetMetaTitle($"Ivy Samples {versionLabel}");

        var appShellSettings = new AppShellSettings()
            .Header(
                Layout.Vertical().Padding(2)
                | new IvyLogo()
                | Text.Muted(versionLabel)
            )
            .DefaultApp<HelloApp>()
            .UseTabs(preventDuplicates: true);
        server.UseAppShell(() => new DefaultSidebarAppShell(appShellSettings));

        server.Services.AddSingleton<SampleDbContextFactory>();
        server.Services.AddSingleton<Apps.Widgets.MockEmployeeService>();

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
