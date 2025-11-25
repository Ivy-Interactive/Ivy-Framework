namespace Ivy.Samples.Shared.Apps.Concepts.Models;

public class NumbersExample
{
    [Range(0, 100, ErrorMessage = "Custom error: Value must be between 0 and 100")]
    public int Range { get; set; }
}