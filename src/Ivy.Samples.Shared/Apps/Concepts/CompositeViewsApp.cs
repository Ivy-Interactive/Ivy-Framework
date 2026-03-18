
namespace Ivy.Samples.Shared.Apps.Concepts;

public record struct LoginData(string Username = "", string Password = "", bool RememberMe = false, bool DidLogin = false);

public class LoginForm(IState<LoginData> login) : SampleBase
{
    protected override object? BuildSample()
    {
        var username = UseState(login.Value.Username);
        var password = UseState(login.Value.Password);
        var rememberMe = UseState(login.Value.RememberMe);

        return Layout.Vertical(
            username.ToTextInput(),
            password.ToTextInput().Variant(TextInputVariant.Password),
            rememberMe.ToBoolInput().Label("Remember me"),
            new Button("Login", _ =>
            {
                login.Set(login.Value with { Username = username.Value, Password = password.Value, DidLogin = true, RememberMe = rememberMe.Value });
            })
        );
    }
}

[App(icon: Icons.Blocks, searchHints: ["components", "reusable", "composition", "nested", "modular", "structure"])]
public class CompositeViewsApp : ViewBase
{
    public override object? Build()
    {
        var loginData = UseState(() => new LoginData());

        return new Card(
            Layout.Vertical(
                new LoginForm(loginData),
                (loginData.Value.DidLogin ? Text.Literal($"Logged in as {loginData.Value.Username}") : "")
            )
        ).Title("Composite Views Demo");
    }
}
