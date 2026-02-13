using Ivy.Hooks;
using Ivy.Shared;
using Ivy.Validation;
using Ivy.Views.Forms;

namespace Ivy.Samples.Shared.Apps.Tests;

[App(icon: Icons.ShieldCheck, path: ["Tests"], searchHints: ["validation", "email", "password", "tel", "url", "form", "field", "textinput"])]
public class ValidationTestApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Vertical()
            | Text.H1("Validation test")
            | Text.P("Use the tabs below to try validation for text input variants, field-wrapped inputs, and form fields. Blur or submit to see invalid state.")
            | Layout.Tabs(
                new Tab("Text input variants", new TextInputVariantsTab()),
                new Tab("Field variants", new FieldVariantsTab()),
                new Tab("Form fields", new FormFieldsTab())
            ).Variant(TabsVariant.Content);
    }
}

/// <summary>Plain TextInput variants (Email, Tel, Url, Password) with validation on blur — no Field wrapper.</summary>
public class TextInputVariantsTab : ViewBase
{
    public override object? Build()
    {
        var email = UseState("");
        var tel = UseState("");
        var url = UseState("");
        var password = UseState("");
        var emailInvalid = UseState("");
        var telInvalid = UseState("");
        var urlInvalid = UseState("");
        var passwordInvalid = UseState("");

        return new Card(
            Layout.Vertical().Gap(4)
                | Text.H3("Validated TextInput variants (no Field)")
                | Text.P("Blur each field to run validation. Invalid values show an error message.")
                | new TextInput(email)
                    .Placeholder("e.g. user@example.com")
                    .Variant(TextInputs.Email)
                    .Invalid(emailInvalid.Value)
                    .HandleBlur(_ => { var (ok, msg) = Validators.ValidateValue(email.Value, TextInputs.Email, "Email"); emailInvalid.Set(ok ? "" : msg ?? ""); })
                | new TextInput(tel)
                    .Placeholder("e.g. +1 234 567 8900")
                    .Variant(TextInputs.Tel)
                    .Invalid(telInvalid.Value)
                    .HandleBlur(_ => { var (ok, msg) = Validators.ValidateValue(tel.Value, TextInputs.Tel, "Phone"); telInvalid.Set(ok ? "" : msg ?? ""); })
                | new TextInput(url)
                    .Placeholder("e.g. https://example.com")
                    .Variant(TextInputs.Url)
                    .Invalid(urlInvalid.Value)
                    .HandleBlur(_ => { var (ok, msg) = Validators.ValidateValue(url.Value, TextInputs.Url, "Website"); urlInvalid.Set(ok ? "" : msg ?? ""); })
                | new TextInput(password)
                    .Placeholder("Min 8 characters")
                    .Variant(TextInputs.Password)
                    .Invalid(passwordInvalid.Value)
                    .HandleBlur(_ => { var (ok, msg) = Validators.ValidateValue(password.Value, TextInputs.Password, "Password"); passwordInvalid.Set(ok ? "" : msg ?? ""); })
        ).Width(Size.Full());
    }
}

/// <summary>Field-wrapped validated inputs with label, description, required — validation on blur via Validators.ValidateValue.</summary>
public class FieldVariantsTab : ViewBase
{
    public override object? Build()
    {
        var email = UseState("");
        var tel = UseState("");
        var url = UseState("");
        var password = UseState("");
        var emailInvalid = UseState("");
        var telInvalid = UseState("");
        var urlInvalid = UseState("");
        var passwordInvalid = UseState("");

        return new Card(
            Layout.Vertical().Gap(6)
                | Text.H3("Field-wrapped validated inputs")
                | Text.P("Each input is a Field with label, description, and required. Validation on blur.")
                | email.ToEmailInput("user@example.com")
                    .WithField().Label("Email").Description("We use this for account recovery.").Required()
                | tel.ToTelInput("+1 234 567 8900")
                    .WithField().Label("Phone").Description("7–15 digits.").Required()
                | url.ToUrlInput("https://example.com")
                    .WithField().Label("Website").Description("Must start with http or https.")
                | password.ToPasswordInput("At least 8 characters")
                    .WithField().Label("Password").Description("Required for sign-up.").Required()
        ).Width(Size.Full());
    }
}

public record ValidationFormModel(string Email, string Password, string? PhoneNumber, string? Website);

/// <summary>Form with validated email, password, tel, url — validation on blur and submit via FormFieldView.</summary>
public class FormFieldsTab : ViewBase
{
    public override object? Build()
    {
        var model = UseState(() => new ValidationFormModel("", "", null, null));
        var client = UseService<IClientProvider>();

        UseEffect(() =>
        {
            if (!string.IsNullOrEmpty(model.Value.Email) && model.Value.Email.Contains('@'))
            {
                client.Toast("Form submitted successfully!");
            }
        }, model);

        var form = model.ToForm("Submit")
            .Builder(m => m.Email, s => s.ToEmailInput())
            .Builder(m => m.Password, s => s.ToPasswordInput())
            .Builder(m => m.PhoneNumber, s => s.ToTelInput())
            .Builder(m => m.Website, s => s.ToUrlInput());

        return new Card(
            Layout.Vertical().Gap(4)
                | Text.H3("Form with validated fields")
                | Text.P("Email, password, phone, and URL are validated on blur and before submit.")
                | form
        ).Width(Size.Full());
    }
}
