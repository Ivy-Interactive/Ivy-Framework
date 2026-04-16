using Ivy;

var server = new Server();

server.UseHotReload();

server.AddConnectionsFromAssembly();
server.AddAppsFromAssembly();

var settings = new AppShellSettings()
    .UseTabs(preventDuplicates: true)
    .DefaultApp<MicrosoftEntraExample.MainApp>();

server.UseAppShell(settings);

server.SetMetaTitle("Microsoft Entra Example");

Server.AuthCookiePrefix = "entra";

await server.RunAsync();
