using Ivy;
using Ivy.Auth.Auth0;
using Ivy.Chrome;

var server = new Server();

server.UseHotReload();

server.UseAuth<Auth0AuthProvider>(auth => auth
    .UseEmailPassword()
    .UseGoogle());

server.AddAppsFromAssembly();

var settings = new ChromeSettings()
    .UseTabs(preventDuplicates: true)
    .DefaultApp<Auth0Example.MainApp>();

server.UseChrome(settings);

server.SetMetaTitle("Auth0 Example");

await server.RunAsync();
