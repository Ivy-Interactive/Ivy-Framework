using Ivy;
using Ivy.Auth;
using Microsoft.Extensions.Configuration;
using Ivy.Auth.Supabase;

namespace SupabaseExample.Connections.Auth;

public class SupabaseAuthConnection : IConnection, IHaveSecrets
{
    public string GetContext(string connectionPath) => string.Empty;

    public string GetName() => "SupabaseAuth";

    public string GetNamespace() => typeof(SupabaseAuthConnection).Namespace ?? "";

    public string GetConnectionType() => "Auth";

    public ConnectionEntity[] GetEntities() => [];

    public void RegisterServices(Server server)
    {
        server.UseAuth<SupabaseAuthProvider>(c => c.UseEmailPassword().UseGoogle().UseGithub());
    }

    public Secret[] GetSecrets() =>
    [
        new("Supabase:Url"),
        new("Supabase:ApiKey")
    ];

    public async Task<(bool ok, string? message)> TestConnection(IConfiguration config)
    {
        await Task.CompletedTask;
        return (true, "Supabase configured");
    }
}
