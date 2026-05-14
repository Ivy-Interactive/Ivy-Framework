using Ivy;

var server = new Server();
server.UseAppShell(new AppShellSettings());
server.AddAppsFromAssembly(typeof(Program).Assembly);

var pluginsDir = Path.GetFullPath(
    Path.Combine(System.AppContext.BaseDirectory, "..", "..", "..", "..", "..", "plugins"));

server.UsePlugins(pluginsDir,
    sharedAssemblyNames: ["Ivy.Plugin.HelloWorld.Abstractions"],
    buildSourcePlugins: true);

await server.RunAsync();
