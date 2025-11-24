using System.ComponentModel.DataAnnotations;
using Ivy.Shared;
using Ivy.Views.Builders;
using Ivy.Views.Forms;

namespace Ivy.Samples.Shared.Apps.Concepts;

public record FormValidationExamples
{
    [Display(Name = "User Name", Description = "Enter your full name", Prompt = "John Doe", Order = 1)]
    [Required(ErrorMessage = "Name is required")]
    [Length(2, 50, ErrorMessage = "Name must be between 2 and 50 characters")]
    public string Name { get; init; } = string.Empty;

    [Display(Name = "Email Address", Description = "Your primary contact email", Order = 2)]
    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; init; } = string.Empty;

    [Display(Name = "Phone", Description = "Mobile or landline", Prompt = "+1-234-567-8900", Order = 3)]
    [Phone]
    [StringLength(20, MinimumLength = 10)]
    public string? PhoneNumber { get; init; }

    [Display(Name = "Age", Description = "Must be 18-120", Order = 4)]
    [Range(18, 120)]
    public int Age { get; init; } = 18;

    [Display(Name = "Website", Order = 5)]
    [Url]
    public string? Website { get; init; }

    [Display(Name = "Bio", Description = "Tell us about yourself", Order = 6)]
    [MinLength(10)]
    [MaxLength(500)]
    public string Bio { get; init; } = string.Empty;

    [Display(Name = "Country", Description = "Select your country", Order = 7)]
    [AllowedValues("USA", "Canada", "UK", "Germany", "France")]
    public string Country { get; init; } = "USA";

    [Display(Name = "Interests", Description = "Pick multiple", Order = 8)]
    [AllowedValues("Technology", "Sports", "Music", "Art", "Travel")]
    [MinLength(1)]
    public string[] Interests { get; init; } = Array.Empty<string>();

    [Display(Name = "Reference Code", Description = "Format: YYYY-MM-DD", Prompt = "2024-01-15", Order = 9)]
    [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "Must match format YYYY-MM-DD")]
    public string? ReferenceCode { get; init; }

    [Display(GroupName = "File Upload", Name = "Profile Picture", Order = 10)]
    [Range(1, 10485760)]
    public long MaxImageSize { get; init; } = 2 * 1024 * 1024;

    [Display(GroupName = "File Upload", Name = "Allowed Types", Order = 11)]
    [AllowedValues("image/png", "image/jpeg", "image/webp")]
    public string AcceptedImageTypes { get; init; } = "image/jpeg";

    [Display(GroupName = "Preferences", Name = "Newsletter", Description = "Receive weekly updates", Order = 12)]
    public bool SubscribeNewsletter { get; init; } = false;

    [Display(GroupName = "Preferences", Name = "Theme", Order = 13)]
    [AllowedValues("Light", "Dark", "Auto")]
    public string Theme { get; init; } = "Auto";

    [Display(Name = "Credit Card", Order = 14)]
    [CreditCard]
    [StringLength(19, MinimumLength = 13)]
    public string? CreditCardNumber { get; init; }

    [Display(Name = "ZIP Code", Order = 15)]
    [RegularExpression(@"^\d{5}(-\d{4})?$")]
    public string? ZipCode { get; init; }

    [Display(Name = "Password", Order = 16)]
    [Required]
    [StringLength(100, MinimumLength = 8)]
    [DataType(DataType.Password)]
    public string Password { get; init; } = string.Empty;

    [Display(Name = "Rating", Description = "Rate from 1-5 stars", Order = 17)]
    [Range(1.0, 5.0)]
    public decimal Rating { get; init; } = 3.0m;

    [Display(Name = "Birthdate", Order = 18)]
    [DataType(DataType.Date)]
    public DateTime? BirthDate { get; init; }

    [Display(Name = "Appointment Time", Order = 19)]
    [DataType(DataType.DateTime)]
    public DateTime? AppointmentDateTime { get; init; }

    [Display(Name = "Comments", Description = "Optional feedback", Order = 20)]
    [DataType(DataType.MultilineText)]
    [MaxLength(1000)]
    public string? Comments { get; init; }
}

[App(icon: Icons.Clipboard, searchHints: ["validation", "attributes", "forms", "dataannotations", "display", "required"])]
public class FormValidationApp : SampleBase
{
    protected override object? BuildSample()
    {
        var model = UseState(() => new FormValidationExamples());
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

        return Layout.Horizontal()
            | new Card(form)
                .Width(1 / 2f)
                .Title("Enhanced Form Validation")
            | new Card(
                Layout.Vertical()
                    | Text.H4("Current Form Data")
                    | model.ToDetails()
            ).Width(1 / 2f)
                .Title("Form State");
    }
}