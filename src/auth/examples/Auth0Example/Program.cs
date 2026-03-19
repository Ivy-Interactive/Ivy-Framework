using Ivy;

var server = new Server();

server.UseHotReload();

server.AddConnectionsFromAssembly();
server.AddAppsFromAssembly();

var settings = new ChromeSettings()
    .UseTabs(preventDuplicates: true)
    .DefaultApp<Auth0Example.MainApp>();

server.UseChrome(settings);

server.SetMetaTitle("Auth0 Example");

await server.RunAsync();
