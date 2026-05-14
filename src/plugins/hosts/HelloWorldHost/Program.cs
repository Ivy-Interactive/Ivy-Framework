using Ivy;
using Ivy.Plugins;

var server = new Server();
server.UseAppShell(new AppShellSettings());
server.AddAppsFromAssembly(typeof(Program).Assembly);

var pluginsDir = Path.GetFullPath(
    Path.Combine(System.AppContext.BaseDirectory, "..", "..", "..", "..", "..", "plugins"));

var configFactory = new InMemoryPluginConfigFactory();
server.UsePlugins(pluginsDir, configFactory,
    sharedAssemblyNames: ["Ivy.Plugin.HelloWorld.Abstractions"],
    buildSourcePlugins: true);

await server.RunAsync();

internal class InMemoryPluginConfigFactory : IIvyPluginConfigFactory
{
    private readonly Dictionary<string, string> _store = new();
    private IPluginManager? _pluginManager;

    public void SetPluginManager(IPluginManager pluginManager) => _pluginManager = pluginManager;
    public IIvyPluginConfig Create(string pluginId) => new InMemoryPluginConfig(_store, pluginId, this);
    internal IPluginManager? PluginManager => _pluginManager;
}

internal class InMemoryPluginConfig(Dictionary<string, string> store, string pluginId, InMemoryPluginConfigFactory factory) : IIvyPluginConfig
{
    private string FullKey(string key) => $"{pluginId}:{key}";

    public string? GetValue(string key) =>
        store.TryGetValue(FullKey(key), out var value) ? value : null;

    public void SetValue(string key, string value) => store[FullKey(key)] = value;
    public void RemoveValue(string key) => store.Remove(FullKey(key));

    public void Save() => factory.PluginManager?.ReconfigurePlugin(pluginId);
}
