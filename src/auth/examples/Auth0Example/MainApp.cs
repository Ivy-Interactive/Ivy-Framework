using Ivy;
using Ivy.Auth.Examples.Shared;

namespace Auth0Example;

[App(id: "auth-test", title: "Auth Test")]
public class MainApp : ViewBase
{
    public override object? Build()
    {
        var auth = UseService<IAuthService>();
        var userInfo = UseState<UserInfo?>();
        var brokeredSessions = UseState<Dictionary<string, IAuthTokenHandlerSession>?>();

        UseEffect(async () =>
        {
            var info = await auth.GetUserInfoAsync();
            userInfo.Set(info);

            // Get brokered auth sessions
            var result = await auth.GetBrokeredSessionsAsync();
            brokeredSessions.Set(result.Sessions);
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
                 new Image(user.AvatarUrl ?? "").Size(Size.Units(64)),
                 Layout.Vertical(
                     Text.H3(user.FullName ?? "User"),
                     Text.Muted(user.Email)
                 ).Gap(4).AlignContent(Align.Center)
            ).Gap(20).AlignContent(Align.Center),

            // Brokered Auth Sessions Section
            Text.H3("Brokered Auth Sessions"),
            brokeredSessions.Value == null
                ? Text.P("Brokered auth sessions not available")
                : brokeredSessions.Value.Count == 0
                    ? Text.P("No brokered auth sessions connected")
                    : Layout.Vertical(
                        Text.P($"Connected providers: {string.Join(", ", brokeredSessions.Value.Keys)}"),

                        // Automatically show the appropriate test view for each provider
                        Layout.Vertical(brokeredSessions.Value.Select(kvp => new OAuthProviderTestView(kvp.Key, kvp.Value)).ToArray())
                    ).Gap(10)

        ).Gap(40).Padding(50).AlignContent(Align.Center).Height(Size.Full());
    }
}
