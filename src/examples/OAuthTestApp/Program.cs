using Ivy;
using Ivy.Auth.GitHub;

var server = new Server();

server.UseHotReload();

server.Services.AddHttpClient("GitHubAuth", client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "Ivy-Framework");
    client.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
    client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
});

server.Services.AddSingleton(server.Configuration);

server.UseAuth<GitHubAuthProvider>(c => c.UseGitHub());

server.AddAppsFromAssembly();

server.UseChrome();

server.SetMetaTitle("OAuth Test App");

await server.RunAsync();
