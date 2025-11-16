using Ivy.Shared;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ivy.Samples.Shared.Apps.Demos;

[App(icon: Icons.TextSelect, title: "Field Input validation test", path: ["Widgets", "Inputs"], searchHints: ["label", "wrapper", "form-field", "input", "description", "help", "validation"])]
public class FieldValidationTestApp : SampleBase
{
    protected override object? BuildSample()
    {
        var emailInput = UseState("");
        var passwordInput = UseState("");
        return Layout.Vertical()
            | emailInput.ToEmailInput()
                .WithField()
                .Label("Email")
                .Description("Your email address")
                .Required()
            | passwordInput.ToPasswordInput()
                .WithField()
                .Label("Password")
                .Description("Your password")
                .Required();
    }
}