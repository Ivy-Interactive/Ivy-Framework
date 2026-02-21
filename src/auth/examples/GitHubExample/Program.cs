using Ivy;
using Ivy.Auth.GitHub;
using Ivy.Chrome;

var server = new Server();

server.UseHotReload();

server.UseAuth<GitHubAuthProvider>();

server.AddAppsFromAssembly();

var settings = new ChromeSettings()
    .UseTabs(preventDuplicates: true)
    .DefaultApp<GitHubExample.MainApp>();

server.UseChrome(settings);

server.SetMetaTitle("GitHub Example");

await server.RunAsync();
