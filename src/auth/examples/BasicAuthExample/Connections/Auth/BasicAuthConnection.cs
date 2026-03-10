using Ivy;
using Microsoft.Extensions.Configuration;

namespace BasicAuthExample.Connections.Auth;

public class BasicAuthConnection : IConnection, IHaveSecrets
{
    public string GetContext(string connectionPath) => string.Empty;

    public string GetName() => "BasicAuth";

    public string GetNamespace() => typeof(BasicAuthConnection).Namespace ?? "";

    public string GetConnectionType() => "Auth";

    public ConnectionEntity[] GetEntities() => [];

    public void RegisterServices(Server server)
    {
        server.UseAuth<BasicAuthProvider>();
    }

    public Secret[] GetSecrets() =>
    [
        new("BasicAuth:Users"),
        new("BasicAuth:HashSecret"),
        new("BasicAuth:JwtSecret"),
        new("BasicAuth:JwtIssuer"),
        new("BasicAuth:JwtAudience")
    ];

    public async Task<(bool ok, string? message)> TestConnection(IConfiguration config)
    {
        await Task.CompletedTask;
        return (true, "Basic Auth configured");
    }
}
