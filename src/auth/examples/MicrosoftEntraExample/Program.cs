using Ivy;
using Ivy.Auth.MicrosoftEntra;
using Ivy.Chrome;

var server = new Server();

server.UseHotReload();

server.UseAuth<MicrosoftEntraAuthProvider>();

server.AddAppsFromAssembly();

var settings = new ChromeSettings()
    .UseTabs(preventDuplicates: true)
    .DefaultApp<MicrosoftEntraExample.MainApp>();

server.UseChrome(settings);

server.SetMetaTitle("Microsoft Entra Example");

await server.RunAsync();
