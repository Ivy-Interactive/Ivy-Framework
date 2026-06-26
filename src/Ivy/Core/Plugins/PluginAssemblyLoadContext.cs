using System.Reflection;
using System.Runtime.Loader;

namespace Ivy.Core.Plugins;

internal class PluginAssemblyLoadContext(string pluginPath, IReadOnlySet<string> sharedAssemblyNames)
    : AssemblyLoadContext(isCollectible: true)
{
    private readonly AssemblyDependencyResolver _resolver = new(pluginPath);

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        if (assemblyName.Name is null)
            return null;

        // Shared assemblies are loaded from the host so types match across contexts.
        // Explicitly load by name (ignoring version) from the Default context to handle
        // version mismatches between NuGet-referenced plugins and locally-built host assemblies.
        if (sharedAssemblyNames.Contains(assemblyName.Name))
            return Default.LoadFromAssemblyName(new AssemblyName(assemblyName.Name));

        var path = _resolver.ResolveAssemblyToPath(assemblyName);
        return path != null ? LoadFromAssemblyPath(path) : null;
    }
}
