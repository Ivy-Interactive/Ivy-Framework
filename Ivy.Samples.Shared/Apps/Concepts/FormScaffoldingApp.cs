using System.Collections.Immutable;
using Ivy.Samples.Shared.Apps.Concepts.Models;
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

[App(icon: Icons.Brain, searchHints: ["forms", "scaffolding"])]
public class FormScaffoldingApp : SampleBase
{
    protected override object? BuildSample()
    {
        var displayExample = UseState(() => new DisplayExample());
        var displayForm = displayExample.ToForm();
        var displayGrid = Layout.Vertical()
                          | new CodeView(typeof(DisplayExample))
                          | (Layout.Grid().Columns(3).Gap(10)
                              | displayForm
                              | displayExample.ToDetails())

            ;

        var stringsExample = UseState(() => new StringsExample());
        var stringsForm = stringsExample.ToForm();
        var stringsGrid = Layout.Vertical()
                          | new CodeView(typeof(StringsExample))
                          | (Layout.Grid().Columns(3).Gap(10)
                            | stringsForm
                            | stringsExample.ToDetails())

            ;

        return Layout.Vertical()
               | Text.H1("Form Scaffolding")
               | Text.H2("Display")
               | displayGrid

               | Text.H2("Strings")
               | stringsGrid;

    }
}