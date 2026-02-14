using Ivy;
using Ivy.Auth.GitHub;

var server = new Server();

server.UseHotReload();

server.UseAuth<GitHubAuthProvider>(c => c.UseGitHub());

server.AddAppsFromAssembly();

server.UseChrome();

server.SetMetaTitle("OAuth Test App");

await server.RunAsync();
