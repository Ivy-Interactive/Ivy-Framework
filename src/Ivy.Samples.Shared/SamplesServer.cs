using Ivy.Auth;
using Ivy.Samples.Shared.Apps.Demos;
using Isopoh.Cryptography.Argon2;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using System.Text;

namespace Ivy.Samples.Shared;

public static class SamplesServer
{
    public static async Task RunAsync(ServerArgs? args = null)
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");

        var hashSecretBytes = Encoding.UTF8.GetBytes("test-hash-secret-for-samples-12345");
        var hashSecret = Convert.ToBase64String(hashSecretBytes);
        var jwtSecret = Convert.ToBase64String(Encoding.UTF8.GetBytes("test-jwt-secret-for-samples-1234567890123456"));

        var passwordHash = Argon2.Hash("password");

        var verifyConfig = new Argon2Config
        {
            Password = Encoding.UTF8.GetBytes("password"),
        };
        if (!Argon2.Verify(passwordHash, verifyConfig))
        {
            throw new Exception("Generated hash does not verify correctly!");
        }

        Environment.SetEnvironmentVariable("BasicAuth__HashSecret", hashSecret);
        Environment.SetEnvironmentVariable("BasicAuth__JwtSecret", jwtSecret);
        Environment.SetEnvironmentVariable("BasicAuth__JwtIssuer", "ivy-samples");
        Environment.SetEnvironmentVariable("BasicAuth__JwtAudience", "ivy-samples-app");
        Environment.SetEnvironmentVariable("BasicAuth__Users", $"admin:{passwordHash}");

        var server = new Server(args);
        server.UseHotReload();
        server.AddAppsFromAssembly(typeof(SamplesServer).Assembly);
        server.AddAppsFromAssembly(typeof(DefaultAuthApp).Assembly);

        server.Services.AddSingleton<BasicAuthProvider>();
        server.Services.AddSingleton<IAuthProvider, BasicAuthProvider>(s => s.GetRequiredService<BasicAuthProvider>());

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