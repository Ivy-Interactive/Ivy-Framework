using Ivy;
using Ivy.Auth.Authelia;
using Ivy.Chrome;

var server = new Server();

server.UseHotReload();

server.UseAuth<AutheliaAuthProvider>();

server.AddAppsFromAssembly();

var settings = new ChromeSettings()
    .UseTabs(preventDuplicates: true)
    .DefaultApp<AutheliaExample.MainApp>();

server.UseChrome(settings);

server.SetMetaTitle("Authelia Example");

await server.RunAsync();
