using Ivy;

var server = new Server();

server.UseHotReload();

server.AddConnectionsFromAssembly();
server.AddAppsFromAssembly();

var settings = new AppShellSettings()
    .UseTabs(preventDuplicates: true)
    .DefaultApp<AutheliaExample.MainApp>();

server.UseAppShell(settings);

server.SetMetaTitle("Authelia Example");

Server.AuthCookiePrefix = "authelia";

await server.RunAsync();
