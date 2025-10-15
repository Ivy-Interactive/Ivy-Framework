using Ivy.Shared;
using Ivy.Views.Forms;

namespace Ivy.Samples.Shared.Apps.Tests;

public record LoginModel(string Username, string Password);

[App(icon: Icons.LogIn, path: ["Tests"], isVisible: true)]
public class FormSubmitTestApp : SampleBase
{
    protected override object? BuildSample()
    {
        var model = UseState(() => new LoginModel("", ""));

        return new Card().Title("Login")
            | model.ToForm();
    }
}

