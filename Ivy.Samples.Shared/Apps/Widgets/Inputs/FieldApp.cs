using Ivy.Shared;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ivy.Samples.Shared.Apps.Demos;

[App(icon: Icons.TextSelect, title: "Field Input ", path: ["Widgets", "Inputs"], searchHints: ["label", "wrapper", "form-field", "input", "description", "help"])]
public class FieldApp : SampleBase
{
    protected override object? BuildSample()
    {
        var nameState = this.UseState<string>();
        var emailState = this.UseState<string>();
        var passwordState = this.UseState<string>();
        var acceptedTerms = this.UseState(false);
        var options = new List<string>() { "I read the terms and conditions and I agree" };
        var addressState = this.UseState<string>();

        return Layout.Vertical().Center()
            | (new Card(
                Layout.Vertical().Gap(6).Padding(2)
                | Text.H2("Field")

                // Explicit Field
                | new Field(
                    new TextInput(nameState)
                        .Placeholder("Enter your name")
                )
                .Label("Name")
                .Description("Your full name")
                .Required()

                // Using .WithField() shortcut
                | emailState.ToTextInput()
                    .Placeholder("Enter your email")
                    .WithField()
                    .Label("Email")
                    .Description("Required for contact")
                    .Required()

                // Password field, disabled if name is empty
                | passwordState.ToPasswordInput()
                    .Placeholder("Enter password")
                    .Disabled(string.IsNullOrWhiteSpace(nameState.Value))
                    .WithField()
                    .Label("Password")
                    .Description("At least 8 characters")

                // Checkbox wrapped with .WithField()
                | acceptedTerms.ToSelectInput(options.ToOptions())
                                .Variant(SelectInputs.List)
                    .WithField()
                    .Label("Accept Terms & Conditions")
                    .Description("You must accept to continue")
                    .Required()

                // TextArea input using .WithField()
                | addressState.ToTextAreaInput()
                    .Placeholder("Street, City, ZIP")
                    .WithField()
                    .Label("Address")
                    .Description("Your mailing address")

                // Disabled TextInput
                | new TextInput("Disabled value")
                    .Disabled()
                    .WithField()
                    .Label("Disabled Field")
                    .Description("This field is disabled")

                // Invalid example
                | new TextInput("abc")
                    .Invalid("Must be numeric")
                    .WithField()
                    .Label("Invalid Example")
            )
            .Width(Size.Units(120).Max(500))


        );
    }
}