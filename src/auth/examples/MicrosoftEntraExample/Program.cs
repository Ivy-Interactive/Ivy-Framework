using Ivy;

var server = new Server();

server.UseHotReload();

server.AddConnectionsFromAssembly();
server.AddAppsFromAssembly();

var settings = new ChromeSettings()
    .UseTabs(preventDuplicates: true)
    .DefaultApp<MicrosoftEntraExample.MainApp>();

server.UseChrome(settings);

server.SetMetaTitle("Microsoft Entra Example");

await server.RunAsync();
