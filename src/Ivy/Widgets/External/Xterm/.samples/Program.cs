using Ivy;

var server = new Server();
server.UseAppShell();
server.AddAppsFromAssembly();
await server.RunAsync();
