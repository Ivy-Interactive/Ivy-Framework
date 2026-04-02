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
        var secretsPath = Environment.GetEnvironmentVariable("IVY_CLERK_SECRETS_PATH");
        if (!string.IsNullOrEmpty(secretsPath))
        {
            if (!File.Exists(secretsPath))
            {
                throw new FileNotFoundException(
                    $"Clerk secrets file not found at path specified by IVY_CLERK_SECRETS_PATH: '{secretsPath}'");
            }
            config.AddJsonFile(secretsPath, optional: false);
        }
    }
});

await server.RunAsync();
