using Ivy;

var server = new Server();

server.UseHotReload();

server.AddConnectionsFromAssembly();
server.AddAppsFromAssembly();

var settings = new AppShellSettings()
    .UseTabs(preventDuplicates: true)
    .DefaultApp<BasicAuthExample.MainApp>();

server.UseAppShell(settings);

server.SetMetaTitle("BasicAuth Example");

Server.AuthCookiePrefix = "basic";

await server.RunAsync();
