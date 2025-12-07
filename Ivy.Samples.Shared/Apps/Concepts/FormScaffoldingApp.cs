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
        var formatsExample = UseState(() => new FormatsExample());
        var formatsForm = formatsExample.ToForm();
        var formatsGrid = Layout.Vertical()
                          | new Expandable("Code", new CodeView(typeof(FormatsExample)))
                          | (Layout.Grid().Columns(3).Gap(10)
                              | formatsForm
                              | formatsExample.ToDetails().Builder(NullBuilder))
            ;

        var displayExample = UseState(() => new DisplayExample());
        var displayForm = displayExample.ToForm();
        var displayGrid = Layout.Vertical()
                          | new Expandable("Code", new CodeView(typeof(DisplayExample)))
                          | (Layout.Grid().Columns(3).Gap(10)
                              | displayForm
                              | displayExample.ToDetails().Builder(NullBuilder))

            ;

        var stringsExample = UseState(() => new StringsExample());
        var stringsForm = stringsExample.ToForm();
        var stringsGrid = Layout.Vertical()
                          | new Expandable("Code", new CodeView(typeof(StringsExample)))
                          | (Layout.Grid().Columns(3).Gap(10)
                            | stringsForm
                            | stringsExample.ToDetails().Builder(NullBuilder))
            ;

        var numbersExample = UseState(() => new NumbersExample());
        var numbersForm = numbersExample.ToForm();
        var numbersGrid = Layout.Vertical()
                          | new Expandable("Code", new CodeView(typeof(NumbersExample)))
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
               | Text.H2("Formats")
               | formatsGrid
               | Text.H2("Numbers")
               | numbersGrid
            ;

        object? NullBuilder(object? e)
        {
            if (e == null) return Text.Muted("(null)");
            if (e is string s && string.IsNullOrEmpty(s)) return Text.Muted("(empty string)");
            return e;
        }
    }
}