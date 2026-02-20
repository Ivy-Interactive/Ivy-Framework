using Ivy;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace Auth0Example;

[App(id: "auth-test", title: "Auth Test")]
public class MainApp : ViewBase
{
    public override object? Build()
    {
        var auth = UseService<IAuthService>();
        var userInfo = UseState<UserInfo?>();
        var oauthTokens = UseState<Dictionary<string, OAuthProviderToken>?>();
        var apiResponse = UseState<string?>();

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
                        oauthTokens.Value.ContainsKey("google-oauth2")
                            ? Layout.Vertical(
                                new Button("Test Google API Access", async () =>
                                {
                                    var googleToken = oauthTokens.Value["google-oauth2"];
                                    using var httpClient = new HttpClient();
                                    httpClient.DefaultRequestHeaders.Authorization =
                                        new AuthenticationHeaderValue("Bearer", googleToken.AccessToken);

                                    try
                                    {
                                        var response = await httpClient.GetStringAsync(
                                            "https://www.googleapis.com/oauth2/v2/userinfo");
                                        apiResponse.Set(response);
                                    }
                                    catch (Exception ex)
                                    {
                                        apiResponse.Set($"Error: {ex.Message}");
                                    }
                                }, variant: ButtonVariant.Primary),
                                apiResponse.Value != null
                                    ? Text.Json(apiResponse.Value)
                                    : null
                            ).Gap(10)
                            : null,

                        // Example: Test GitHub API access if available
                        oauthTokens.Value.ContainsKey("github")
                            ? Layout.Vertical(
                                new Button("Test GitHub API Access", async () =>
                                {
                                    var githubToken = oauthTokens.Value["github"];
                                    using var httpClient = new HttpClient();
                                    httpClient.DefaultRequestHeaders.Authorization =
                                        new AuthenticationHeaderValue("Bearer", githubToken.AccessToken);
                                    httpClient.DefaultRequestHeaders.UserAgent.Add(
                                        new ProductInfoHeaderValue("Auth0Example", "1.0"));

                                    try
                                    {
                                        var response = await httpClient.GetStringAsync("https://api.github.com/user");
                                        apiResponse.Set(response);
                                    }
                                    catch (Exception ex)
                                    {
                                        apiResponse.Set($"Error: {ex.Message}");
                                    }
                                }, variant: ButtonVariant.Primary),
                                apiResponse.Value != null
                                    ? Text.Json(apiResponse.Value)
                                    : null
                            ).Gap(10)
                            : null
                    ).Gap(10)

        ).Gap(40).Padding(50).Align(Align.Center).Height(Size.Full());
    }
}
