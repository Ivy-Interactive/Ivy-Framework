using System;
using System.Linq;
using System.Threading.Tasks;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy.Samples.Shared.Apps.Widgets.Inputs;

[App(icon: Icons.EllipsisVertical, path: ["Widgets", "Inputs"], searchHints: ["ellipsis", "overflow", "select", "async select", "placeholder"])]
public class EllipsisInputOverflowApp : SampleBase
{
    protected override object? BuildSample()
    {
        // SelectInput example based on Select.md - programming languages with long placeholder
        var langs = new string[] { "C#JavaScriptJavaScriptJavaScript", "Java", "Go", "JavaScript", "F#", "Kotlin", "VB.NET", "Rust", "Python", "TypeScript", "Swift", "Dart" };
        var favLang = UseState("C#");

        var selectInput = favLang
            .ToSelectInput(langs.ToOptions())
            .Variant(SelectInputs.Select)
            .Placeholder("Please select your favourite programming language from this extensive list of options")
            .Width(Size.Units(40));

        // AsyncSelectInput example based on AsyncSelect.md - categories with long placeholder
        var selectedCategory = UseState<string?>(default(string?));
        var categories = new[] { "Electronics", "Clothing", "Books", "Home & Garden", "Sports", "Automotive", "Health & Beauty", "Toys & Games" };

        Task<Option<string>[]> QueryCategories(string query)
        {
            return System.Threading.Tasks.Task.FromResult(categories
                .Where(c => c.Contains(query, StringComparison.OrdinalIgnoreCase))
                .Select(c => new Option<string>(c))
                .ToArray());
        }

        Task<Option<string>?> LookupCategory(string? category)
        {
            return System.Threading.Tasks.Task.FromResult(category != null ? new Option<string>(category) : null);
        }

        // Text block with long text value to test ellipsis
        var textBlock = Text.Block("This is a very long text block that should be ellipsed when the width is constrained to a small size")
            .Width(Size.Units(40));

        return Layout.Vertical()
               | Text.H2("Input Ellipsis Overflow")
               | Text.P("All inputs below are constrained to 40 units width. Long placeholders, option labels, and text values should be clipped with ellipsis.")
               | Text.H3("Text Block")
               | textBlock
               | Text.H3("SelectInput")
               | selectInput
               | Text.H3("AsyncSelectInput");
    }
}
