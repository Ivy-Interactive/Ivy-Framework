using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Ivy.Core.Server;

public class ServerDescription
{
    [YamlMember(Alias = "apps")]
    public List<AppDescription> Apps { get; set; } = new();

    [YamlMember(Alias = "connections")]
    public List<ConnectionDescription> Connections { get; set; } = new();

    [YamlMember(Alias = "secrets")]
    public List<SecretDescription> Secrets { get; set; } = new();

    [YamlMember(Alias = "services")]
    public List<ServiceDescription> Services { get; set; } = new();

    public static ServerDescription Gather(global::Ivy.Server server, IServiceProvider serviceProvider)
    {
        var description = new ServerDescription();

        // Gather apps
        server.AppRepository.Reload();
        foreach (var app in server.AppRepository.All())
        {
            description.Apps.Add(new AppDescription
            {
                Name = app.Title,
                Id = app.Id,
                IsVisible = app.IsVisible
            });
        }

        // Gather connections from registered services
        var connections = serviceProvider.GetServices<IConnection>();
        foreach (var connection in connections)
        {
            description.Connections.Add(new ConnectionDescription
            {
                Name = connection.GetName(),
                ConnectionType = connection.GetConnectionType(),
                Namespace = connection.GetNamespace()
            });
        }

        // Also try to find connections from the assembly that may not be registered
        var assembly = Assembly.GetEntryAssembly();
        if (assembly != null)
        {
            var connectionTypes = assembly.GetTypes()
                .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IConnection).IsAssignableFrom(t));

            foreach (var type in connectionTypes)
            {
                // Skip if already found in registered services
                if (connections.Any(c => c.GetType() == type))
                    continue;

                try
                {
                    var connection = (IConnection)Activator.CreateInstance(type)!;
                    description.Connections.Add(new ConnectionDescription
                    {
                        Name = connection.GetName(),
                        ConnectionType = connection.GetConnectionType(),
                        Namespace = connection.GetNamespace()
                    });
                }
                catch
                {
                    // Skip connections that can't be instantiated
                }
            }
        }

        // Gather secrets from IHaveSecrets implementations
        var secretsProviders = serviceProvider.GetServices<IHaveSecrets>();
        var secretEntries = new Dictionary<string, string?>();
        foreach (var provider in secretsProviders)
        {
            var secrets = provider.GetSecrets();
            foreach (var secret in secrets)
            {
                secretEntries.TryAdd(secret.Key, secret.Preset);
            }
        }

        // Also check assembly for IHaveSecrets that may not be registered
        if (assembly != null)
        {
            var secretProviderTypes = assembly.GetTypes()
                .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IHaveSecrets).IsAssignableFrom(t));

            foreach (var type in secretProviderTypes)
            {
                // Skip if already found in registered services
                if (secretsProviders.Any(p => p.GetType() == type))
                    continue;

                try
                {
                    if (Activator.CreateInstance(type) is IHaveSecrets provider)
                    {
                        var secrets = provider.GetSecrets();
                        foreach (var secret in secrets)
                        {
                            secretEntries.TryAdd(secret.Key, secret.Preset);
                        }
                    }
                }
                catch
                {
                    // Skip providers that can't be instantiated
                }
            }
        }
        description.Secrets = secretEntries
            .Select(kvp => new SecretDescription { Key = kvp.Key, Preset = kvp.Value })
            .ToList();

        // Gather all registered services from the DI container
        if (serviceProvider is ServiceProvider sp)
        {
            // Get the service collection from the service provider
            var serviceCollection = server.Services;

            // First add services from server's Services collection
            foreach (var service in serviceCollection)
            {
                string? descriptionYaml = null;
                string? implementationTypeName = service.ImplementationType?.FullName ?? service.ImplementationType?.Name;

                // Try to get the actual instance to determine implementation type and description
                try
                {
                    var instance = serviceProvider.GetService(service.ServiceType);
                    if (instance != null)
                    {
                        // Get the actual implementation type from the instance
                        var actualType = instance.GetType();
                        if (implementationTypeName == null)
                        {
                            implementationTypeName = actualType.FullName ?? actualType.Name;
                        }

                        // Check if it implements IDescribableService
                        if (instance is IDescribableService describable)
                        {
                            descriptionYaml = describable.ToYaml();
                        }
                    }
                }
                catch
                {
                    // If we can't get the instance, continue with what we have
                }

                var serviceDesc = new ServiceDescription
                {
                    ServiceType = service.ServiceType.FullName ?? service.ServiceType.Name,
                    ImplementationType = implementationTypeName,
                    Lifetime = service.Lifetime.ToString(),
                    Description = descriptionYaml
                };
                description.Services.Add(serviceDesc);
            }
        }

        return description;
    }

    public static IConnection? FindConnection(global::Ivy.Server server, IServiceProvider serviceProvider, string name)
    {
        // Check registered IConnection services first
        var connections = serviceProvider.GetServices<IConnection>();
        var match = connections.FirstOrDefault(c =>
            string.Equals(c.GetName(), name, StringComparison.OrdinalIgnoreCase));
        if (match != null) return match;

        // Fall back to assembly scanning
        var assembly = Assembly.GetEntryAssembly();
        if (assembly == null) return null;

        var connectionTypes = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IConnection).IsAssignableFrom(t));

        foreach (var type in connectionTypes)
        {
            if (connections.Any(c => c.GetType() == type))
                continue;

            try
            {
                var connection = (IConnection)Activator.CreateInstance(type)!;
                if (string.Equals(connection.GetName(), name, StringComparison.OrdinalIgnoreCase))
                    return connection;
            }
            catch
            {
                // Skip connections that can't be instantiated
            }
        }

        return null;
    }

    public string ToYaml()
    {
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        return serializer.Serialize(this);
    }
}

public class AppDescription
{
    [YamlMember(Alias = "name")]
    public string Name { get; set; } = string.Empty;

    [YamlMember(Alias = "id")]
    public string Id { get; set; } = string.Empty;

    [YamlMember(Alias = "isVisible")]
    public bool IsVisible { get; set; }
}

public class ConnectionDescription
{
    [YamlMember(Alias = "name")]
    public string Name { get; set; } = string.Empty;

    [YamlMember(Alias = "connectionType")]
    public string ConnectionType { get; set; } = string.Empty;

    [YamlMember(Alias = "namespace")]
    public string Namespace { get; set; } = string.Empty;
}

public class ServiceDescription
{
    [YamlMember(Alias = "serviceType")]
    public string ServiceType { get; set; } = string.Empty;

    [YamlMember(Alias = "implementationType")]
    public string? ImplementationType { get; set; }

    [YamlMember(Alias = "lifetime")]
    public string Lifetime { get; set; } = string.Empty;

    [YamlMember(Alias = "description")]
    public string? Description { get; set; }
}

public class SecretDescription
{
    [YamlMember(Alias = "key")]
    public string Key { get; set; } = string.Empty;

    [YamlMember(Alias = "preset")]
    public string? Preset { get; set; }
}