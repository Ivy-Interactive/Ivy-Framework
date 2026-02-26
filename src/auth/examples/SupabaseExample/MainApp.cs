using Ivy;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace SupabaseExample;

[App(id: "auth-test", title: "Auth Test")]
public class MainApp : ViewBase
{
    public override object? Build()
    {
        var auth = UseService<IAuthService>();
        var userInfo = UseState<UserInfo?>();
        var oauthTokens = UseState<Dictionary<OAuthProvider, OAuthProviderToken>?>();
        var googleProfile = UseState<string?>();
        var githubRepos = UseState<string?>();

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
            Text.H3("OAuth Provider Tokens"),
            oauthTokens.Value == null
                ? Text.P("OAuth tokens not available")
                : oauthTokens.Value.Count == 0
                    ? Text.P("No OAuth providers connected")
                    : Layout.Vertical(
                        Text.P($"Connected providers: {string.Join(", ", oauthTokens.Value.Keys)}"),

                        // Example: Test Google API access if available
                        oauthTokens.Value.ContainsKey(OAuthProvider.Google)
                            ? Layout.Vertical(
                                Text.H4("Google OAuth Test"),
                                Layout.Horizontal(
                                    new Button("Get Google Profile", async () =>
                                    {
                                        var googleToken = oauthTokens.Value[OAuthProvider.Google];
                                        using var httpClient = new HttpClient();
                                        httpClient.DefaultRequestHeaders.Authorization =
                                            new AuthenticationHeaderValue("Bearer", googleToken.AuthToken.AccessToken);

                                        try
                                        {
                                            var response = await httpClient.GetStringAsync(
                                                "https://www.googleapis.com/oauth2/v2/userinfo");
                                            googleProfile.Set(response);
                                        }
                                        catch (Exception ex)
                                        {
                                            googleProfile.Set($"{{\"error\": \"{ex.Message}\"}}");
                                        }
                                    }, variant: ButtonVariant.Primary),
                                    new Button("List Google Drive Files", async () =>
                                    {
                                        var googleToken = oauthTokens.Value[OAuthProvider.Google];
                                        using var httpClient = new HttpClient();
                                        httpClient.DefaultRequestHeaders.Authorization =
                                            new AuthenticationHeaderValue("Bearer", googleToken.AuthToken.AccessToken);

                                        try
                                        {
                                            var response = await httpClient.GetStringAsync(
                                                "https://www.googleapis.com/drive/v3/files?pageSize=10");
                                            googleProfile.Set(response);
                                        }
                                        catch (Exception ex)
                                        {
                                            googleProfile.Set($"{{\"error\": \"{ex.Message}\"}}");
                                        }
                                    }, variant: ButtonVariant.Outline)
                                ).Gap(10),
                                googleProfile.Value != null
                                    ? Text.Json(googleProfile.Value)
                                    : null
                            ).Gap(10)
                            : null,

                        // Example: Test GitHub API access if available
                        oauthTokens.Value.ContainsKey(OAuthProvider.GitHub)
                            ? Layout.Vertical(
                                Text.H4("GitHub OAuth Test"),
                                new Button("Fetch My Repositories", async () =>
                                {
                                    var githubToken = oauthTokens.Value[OAuthProvider.GitHub];
                                    using var httpClient = new HttpClient();
                                    httpClient.DefaultRequestHeaders.Authorization =
                                        new AuthenticationHeaderValue("Bearer", githubToken.AuthToken.AccessToken);
                                    httpClient.DefaultRequestHeaders.UserAgent.Add(
                                        new ProductInfoHeaderValue("SupabaseExample", "1.0"));

                                    try
                                    {
                                        var response = await httpClient.GetStringAsync("https://api.github.com/user/repos");
                                        githubRepos.Set(response);
                                    }
                                    catch (Exception ex)
                                    {
                                        githubRepos.Set($"{{\"error\": \"{ex.Message}\"}}");
                                    }
                                }, variant: ButtonVariant.Primary),
                                githubRepos.Value != null
                                    ? Text.Json(githubRepos.Value)
                                    : null
                            ).Gap(10)
                            : null
                    ).Gap(10)

        ).Gap(40).Padding(50).Align(Align.Center).Height(Size.Full());
    }
}
