using Ivy;
using Ivy.Auth.GitHub;

var server = new Server();

server.UseHotReload();

server.AddConnectionsFromAssembly();
server.AddAppsFromAssembly();

var settings = new ChromeSettings()
    .UseTabs(preventDuplicates: true)
    .DefaultApp<GitHubExample.MainApp>();

server.UseChrome(settings);

server.SetMetaTitle("GitHub Example");

await server.RunAsync();
