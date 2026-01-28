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
        
        // Configure BasicAuth for testing (test user: admin/password)
        // Generate test secrets for BasicAuth
        var hashSecretBytes = Encoding.UTF8.GetBytes("test-hash-secret-for-samples-12345");
        var hashSecret = Convert.ToBase64String(hashSecretBytes);
        var jwtSecret = Convert.ToBase64String(Encoding.UTF8.GetBytes("test-jwt-secret-for-samples-1234567890123456"));
        
        // Generate Argon2 hash for password "password"
        // Note: For now, we're generating without Secret to get login working
        // TODO: Fix Secret/pepper support - Argon2.Hash(config) with Secret returns incomplete hash
        var passwordHash = Argon2.Hash("password");
        
        // Verify the hash works (without Secret for now)
        var verifyConfig = new Argon2Config
        {
            Password = Encoding.UTF8.GetBytes("password"),
            // Not including Secret for now - hash was generated without it
        };
        if (!Argon2.Verify(passwordHash, verifyConfig))
        {
            throw new Exception("Generated hash does not verify correctly!");
        }
        
        // Set environment variables for BasicAuthProvider (it reads from env vars and user secrets)
        // Note: .NET Configuration uses double underscores for nested keys in environment variables
        Environment.SetEnvironmentVariable("BasicAuth__HashSecret", hashSecret);
        Environment.SetEnvironmentVariable("BasicAuth__JwtSecret", jwtSecret);
        Environment.SetEnvironmentVariable("BasicAuth__JwtIssuer", "ivy-samples");
        Environment.SetEnvironmentVariable("BasicAuth__JwtAudience", "ivy-samples-app");
        Environment.SetEnvironmentVariable("BasicAuth__Users", $"admin:{passwordHash}");
        
        var server = new Server(args);
        server.UseHotReload();
        server.AddAppsFromAssembly(typeof(SamplesServer).Assembly);
        
        // Add DefaultAuthApp to samples
        server.AddApp<DefaultAuthApp>();
        
        // Use BasicAuth for testing (email/password is the default flow)
        server.UseAuth<BasicAuthProvider>();

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