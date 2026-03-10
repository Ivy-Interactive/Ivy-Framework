using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ivy.Samples.Shared.Apps.Widgets.Inputs;

[App(icon: Icons.TextSelect, title: "Field Input ", path: ["Widgets", "Inputs"], searchHints: ["label", "wrapper", "form-field", "input", "description", "help"])]
public class FieldApp : SampleBase
{
    protected override object? BuildSample()
    {
        var nameState = UseState<string>();
        var emailState = UseState<string>();
        var passwordState = UseState<string>();
        var acceptedTerms = UseState(false);
        var addressState = UseState<string>();
        var options = new List<string>() { "I read the terms and conditions and I agree" };

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

                // Using .WithField() shortcut with help text
                | emailState.ToTextInput()
                    .Placeholder("Enter your email")
                    .WithField()
                    .Label("Email")
                    .Description("Required for contact")
                    .Help("We will never share your email with third parties")
                    .Required()

                // Password field, disabled if name is empty, with help text
                | passwordState.ToPasswordInput()
                    .Placeholder("Enter password")
                    .Disabled(string.IsNullOrWhiteSpace(nameState.Value))
                    .WithField()
                    .Label("Password")
                    .Description("At least 8 characters")
                    .Help("Use a mix of letters, numbers, and symbols for better security")

                // Checkbox wrapped with .WithField()
                | acceptedTerms.ToSelectInput(options.ToOptions())
                                .Variant(SelectInputVariant.List)
                    .WithField()
                    .Label("Accept Terms & Conditions")
                    .Description("You must accept to continue")
                    .Required()

                // TextArea input using .WithField()
                | addressState.ToTextareaInput()
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
