using Ivy;
using Ivy.Auth.Examples.Shared;

namespace GitHubExample;

[App(id: "auth-test", title: "Auth Test")]
public class MainApp : ViewBase
{
    public override object? Build()
    {
        var auth = UseService<IAuthService>();
        var userInfo = UseState<UserInfo?>();
        var oauthSessions = UseState<Dictionary<string, IAuthTokenHandlerSession>?>();

        UseEffect(async () =>
        {
            var info = await auth.GetUserInfoAsync();
            userInfo.Set(info);

            // Get OAuth provider sessions
            var result = await auth.GetOAuthSessionsAsync();
            oauthSessions.Set(result.Sessions);
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

            // OAuth Provider Sessions Section
            Text.H3("OAuth Provider Sessions"),
            oauthSessions.Value == null
                ? Text.P("OAuth sessions not available")
                : oauthSessions.Value.Count == 0
                    ? Text.P("No OAuth providers connected")
                    : Layout.Vertical(
                        Text.P($"Connected providers: {string.Join(", ", oauthSessions.Value.Keys)}"),

                        // Automatically show the appropriate test view for each provider
                        Layout.Vertical(oauthSessions.Value.Select(kvp => new OAuthProviderTestView(kvp.Key, kvp.Value)).ToArray())
                    ).Gap(10)

        ).Gap(40).Padding(50).Align(Align.Center).Height(Size.Full());
    }
}
