using Ivy;
using Microsoft.AspNetCore.Mvc;

namespace BasicAuthExample;

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
            // Success Header
            Text.H2("Authentication Successful!").Color(Colors.Success),

            // Profile info
            Layout.Horizontal(
                 new Image(user.AvatarUrl ?? "").Size(Size.Units(64)),
                 Layout.Vertical(
                     Text.H3(user.FullName ?? "User"),
                     Text.Muted(user.Email)
                 ).Gap(4).AlignContent(Align.Center)
            ).Gap(20).AlignContent(Align.Center)

        ).Gap(40).Padding(50).AlignContent(Align.Center).Height(Size.Full());
    }
}
