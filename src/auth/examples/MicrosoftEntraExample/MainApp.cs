using Ivy;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace MicrosoftEntraExample;

[App(id: "auth-test", title: "Auth Test")]
public class MainApp : ViewBase
{
    public override object? Build()
    {
        var auth = UseService<IAuthService>();
        var userInfo = UseState<UserInfo?>();
        var oauthTokens = UseState<Dictionary<OAuthProvider, OAuthProviderToken>?>();
        var graphProfile = UseState<string?>();

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

                        // Example: Test Microsoft Graph API access
                        oauthTokens.Value.ContainsKey(OAuthProvider.Microsoft)
                            ? Layout.Vertical(
                                Text.H4("Microsoft Graph API Test"),
                                Layout.Horizontal(
                                    new Button("Get Profile", async () =>
                                    {
                                        var microsoftToken = oauthTokens.Value[OAuthProvider.Microsoft];
                                        using var httpClient = new HttpClient();
                                        httpClient.DefaultRequestHeaders.Authorization =
                                            new AuthenticationHeaderValue("Bearer", microsoftToken.AccessToken);

                                        try
                                        {
                                            var response = await httpClient.GetStringAsync(
                                                "https://graph.microsoft.com/v1.0/me");
                                            graphProfile.Set(response);
                                        }
                                        catch (Exception ex)
                                        {
                                            graphProfile.Set($"{{\"error\": \"{ex.Message}\"}}");
                                        }
                                    }, variant: ButtonVariant.Primary),
                                    new Button("List OneDrive Files", async () =>
                                    {
                                        var microsoftToken = oauthTokens.Value[OAuthProvider.Microsoft];
                                        using var httpClient = new HttpClient();
                                        httpClient.DefaultRequestHeaders.Authorization =
                                            new AuthenticationHeaderValue("Bearer", microsoftToken.AccessToken);

                                        try
                                        {
                                            var response = await httpClient.GetStringAsync(
                                                "https://graph.microsoft.com/v1.0/me/drive/root/children");
                                            graphProfile.Set(response);
                                        }
                                        catch (Exception ex)
                                        {
                                            graphProfile.Set($"{{\"error\": \"{ex.Message}\"}}");
                                        }
                                    }, variant: ButtonVariant.Outline)
                                ).Gap(10),
                                graphProfile.Value != null
                                    ? Text.Json(graphProfile.Value)
                                    : null
                            ).Gap(10)
                            : null
                    ).Gap(10)

        ).Gap(40).Padding(50).Align(Align.Center).Height(Size.Full());
    }
}
