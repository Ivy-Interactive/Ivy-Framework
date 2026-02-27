using Ivy;
using Ivy.Auth.Examples.Shared;

namespace SupabaseExample;

[App(id: "auth-test", title: "Auth Test")]
public class MainApp : ViewBase
{
    public override object? Build()
    {
        var auth = UseService<IAuthProviderService>();
        var userInfo = UseState<UserInfo?>();
        var oauthTokens = UseState<Dictionary<OAuthProvider, OAuthProviderToken>?>();

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

                        // Automatically show the appropriate test view for each provider
                        Layout.Vertical(oauthTokens.Value.Values.Select(token => new OAuthProviderTestView(token)).ToArray())
                    ).Gap(10)

        ).Gap(40).Padding(50).Align(Align.Center).Height(Size.Full());
    }
}
