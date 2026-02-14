using Ivy;
using Ivy.Auth.Clerk;
using Ivy.Chrome;

var server = new Server();

server.UseHotReload();

server.UseAuth<ClerkAuthProvider>(auth => auth
    .UseEmailPassword()
    .UseGoogle());

server.AddAppsFromAssembly();

var settings = new ChromeSettings()
    .UseTabs(preventDuplicates: true)
    .DefaultApp<ClerkExample.MainApp>();

server.UseChrome(settings);

server.SetMetaTitle("Clerk Example");

await server.RunAsync();
