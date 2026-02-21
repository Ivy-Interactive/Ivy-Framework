using Ivy.Hooks;
using Ivy.Shared;
using Ivy.Views.Forms;

namespace Ivy.Samples.Shared.Apps.Tests;

[App(icon: Icons.ShieldCheck, path: ["Tests"], searchHints: ["validation", "email", "password", "tel", "url", "form", "field", "textinput"])]
public class ValidationTestApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Vertical()
            | Text.H1("Validation test")
            | Text.P("Text input variants, field-wrapped inputs, and form fields.")
            | Layout.Tabs(
                new Tab("Text input variants", new TextInputVariantsTab()),
                new Tab("Field variants", new FieldVariantsTab()),
                new Tab("Form fields", new FormFieldsTab())
            ).Variant(TabsVariant.Content);
    }
}

/// <summary>TextInput variants (Email, Tel, Url, Password) without Field wrapper.</summary>
public class TextInputVariantsTab : ViewBase
{
    public override object? Build()
    {
        var email = UseState("");
        var tel = UseState("");
        var url = UseState("");
        var password = UseState("");

        return new Card(
            Layout.Vertical().Gap(4)
                | Text.H3("TextInput variants (no Field)")
                | Text.P("Email, Tel, Url, and Password variants.")
                | email.ToEmailInput("e.g. user@example.com")
                | tel.ToTelInput("e.g. +1 234 567 8900")
                | url.ToUrlInput("e.g. https://example.com")
                | password.ToPasswordInput("Min 8 characters")
        ).Width(Size.Full());
    }
}

/// <summary>Validated field inputs (ToEmailField, ToTelField, etc.) with label, description, and validation on blur.</summary>
public class FieldVariantsTab : ViewBase
{
    public override object? Build()
    {
        var email = UseState("");
        var tel = UseState("");
        var url = UseState("");
        var password = UseState("");

        return new Card(
            Layout.Vertical().Gap(6)
                | Text.H3("Validated fields")
                | Text.P("Each uses To*Field() for a Field with label, description, required, and validation on blur.")
                | email.ToEmailField("user@example.com").Label("Email").Description("We use this for account recovery.").Required()
                | tel.ToTelField("+1 234 567 8900").Label("Phone").Description("7–15 digits.").Required()
                | url.ToUrlField("https://example.com").Label("Website").Description("Must start with http or https.")
                | password.ToPasswordField("At least 8 characters").Label("Password").Description("Required for sign-up.").Required()
        ).Width(Size.Full());
    }
}

public record ValidationFormModel(string Email, string Password, string? PhoneNumber, string? Website);

/// <summary>Form with email, password, tel, url — validation on blur and submit via Form.</summary>
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
