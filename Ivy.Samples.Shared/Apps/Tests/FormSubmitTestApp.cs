using Ivy.Shared;
using Ivy.Views.Forms;

namespace Ivy.Samples.Shared.Apps.Tests;

public record LoginModel(string Username, string Email, string Password);

[App(icon: Icons.LogIn, path: ["Tests"], isVisible: false)]
public class FormSubmitTestApp : SampleBase
{
    protected override object? BuildSample()
    {
        var model = UseState(() => new LoginModel("", "", ""));
        var client = UseService<IClientProvider>();

        UseEffect(() =>
        {
            if (!string.IsNullOrEmpty(model.Value.Username))
            {
                client.Toast("Form submitted successfully!");
            }
        }, model);

        return new Card()
            .Title("Login")
                | model.ToForm()
                  .Validate<string>(m => m.Username, username => (username.Length >= 3, "Username must be at least 3 characters"))
                  .Validate<string>(m => m.Password, password => (password.Length >= 8, "Password must be at least 8 characters"));
    }
}

