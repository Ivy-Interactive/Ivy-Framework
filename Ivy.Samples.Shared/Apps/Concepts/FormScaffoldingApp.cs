using System.Collections.Immutable;
using Ivy.Shared;
using Ivy.Views.Builders;
using Ivy.Views.Forms;

namespace Ivy.Samples.Shared.Apps.Concepts;

/*
[Required] - Field must have a value
[MinLength(n)] - Minimum string length
[MaxLength(n)] - Maximum string length
[StringLength(max, MinimumLength = min)] - String length constraints
[Length(min, max)] - Length constraints for strings and collections
[Range(min, max)] - Value must be within range
[EmailAddress] - Valid email format
[Phone] - Valid phone number format
[Url] - Valid URL format
[CreditCard] - Valid credit card number format
[RegularExpression(pattern)] - Match a regex pattern
[AllowedValues(...)] - Value must be from specified list
[DataType(...)] - Specifies data type (Password, Date, DateTime, MultilineText, etc.)
[Display(...)] - Controls field display properties (Name, Description, Order, GroupName, Prompt)
 */

public class DisplayExample
{
    [Display(Name = "Custom Name", Description = "This is a custom description.", Order = 2, GroupName = "Group A", Prompt = "Enter value here")]
    public string CustomDisplayString { get; set; } = "Display Example";

    [Display(Name = "Another Name", Order = 1, GroupName = "Group A")]
    public string AnotherDisplayString { get; set; } = "Another Display Example";

    [Display(Name = "Different Group", GroupName = "Group B")]
    public string DifferentGroupString { get; set; } = "Different Group Example";
}

public class StringsExample
{
    [ScaffoldColumn(false)]
    public string IgnoredString1 { get; set; } = "This string will be ignored.";

    //[Ignore]
    //public string IgnoredString2 { get; set; } = "This string will also be ignored.";

    public string NormalString { get; set; } = "This is a normal string.";

    public string? NullableString { get; set; } = null;

    [Required]
    public string RequiredString1 { get; set; }

    [Required]
    public string RequiredString2 { get; set; }
}

[App(icon: Icons.Clipboard, searchHints: ["forms", "scaffolding"])]
public class FormScaffoldingApp : SampleBase
{
    protected override object? BuildSample()
    {
        var displayExample = UseState(() => new DisplayExample());
        var displayForm = displayExample.ToForm();
        var displayGrid = Layout.Grid().Columns(3)
                          | displayForm
                          | displayExample.ToDetails();

        var stringsExample = UseState(() => new StringsExample());
        var stringsForm = stringsExample.ToForm();
        var stringsGrid = Layout.Grid().Columns(3)
                          | stringsForm
                          | stringsExample.ToDetails();

        return Layout.Vertical()
               | Text.H1("Form Scaffolding")
               | Text.H2("Display")
               | displayGrid

               | Text.H2("Strings")
               | stringsGrid;

    }
}