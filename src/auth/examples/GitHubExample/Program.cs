using Ivy;

var server = new Server();

server.UseHotReload();

server.AddConnectionsFromAssembly();
server.AddAppsFromAssembly();

var settings = new AppShellSettings()
    .UseTabs(preventDuplicates: true)
    .DefaultApp<GitHubExample.MainApp>();

server.UseAppShell(settings);

server.SetMetaTitle("GitHub Example");

await server.RunAsync();
