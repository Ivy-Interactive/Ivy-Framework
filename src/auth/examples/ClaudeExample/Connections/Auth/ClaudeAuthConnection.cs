using Ivy;
using Ivy.Auth.Claude;
using Microsoft.Extensions.Configuration;

namespace ClaudeExample.Connections.Auth;

public class ClaudeAuthConnection : IConnection, IHaveSecrets
{
    public string GetContext(string connectionPath) => string.Empty;

    public string GetName() => "ClaudeAuth";

    public string GetNamespace() => typeof(ClaudeAuthConnection).Namespace ?? "";

    public string GetConnectionType() => "Auth";

    public ConnectionEntity[] GetEntities() => [];

    public void RegisterServices(Server server)
    {
        server.UseAuth<ClaudeAuthProvider>();
    }

    public Secret[] GetSecrets() =>
    [
        new("Claude:ClientId"),
        new("Claude:ClientSecret"),
        new("Claude:RedirectUri")
    ];

    public async Task<(bool ok, string? message)> TestConnection(IConfiguration config)
    {
        await Task.CompletedTask;
        return (true, "Claude OAuth configured");
    }
}
