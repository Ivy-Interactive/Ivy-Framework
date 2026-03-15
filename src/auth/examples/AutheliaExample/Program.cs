using Ivy;

var server = new Server();

server.UseHotReload();

server.AddConnectionsFromAssembly();
server.AddAppsFromAssembly();

var settings = new ChromeSettings()
    .UseTabs(preventDuplicates: true)
    .DefaultApp<AutheliaExample.MainApp>();

server.UseChrome(settings);

server.SetMetaTitle("Authelia Example");

await server.RunAsync();
