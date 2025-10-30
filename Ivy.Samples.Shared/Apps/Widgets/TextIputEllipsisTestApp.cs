using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets;

/// <summary>
/// Test app specifically for TextInput widgets and their ellipsis behavior.
/// This app demonstrates how TextInput handles long text and whether ellipsis is applied.
/// </summary>
[App(
    order: 1,
    icon: Icons.Type,
    title: "TextInput Ellipsis Test",
    description: "Test app to demonstrate ellipsis behavior in TextInput widgets and identify issues with long text handling"
)]
public class TextInputEllipsisTestApp : SampleBase
{
    protected override object? BuildSample()
    {
        var shortText = this.UseState("Short text");
        var mediumText = this.UseState("This is a medium length text that should fit comfortably in most input fields");
        var longText = this.UseState("This is an extremely long text input that should definitely be truncated with ellipsis and show proper behavior when the text exceeds the available width of the input field");
        var veryLongText = this.UseState("This is an extremely long text input that should definitely be truncated with ellipsis and show proper behavior when the text exceeds the available width of the input field. This text is intentionally very long to test how the TextInput widget handles overflow and whether it applies ellipsis correctly. We want to see if the text gets cut off with ellipsis (...) or if it overflows the container boundaries.");
        var placeholderText = this.UseState("");
        var disabledText = this.UseState("This is disabled text that should also demonstrate ellipsis behavior");
        var invalidText = this.UseState("This is invalid text with error state that should show ellipsis behavior");
        // Additional states for other input examples
        var boolState = this.UseState(false);
        var colorState = this.UseState("#ff0000");
        var languages = new string[]
        {
            "This is an extremely long option label that should definitely be truncated with ellipsis when the select is narrow",
            "C#",
            "JavaScript",
            "Rust",
            "Go"
        };
        var selectedLang = this.UseState(languages[0]);

        return Layout.Vertical()
               | Text.H1("TextInput Ellipsis Test")
               | Text.P("This app tests how TextInput widgets handle long text and whether ellipsis is properly applied.")

               | Text.H2("Basic TextInput Tests")
               | Text.P("Testing different text lengths in TextInput widgets:")

               // Short text test
               | Text.P("Short Text (should fit normally):")
               | shortText.ToTextInput()
                   .Placeholder("Enter short text")
                   .Width(Size.Units(30))

               // Medium text test
               | Text.P("Medium Text (should fit but test behavior):")
               | mediumText.ToTextInput()
                   .Placeholder("Enter medium text")
                   .Width(Size.Units(30))

               // Long text test
               | Text.P("Long Text (should show ellipsis):")
               | longText.ToTextInput()
                   .Placeholder("Enter long text")
                   .Width(Size.Units(30))

               // Very long text test
               | Text.P("Very Long Text (definitely should show ellipsis):")
               | veryLongText.ToTextInput()
                   .Placeholder("Enter very long text")
                   .Width(Size.Units(30))

               | Text.H2("TextInput States with Long Text")
               | Text.P("Testing different states with long text:")

               // Placeholder test
               | Text.P("Empty with Long Placeholder:")
               | placeholderText.ToTextInput()
                   .Placeholder("This is an extremely long placeholder text that should demonstrate how placeholders are handled when they exceed the input field width")
                   .Width(Size.Units(30))

               // Disabled state test
               | Text.P("Disabled with Long Text:")
               | disabledText.ToTextInput()
                   .Placeholder("Enter text")
                   .Width(Size.Units(30))
                   .Disabled()

               // Invalid state test
               | Text.P("Invalid with Long Text:")
               | invalidText.ToTextInput()
                   .Placeholder("Enter text")
                   .Width(Size.Units(30))
                   .Invalid("This is a very long error message that should also demonstrate ellipsis behavior when it exceeds the available space")

               | Text.H2("TextInput Sizes with Long Text")
               | Text.P("Testing different sizes with long text:")

               // Small size
               | Text.P("Small Size:")
               | longText.ToTextInput()
                   .Placeholder("Enter text")
                   .Width(Size.Units(20))
                   .Small()

               // Medium size (default)
               | Text.P("Medium Size (default):")
               | longText.ToTextInput()
                   .Placeholder("Enter text")
                   .Width(Size.Units(30))

               // Large size
               | Text.P("Large Size:")
               | longText.ToTextInput()
                   .Placeholder("Enter text")
                   .Width(Size.Units(40))
                   .Large()

               | Text.H2("TextInput Width Constraints")
               | Text.P("Testing different width constraints:")

               // Very narrow width
               | Text.P("Very Narrow (100px - should definitely show ellipsis):")
               | longText.ToTextInput()
                   .Placeholder("Enter text")
                   .Width(Size.Units(30))

               // Narrow width
               | Text.P("Narrow (150px - should show ellipsis):")
               | longText.ToTextInput()
                   .Placeholder("Enter text")
                   .Width(Size.Units(30))

               // Wide width
               | Text.P("Wide (500px - might not need ellipsis):")
               | longText.ToTextInput()
                   .Placeholder("Enter text")
                   .Width(Size.Units(50))

               | Text.H2("Current Values")
               | Text.P("Current text values (to verify state management):")
               | Text.P($"Short: {shortText.Value}")
               | Text.P($"Medium: {mediumText.Value}")
               | Text.P($"Long: {longText.Value}")
               | Text.P($"Very Long: {veryLongText.Value}")
               | Text.P($"Placeholder: '{placeholderText.Value}'")
               | Text.P($"Disabled: {disabledText.Value}")
               | Text.P($"Invalid: {invalidText.Value}")

               | Text.H2("List Widget Tests with Long Text")
               | Text.P("Testing List with long text in titles and subtitles:")
               | new Card(
                new List(
                   new ListItem(
                       title: "Short Title",
                       subtitle: "Short subtitle"
                   ),
                   new ListItem(
                       title: "This is an extremely long title that should definitely be truncated with ellipsis and show proper behavior when the text exceeds the available width",
                       subtitle: "Short subtitle"
                   ),
                   new ListItem(
                       title: "Medium length title",
                       subtitle: "This is an extremely long subtitle that should definitely be truncated with ellipsis and show proper behavior when the text exceeds the available width of the list item"
                   ),
                   new ListItem(
                       title: "This is an extremely long title that should definitely be truncated with ellipsis and show proper behavior when the text exceeds the available width",
                       subtitle: "This is also an extremely long subtitle that should definitely be truncated with ellipsis and show proper behavior when the text exceeds the available width of the list item"
                   ),
                   new ListItem(
                       title: "List item with very long title text that demonstrates how the List widget handles overflow and whether it applies ellipsis correctly to prevent layout issues",
                       subtitle: "And an equally long subtitle that explains in great detail what this list item represents, including all of its features, capabilities, and use cases that should definitely be truncated"
                   ),
                   new ListItem(
                       title: "Another example with maximum length title to test the absolute limits of text truncation in list items and see how the UI handles extremely long content",
                       subtitle: "With a maximum length subtitle that provides extensive information about the list item including detailed descriptions of its purpose, functionality, and expected behavior in various scenarios"
                   ),
                   new ListItem(
                       title: "Short",
                       subtitle: "Normal",
                       badge: "This is a very long badge text that should also demonstrate ellipsis behavior"
                   ),
                   new ListItem(
                       title: "Testing all fields",
                       subtitle: "With reasonable length",
                       badge: "VeryLongBadgeTextThatShouldBeTruncated"
                   )
               )).Width(Size.Units(60))
               | new Card(
                new Expandable("Click to expandClick to expandClick to expandClick to expandClick to expandClick to expandClick to expandClick to expandClick to expandClick to expandClick to expandClick to expandClick to expandClick to expandClick to expandClick to expandClick to expandClick to expandClick to expandClick to expandClick to expandClick to expandClick to expandClick to expandClick to expand",
    "This is the hidden content that appears when you expand the widget.").Width(Size.Units(30))
               ).Width(Size.Units(60))
               | new Card(
                   Layout.Vertical()
                   | Text.H2("AsyncSelectInput Widget Test")
                   | Text.P("Testing AsyncSelectInput with category selection:")
                   | new AsyncSelectDemo()
               ).Width(Size.Units(60))
               | new Card(
                   Layout.Vertical()
                   | Text.H2("Other Inputs Ellipsis Tests")
                   | Text.P("Small-width inputs should show ellipsis for long labels, placeholders and options:")
                   // Bool input with long label/description
                   | Text.P("BoolInput with long label and description (small width):")
                   | boolState
                       .ToBoolInput()
                       .Label("Enable the feature with a very long descriptive label that should not overflow the container and should be truncated appropriately with ellipsis when the width is constrained")
                       .Description("This description explains in detail what the toggle does and is intentionally very long to verify that descriptions are also truncated with ellipsis when there is not enough space")
                       .Width(Size.Units(20))
                   // Color input with long placeholder
                   | Text.P("ColorInput with long placeholder (small width):")
                   | colorState
                       .ToColorInput()
                       .Placeholder("Select a color or paste a hex code like #ff00ff; this placeholder is long to test ellipsis behavior in narrow inputs")
                       .Variant(ColorInputs.TextAndPicker)
                       .Width(Size.Units(20))
                   // Select input with long options and placeholder
                   | Text.P("SelectInput with long option and placeholder (small width):")
                   | selectedLang
                       .ToSelectInput(languages.ToOptions(), "Choose your favourite language from this incredibly long placeholder text that should be truncated with ellipsis in a small select")
                       .Variant(SelectInputs.Select)
                       .Width(Size.Units(40))
               ).Width(Size.Units(60))

               | Text.H2("Expected Behavior")
               | Text.P("Expected ellipsis behavior:")
               | Text.P("• TextInput should show ellipsis (...) when text exceeds the input field width")
               | Text.P("• Placeholder text should also be truncated if too long")
               | Text.P("• Error messages should be truncated if they exceed available space")
               | Text.P("• All states (normal, disabled, invalid) should handle ellipsis consistently")
               | Text.P("• Different sizes should maintain proper ellipsis behavior")
               | Text.P("• Text should be selectable and editable even when truncated")
               | Text.P("• List items should truncate long titles, subtitles, and badges with ellipsis")
               | Text.P("• Tooltips should appear on hover for truncated list content");
    }
}

public class AsyncSelectDemo : ViewBase
{
    private static readonly string[] Categories = { "akdjsfhjksdfhakjslfhasjkdfhasdjkfhakjsdfhklhafsdf", "Clothing", "Books", "Home & Garden", "Sports" };

    public override object? Build()
    {
        var selectedCategory = this.UseState<string?>(default(string?));

        Task<Option<string>[]> QueryCategories(string query)
        {
            return System.Threading.Tasks.Task.FromResult(Categories
                .Where(c => c.Contains(query, StringComparison.OrdinalIgnoreCase))
                .Select(c => new Option<string>(c))
                .ToArray());
        }

        Task<Option<string>?> LookupCategory(string? category)
        {
            return System.Threading.Tasks.Task.FromResult(category != null ? new Option<string>(category) : null);
        }

        return Layout.Vertical()
            | Text.Label("Select a category:")
            | (Layout.Horizontal()
                | selectedCategory.ToAsyncSelectInput(QueryCategories, LookupCategory, "Search categories...Search categories...Search categories...Search categories...Search categories..."))
                .Width(Size.Units(20))
            | Text.Small($"Selected: {selectedCategory.Value ?? "None"}");
    }
}
