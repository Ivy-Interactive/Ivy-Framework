using Ivy.Shared;
using Ivy.Views.Builders;
using Ivy.Views.Forms;

namespace Ivy.Samples.Shared.Apps.Concepts.Forms.Variants;

/// <summary>
/// Demonstrates form validation using data annotations.
/// </summary>
public class FormValidationExample : ViewBase
{
    public override object? Build()
    {
        var model = UseState(() => new FormValidationModel());
        var client = UseService<IClientProvider>();

        UseEffect(() =>
        {
            if (!string.IsNullOrEmpty(model.Value.Name))
            {
                client.Toast($"Form submitted successfully for {model.Value.Name}!");
            }
        }, model);

        var countryOptions = new[] { "USA", "Canada", "UK", "Germany", "France" }.ToOptions();
        var interestOptions = new[] { "Technology", "Sports", "Music", "Art", "Travel" }.ToOptions();
        var themeOptions = new[] { "Light", "Dark", "Auto" }.ToOptions();
        var imageTypeOptions = new[] { "image/png", "image/jpeg", "image/webp" }.ToOptions();

        var form = model.ToForm("Submit Registration")
            // Custom builders for specific field types
            .Builder(m => m.Bio, s => s.ToTextAreaInput())
            .Builder(m => m.Country, s => s.ToSelectInput(countryOptions))
            .Builder(m => m.Interests, s => s.ToSelectInput(interestOptions).List())
            .Builder(m => m.Theme, s => s.ToSelectInput(themeOptions))
            .Builder(m => m.AcceptedImageTypes, s => s.ToSelectInput(imageTypeOptions))
            .Builder(m => m.Comments, s => s.ToTextAreaInput())
            .Builder(m => m.BirthDate, s => s.ToDateTimeInput())
            .Builder(m => m.AppointmentDateTime, s => s.ToDateTimeInput())
            .Builder(m => m.Password, s => s.ToPasswordInput())
            .Builder(m => m.Website, s => s.ToUrlInput())
            .Builder(m => m.PhoneNumber, s => s.ToTelInput())
            // Custom validation
            .Validate<DateTime?>(m => m.BirthDate, birthDate =>
                (birthDate == null || birthDate <= DateTime.Now, "Birth date cannot be in the future"))
            .Validate<string>(m => m.Bio, bio =>
                (string.IsNullOrEmpty(bio) || !bio.Contains("spam"), "Bio cannot contain spam content"));

        return Layout.Vertical()
            | (Layout.Horizontal()
                | new Card(form)
                    .Width(1 / 2f)
                    .Title("Enhanced Form Validation")
                | new Card(
                    Layout.Vertical()
                        | Text.H4("Current Form Data")
                        | model.ToDetails()
                ).Width(1 / 2f)
                    .Title("Form State"));
    }
}
