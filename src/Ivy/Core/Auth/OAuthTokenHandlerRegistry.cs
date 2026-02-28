using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Ivy.Core.Auth;

public class OAuthTokenHandlerRegistry : IOAuthTokenHandlerRegistry
{
    private readonly Dictionary<OAuthProvider, IAuthTokenHandler> _handlers = new();
    private readonly ILogger<OAuthTokenHandlerRegistry>? _logger;

    public OAuthTokenHandlerRegistry(ILogger<OAuthTokenHandlerRegistry>? logger = null)
    {
        _logger = logger;
        DiscoverAndRegisterHandlers();
    }

    private void DiscoverAndRegisterHandlers()
    {
        _logger?.LogInformation("OAuthTokenHandlerRegistry: Starting discovery of OAuth token handlers");
        try
        {
            // Get all loaded assemblies
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            // Also try to load referenced Ivy.Auth.* assemblies from entry assembly
            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly != null)
            {
                var referencedAssemblyNames = entryAssembly.GetReferencedAssemblies()
                    .Where(name => name.Name?.StartsWith("Ivy.Auth.") == true)
                    .ToList();

                foreach (var assemblyName in referencedAssemblyNames)
                {
                    try
                    {
                        Assembly.Load(assemblyName);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogDebug(ex, "Could not load referenced assembly {AssemblyName}", assemblyName.Name);
                    }
                }
            }

            // Now get all loaded assemblies (including newly loaded ones)
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            _logger?.LogInformation("OAuthTokenHandlerRegistry: Scanning {AssemblyCount} assemblies", assemblies.Length);

            foreach (var assembly in assemblies)
            {
                try
                {
                    // Skip system assemblies for performance
                    var assemblyName = assembly.GetName().Name ?? "";
                    if (assemblyName.StartsWith("System.") ||
                        assemblyName.StartsWith("Microsoft.") ||
                        assemblyName == "netstandard" ||
                        assemblyName == "mscorlib")
                    {
                        continue;
                    }

                    // Find all types with OAuthTokenHandlerAttribute
                    var handlerTypes = assembly.GetTypes()
                        .Where(t => t.IsClass && !t.IsAbstract && typeof(IAuthTokenHandler).IsAssignableFrom(t))
                        .Where(t => t.GetCustomAttribute<OAuthTokenHandlerAttribute>() != null)
                        .ToList();

                    foreach (var handlerType in handlerTypes)
                    {
                        var attribute = handlerType.GetCustomAttribute<OAuthTokenHandlerAttribute>();
                        if (attribute == null)
                            continue;

                        try
                        {
                            // Create an instance with HttpClient
                            var httpClient = new HttpClient();
                            // Set a default User-Agent header (required by some APIs like GitHub)
                            httpClient.DefaultRequestHeaders.Add("User-Agent", "Ivy-Framework/1.0");
                            var handler = (IAuthTokenHandler?)Activator.CreateInstance(handlerType, httpClient);

                            if (handler != null)
                            {
                                Register(attribute.Provider, handler);
                                _logger?.LogInformation("Registered OAuth token handler for {Provider}: {HandlerType}",
                                    attribute.Provider, handlerType.Name);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogWarning(ex, "Failed to instantiate OAuth token handler for {Provider}: {HandlerType}",
                                attribute.Provider, handlerType.Name);
                        }
                    }
                }
                catch (ReflectionTypeLoadException ex)
                {
                    _logger?.LogDebug("Could not load types from assembly {Assembly}: {Message}",
                        assembly.FullName, ex.Message);
                }
                catch (Exception ex)
                {
                    _logger?.LogDebug(ex, "Error scanning assembly {Assembly} for OAuth token handlers",
                        assembly.FullName);
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "OAuthTokenHandlerRegistry: Error discovering OAuth token handlers");
        }

        _logger?.LogInformation("OAuthTokenHandlerRegistry: Discovery complete. Registered {Count} handlers: {Providers}",
            _handlers.Count, string.Join(", ", _handlers.Keys));
    }

    public void Register(OAuthProvider provider, IAuthTokenHandler handler)
    {
        _handlers[provider] = handler;
    }

    public IAuthTokenHandler? GetHandler(OAuthProvider provider)
    {
        return _handlers.TryGetValue(provider, out var handler) ? handler : null;
    }
}
