namespace Ivy.Samples.Shared.Apps.Concepts.Models;

public class StringsExample
{
    [ScaffoldColumn(false)]
    public string IgnoredString1 { get; set; } = "";

    public string NormalString { get; set; } = "Hello";

    public string? NullableString { get; set; } = null;

    [Required]
    public string RequiredString1 { get; set; }

    [Required]
    public string? RequiredString2 { get; set; }

    [MinLength(5, ErrorMessage = "Custom error: Minimum length is 5")]
    [Display(Description = "Must be at least 5 characters long")]
    public string MinLengthString { get; set; } = "";

    [MaxLength(5, ErrorMessage = "Custom error: Maximum length is 5")]
    [Display(Description = "Must be at most 5 characters long")]
    public string MaxLengthString { get; set; } = "";

    [StringLength(10, MinimumLength = 5, ErrorMessage = "Custom error: Must be between 5 and 10 characters long")]
    [Display(Description = "Must be between 5 and 10 characters long")]
    public string StringLengthString { get; set; } = "";

    [Length(5, 10, ErrorMessage = "Custom error: Must be between 5 and 10 characters long")]
    [Display(Description = "Must be between 5 and 10 characters long")]
    public string LengthString { get; set; } = "";
}
