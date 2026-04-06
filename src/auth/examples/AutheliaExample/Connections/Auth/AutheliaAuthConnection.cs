using Ivy;
using Microsoft.Extensions.Configuration;
using Ivy.Auth.Authelia;

namespace AutheliaExample.Connections.Auth;

public class AutheliaAuthConnection : IConnection, IHaveSecrets
{
    public string GetContext(string connectionPath) => string.Empty;

    public string GetName() => "AutheliaAuth";

    public string GetNamespace() => typeof(AutheliaAuthConnection).Namespace ?? "";

    public string GetConnectionType() => "Auth";

    public ConnectionEntity[] GetEntities() => [];

    public void RegisterServices(Server server)
    {
        server.UseAuth<AutheliaAuthProvider>();
    }

    public Secret[] GetSecrets() =>
    [
        new("Authelia:Url")
    ];

    public async Task<(bool ok, string? message)> TestConnection(IConfiguration config)
    {
        await Task.CompletedTask;
        return (true, "Authelia configured");
    }
}
