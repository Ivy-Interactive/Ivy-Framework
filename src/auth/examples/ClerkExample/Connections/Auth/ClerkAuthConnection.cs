using Ivy;
using Microsoft.Extensions.Configuration;
using Ivy.Auth.Clerk;

namespace ClerkExample.Connections.Auth;

public class ClerkAuthConnection : IConnection, IHaveSecrets
{
    public string GetContext(string connectionPath) => string.Empty;

    public string GetName() => "ClerkAuth";

    public string GetNamespace() => typeof(ClerkAuthConnection).Namespace ?? "";

    public string GetConnectionType() => "Auth";

    public ConnectionEntity[] GetEntities() => [];

    public void RegisterServices(Server server)
    {
        server.UseAuth<ClerkAuthProvider>(c => c.UseEmailPassword().UseGoogle().UseGithub());
    }

    public Secret[] GetSecrets() =>
    [
        new("Clerk:PublishableKey"),
        new("Clerk:SecretKey")
    ];

    public async Task<(bool ok, string? message)> TestConnection(IConfiguration config)
    {
        await Task.CompletedTask;
        return (true, "Clerk configured");
    }
}
