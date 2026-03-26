using Ivy;
using Microsoft.Extensions.Configuration;

var server = new Server();

server.UseHotReload();

server.AddConnectionsFromAssembly();
server.AddAppsFromAssembly();

var settings = new AppShellSettings()
    .UseTabs(preventDuplicates: true)
    .DefaultApp<ClerkExample.MainApp>();

server.UseAppShell(settings);

server.SetMetaTitle("Clerk Example");

server.UseConfiguration(config =>
{
    if (ProcessHelper.IsProduction())
    {
        var secretsPath = Environment.GetEnvironmentVariable("CLERK_SECRETS_PATH");
        if (!string.IsNullOrEmpty(secretsPath))
        {
            config.AddJsonFile(secretsPath, optional: true);
        }
    }
});

await server.RunAsync();
