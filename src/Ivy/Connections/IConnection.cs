using Microsoft.Extensions.DependencyInjection;

namespace Ivy.Connections;

public interface IConnection
{
    public string GetContext(string connectionPath);
    public string GetNamespace();
    public string GetName();
    public string GetConnectionType();
    public ConnectionEntity[] GetEntities();
    public void RegisterServices(IServiceCollection services);
}

public record ConnectionEntity(string Singular, string Plural);