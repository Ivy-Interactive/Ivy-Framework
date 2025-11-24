namespace Ivy.Samples.Shared.Apps.Concepts.Models;

public class FormatsExample
{
    [EmailAddress(ErrorMessage = "Custom: Invalid email address")]
    public string? Field1 { get; set; }

    [CreditCard(ErrorMessage = "Custom: Invalid credit card number")]
    public string? Field2 { get; set; }

    [Url(ErrorMessage = "Custom: Invalid URL")]
    public string? Field3 { get; set; }

    [Phone(ErrorMessage = "Custom: Invalid phone number")]
    public string? Field4 { get; set; }

    [RegularExpression(@"\d{4,5}", ErrorMessage = "Custom: Invalid format")]
    public string? Field5 { get; set; }
}