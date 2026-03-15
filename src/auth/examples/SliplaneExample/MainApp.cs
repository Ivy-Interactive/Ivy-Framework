using Ivy;

namespace SliplaneExample;

[App(id: "auth-test", title: "Auth Test")]
public class MainApp : ViewBase
{
    public override object? Build()
    {
        var auth = UseService<IAuthService>();
        var userInfo = UseState<UserInfo?>();

        UseEffect(async () =>
        {
            var info = await auth.GetUserInfoAsync();
            userInfo.Set(info);
        });

        if (userInfo.Value is null)
        {
            return Text.P("Loading user data...");
        }

        var user = userInfo.Value;

        return Layout.Vertical(
            Text.H2("Authentication Successful!").Color(Colors.Success),
            Layout.Horizontal(
                new Image(user.AvatarUrl ?? "").Size(Size.Units(64)),
                Layout.Vertical(
                    Text.H3(user.FullName ?? "User"),
                    Text.Muted(user.Email)
                ).Gap(4).Align(Align.Center)
            ).Gap(20).Align(Align.Center)
        ).Gap(40).Padding(50).Align(Align.Center).Height(Size.Full());
    }
}
