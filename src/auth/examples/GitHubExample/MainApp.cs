using Ivy;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

namespace GitHubExample;

[App(id: "auth-test", title: "Auth Test")]
public class MainApp : ViewBase
{
    public override object? Build()
    {
        var auth = UseService<IAuthService>();
        var userInfo = UseState<UserInfo?>();
        var oauthTokens = UseState<Dictionary<string, OAuthProviderToken>?>();
        var githubRepos = UseState<List<string>?>();

        UseEffect(async () =>
        {
            var info = await auth.GetUserInfoAsync();
            userInfo.Set(info);

            // Get OAuth provider tokens
            var tokens = await auth.GetOAuthProviderTokensAsync();
            oauthTokens.Set(tokens);
        });

        if (userInfo.Value is null)
        {
            return Text.P("Loading user data...");
        }

        var user = userInfo.Value;

        return Layout.Vertical(
            // Success Header
            Text.H2("Authentication Successful!").Color(Colors.Success),

            // Profile info
            Layout.Horizontal(
                 new Image(user.AvatarUrl ?? "").Size(64),
                 Layout.Vertical(
                     Text.H3(user.FullName ?? "User"),
                     Text.Muted(user.Email)
                 ).Gap(4).Align(Align.Center)
            ).Gap(20).Align(Align.Center),

            // OAuth Provider Tokens Section
            Text.H3("OAuth Provider Access"),
            oauthTokens.Value == null
                ? Text.P("OAuth tokens not available")
                : Layout.Vertical(
                    Text.P($"Connected providers: {string.Join(", ", oauthTokens.Value.Keys)}"),

                    // Example: Fetch user's repositories
                    oauthTokens.Value.ContainsKey("github")
                        ? Layout.Vertical(
                            new Button("Fetch My Repositories", async () =>
                            {
                                var githubToken = oauthTokens.Value["github"];
                                using var httpClient = new HttpClient();
                                httpClient.DefaultRequestHeaders.Authorization =
                                    new AuthenticationHeaderValue("Bearer", githubToken.AccessToken);
                                httpClient.DefaultRequestHeaders.UserAgent.Add(
                                    new ProductInfoHeaderValue("GitHubExample", "1.0"));

                                try
                                {
                                    var response = await httpClient.GetStringAsync("https://api.github.com/user/repos?per_page=10");
                                    using var doc = JsonDocument.Parse(response);
                                    var repos = doc.RootElement.EnumerateArray()
                                        .Select(repo => repo.GetProperty("full_name").GetString() ?? "Unknown")
                                        .ToList();
                                    githubRepos.Set(repos);
                                }
                                catch (Exception ex)
                                {
                                    githubRepos.Set([$"Error: {ex.Message}"]);
                                }
                            }, variant: ButtonVariant.Primary),
                            githubRepos.Value != null
                                ? Layout.Vertical(
                                    Text.P("Your repositories:"),
                                    Layout.Vertical(
                                        githubRepos.Value.Select(repo => Text.P($"• {repo}")).ToArray()
                                    )
                                ).Gap(10)
                                : null
                        ).Gap(10)
                        : Text.P("GitHub OAuth token not available")
                ).Gap(10)

        ).Gap(40).Padding(50).Align(Align.Center).Height(Size.Full());
    }
}
