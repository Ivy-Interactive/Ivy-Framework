using Ivy;

var server = new Server();

server.UseHotReload();

server.AddConnectionsFromAssembly();
server.AddAppsFromAssembly();

var settings = new AppShellSettings()
    .UseTabs(preventDuplicates: true)
    .DefaultApp<Auth0Example.MainApp>();

server.UseAppShell(settings);

server.SetMetaTitle("Auth0 Example");

Server.AuthCookiePrefix = "auth0";

await server.RunAsync();
