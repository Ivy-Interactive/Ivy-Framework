using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ivy.Samples.Shared.Apps.Widgets.Inputs;

[App(icon: Icons.TextSelect, title: "Field Input ", group: ["Widgets", "Inputs"], searchHints: ["label", "wrapper", "form-field", "input", "description", "help"])]
public class FieldApp : SampleBase
{
    protected override object? BuildSample()
    {
        var nameState = UseState<string>();
        var emailState = UseState<string>();
        var passwordState = UseState<string>();
        var acceptedTerms = UseState(false);
        var addressState = UseState<string>();
        var disabledState = UseState("Disabled value");
        var invalidState = UseState("abc");
        var eventsState = UseState<string>();
        var eventsOnBlurLabel = UseState("");
        var eventsOnFocusLabel = UseState("");
        var options = new List<string>() { "I read the terms and conditions and I agree" };

        return Layout.Vertical().Center()
            | Text.H1("Field")
            | (new Card(
                Layout.Vertical().Gap(6).Padding(2)
                | Text.H2("Field")

                // Explicit Field
                | new Field(
                    nameState.ToTextInput()
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
                | disabledState.ToTextInput()
                    .Disabled()
                    .WithField()
                    .Label("Disabled Field")
                    .Description("This field is disabled")

                // Invalid example
                | invalidState.ToTextInput()
                    .Invalid("Must be numeric")
                    .WithField()
                    .Label("Invalid Example")

                | new Spacer().Height(Size.Units(6))
                | Text.H3("Horizontal Labels")

                // Horizontal label using .LabelPosition
                | emailState.ToTextInput()
                    .Variant(TextInputVariant.Email)
                    .WithField()
                    .Label("Work Email")
                    .LabelPosition(LabelPosition.Left)
                    .Description("We'll send updates here")
                    .Required()

                // Horizontal password field
                | passwordState.ToPasswordInput()
                    .WithField()
                    .Label("App Password")
                    .LabelPosition(LabelPosition.Left)
                    | Text.H3("OnFocus/OnBlur")
                    | eventsState.ToTextInput()
                        .Placeholder("Click in, then click away")
                        .OnFocus(_ => eventsOnFocusLabel.Set("Focus Event Triggered"))
                        .OnBlur(_ => eventsOnBlurLabel.Set("Blur Event Triggered"))
                        .WithField()
                        .Label("Focusable field")
                        .Description("Use mouse click or tab to trigger events")
                    | (eventsOnFocusLabel.Value != ""
                        ? Callout.Success(eventsOnFocusLabel.Value)
                        : Callout.Info("Focus the field to see OnFocus"))
                    | (eventsOnBlurLabel.Value != ""
                        ? Callout.Success(eventsOnBlurLabel.Value)
                        : Callout.Info("Blur the field to see OnBlur"))
            )
            .Width(Size.Units(120).Max(500))
        );
    }
}
