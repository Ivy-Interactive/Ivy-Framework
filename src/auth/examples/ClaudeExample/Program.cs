using Ivy;
using Ivy.Desktop;

var server = new Server();

server.UseHotReload();

server.AddConnectionsFromAssembly();
server.AddAppsFromAssembly();

var settings = new AppShellSettings()
    .UseTabs(preventDuplicates: true)
    .DefaultApp<ClaudeExample.MainApp>();

server.UseAppShell(settings);

server.SetMetaTitle("Claude Example");

Server.AuthCookiePrefix = "claude";

var launchDesktop = args.Contains("--desktop") || args.Contains("-d");

if (launchDesktop)
{
    var window = new DesktopWindow(server)
        .Title("Claude Example")
        .AppId("ClaudeExample")
        .Size(1200, 800)
        .MinSize(800, 600)
        .UseDevTools();

    return window.Run();
}

await server.RunAsync();
return 0;
