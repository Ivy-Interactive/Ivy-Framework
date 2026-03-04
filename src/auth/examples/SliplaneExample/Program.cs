using Ivy;
using Ivy.Auth.Sliplane;
using Ivy.Chrome;

var server = new Server();

server.UseHotReload();

server.UseAuth<SliplaneAuthProvider>();

server.AddAppsFromAssembly();

var settings = new ChromeSettings()
    .UseTabs(preventDuplicates: true)
    .DefaultApp<SliplaneExample.MainApp>();

server.UseChrome(settings);

server.SetMetaTitle("Sliplane Example");

await server.RunAsync();

