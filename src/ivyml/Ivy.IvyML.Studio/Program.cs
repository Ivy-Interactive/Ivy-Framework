using Ivy;
using Ivy.IvyML.Studio.Apps;

var server = new Server();
server.UseHotReload();
server.AddApp<StudioApp>(isDefault: true);
await server.RunAsync();
