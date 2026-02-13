using Microsoft.Extensions.Configuration;

namespace Ivy.Connections;

public interface IConnection
{
    public string GetContext(string connectionPath);
    public string GetNamespace();
    public string GetName();
    public string GetConnectionType();
    public ConnectionEntity[] GetEntities();
    public void RegisterServices(Server server);
    public Task<(bool ok, string? message)> TestConnection(IConfiguration config);
}

public record ConnectionEntity(string Singular, string Plural);