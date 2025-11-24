using System.Collections.Immutable;
using Ivy.Samples.Shared.Apps.Concepts.Models;
using Ivy.Shared;
using Ivy.Views.Builders;
using Ivy.Views.Forms;

namespace Ivy.Samples.Shared.Apps.Concepts;

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

        var numbersExample = UseState(() => new NumbersExample());
        var numbersForm = numbersExample.ToForm();
        var numbersGrid = Layout.Vertical()
                          | new CodeView(typeof(NumbersExample))
                          | (Layout.Grid().Columns(3).Gap(10)
                            | numbersForm
                            | numbersExample.ToDetails())
            ;

        return Layout.Vertical()
               | Text.H1("Form Scaffolding")
               | Text.H2("Display")
               | displayGrid
               | Text.H2("Strings")
               | stringsGrid
               | Text.H2("Numbers")
               | numbersGrid
            ;
    }
}