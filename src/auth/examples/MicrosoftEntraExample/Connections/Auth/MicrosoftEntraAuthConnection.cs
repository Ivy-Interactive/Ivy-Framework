using Ivy;
using Microsoft.Extensions.Configuration;
using Ivy.Auth.MicrosoftEntra;

namespace MicrosoftEntraExample.Connections.Auth;

public class MicrosoftEntraAuthConnection : IConnection, IHaveSecrets
{
    public string GetContext(string connectionPath) => string.Empty;

    public string GetName() => "MicrosoftEntraAuth";

    public string GetNamespace() => typeof(MicrosoftEntraAuthConnection).Namespace ?? "";

    public string GetConnectionType() => "Auth";

    public ConnectionEntity[] GetEntities() => [];

    public void RegisterServices(Server server)
    {
        server.UseAuth<MicrosoftEntraAuthProvider>();
    }

    public Secret[] GetSecrets() =>
    [
        new("MicrosoftEntra:TenantId"),
        new("MicrosoftEntra:ClientId"),
        new("MicrosoftEntra:ClientSecret")
    ];

    public async Task<(bool ok, string? message)> TestConnection(IConfiguration config)
    {
        await Task.CompletedTask;
        return (true, "Microsoft Entra ID configured");
    }
}
