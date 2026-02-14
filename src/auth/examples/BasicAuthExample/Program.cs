using Ivy;
using Ivy.Auth;
using Ivy.Chrome;

var server = new Server();

server.UseHotReload();

server.UseAuth<BasicAuthProvider>();

server.AddAppsFromAssembly();

var settings = new ChromeSettings()
    .UseTabs(preventDuplicates: true)
    .DefaultApp<BasicAuthExample.MainApp>();

server.UseChrome(settings);

server.SetMetaTitle("BasicAuth Example");

await server.RunAsync();
