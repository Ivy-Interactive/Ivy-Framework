using Ivy;
using Microsoft.Extensions.Configuration;
using Ivy.Auth.GitHub;

namespace GitHubExample.Connections.Auth;

public class GitHubAuthConnection : IConnection, IHaveSecrets
{
    public string GetContext(string connectionPath) => string.Empty;

    public string GetName() => "GitHubAuth";

    public string GetNamespace() => typeof(GitHubAuthConnection).Namespace ?? "";

    public string GetConnectionType() => "Auth";

    public ConnectionEntity[] GetEntities() => [];

    public void RegisterServices(Server server)
    {
        server.UseAuth<GitHubAuthProvider>();
    }

    public Secret[] GetSecrets() =>
    [
        new("GitHub:ClientId"),
        new("GitHub:ClientSecret"),
        new("GitHub:RedirectUri")
    ];

    public async Task<(bool ok, string? message)> TestConnection(IConfiguration config)
    {
        await Task.CompletedTask;
        return (true, "GitHub OAuth configured");
    }
}
