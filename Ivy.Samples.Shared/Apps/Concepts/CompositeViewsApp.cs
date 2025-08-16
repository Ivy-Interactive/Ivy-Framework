using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Concepts;

public record struct LoginData(string Username = "", string Password = "", bool RememberMe = false, bool DidLogin = false);

public class LoginForm(IState<LoginData> login) : SampleBase
{
    protected override object? BuildSample()
    {
        var username = this.UseState(login.Value.Username);
        var password = this.UseState(login.Value.Password);
        var rememberMe = this.UseState(login.Value.RememberMe);

        return Layout.Vertical(
            username.ToTextInput(),
            password.ToTextInput().Variant(TextInputs.Password),
            new BoolInput<bool>(rememberMe).Label("Remember me"),
            new Button("Login", _ =>
            {
                login.Set(login.Value with { Username = username.Value, Password = password.Value, DidLogin = true, RememberMe = rememberMe.Value });
            })
        );
    }
}

[App(icon: Icons.Blocks)]
public class CompositeViewsApp : ViewBase
{
    public override object? Build()
    {
        var loginData = this.UseState(() => new LoginData());

        return new Card(
            Layout.Vertical(
                new LoginForm(loginData),
                (loginData.Value.DidLogin ? Text.Literal($"Logged in as {loginData.Value.Username}") : "")
            )
        ).Title("Composite Views Demo");
    }
}