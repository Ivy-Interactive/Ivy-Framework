namespace Ivy.Samples.Shared.Apps.Concepts.Models;

public class StringsExample
{
    [ScaffoldColumn(false)]
    public string IgnoredString1 { get; set; } = "";

    public string NormalString { get; set; } = "Hello";

    public string? NullableString { get; set; } = null;

    [Required]
    public string RequiredString1 { get; set; }
}