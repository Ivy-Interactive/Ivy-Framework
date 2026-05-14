using Ivy;
using Ivy.Plugins;

var server = new Server();
server.UseAppShell(new AppShellSettings());
server.AddAppsFromAssembly(typeof(Program).Assembly);

var pluginsDir = Path.GetFullPath(
    Path.Combine(System.AppContext.BaseDirectory, "..", "..", "..", "..", "..", "plugins"));

server.UsePlugins(pluginsDir, new InMemoryPluginConfigFactory(),
    sharedAssemblyNames: ["Ivy.Plugin.HelloWorld.Abstractions"],
    buildSourcePlugins: true);

await server.RunAsync();

internal class InMemoryPluginConfigFactory : IIvyPluginConfigFactory
{
    private readonly Dictionary<string, string> _store = new();
    public IIvyPluginConfig Create(string pluginId) => new InMemoryPluginConfig(_store, pluginId);
}

internal class InMemoryPluginConfig(Dictionary<string, string> store, string pluginId) : IIvyPluginConfig
{
    private string FullKey(string key) => $"{pluginId}:{key}";

    public string? GetValue(string key) =>
        store.TryGetValue(FullKey(key), out var value) ? value : null;

    public void SetValue(string key, string value) => store[FullKey(key)] = value;
    public void RemoveValue(string key) => store.Remove(FullKey(key));
    public void Save() { }
}
