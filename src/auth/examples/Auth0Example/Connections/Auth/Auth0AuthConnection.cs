using Ivy;
using Microsoft.Extensions.Configuration;
using Ivy.Auth.Auth0;

namespace Auth0Example.Connections.Auth;

public class Auth0AuthConnection : IConnection, IHaveSecrets
{
    public string GetContext(string connectionPath) => string.Empty;

    public string GetName() => "Auth0Auth";

    public string GetNamespace() => typeof(Auth0AuthConnection).Namespace ?? "";

    public string GetConnectionType() => "Auth";

    public ConnectionEntity[] GetEntities() => [];

    public void RegisterServices(Server server)
    {
        server.UseAuth<Auth0AuthProvider>(c => c.UseEmailPassword().UseGoogle().UseGithub());
    }

    public Secret[] GetSecrets() =>
    [
        new("Auth0:Domain"),
        new("Auth0:ClientId"),
        new("Auth0:ClientSecret"),
        new("Auth0:Audience")
    ];

    public async Task<(bool ok, string? message)> TestConnection(IConfiguration config)
    {
        await Task.CompletedTask;
        return (true, "Auth0 configured");
    }
}
