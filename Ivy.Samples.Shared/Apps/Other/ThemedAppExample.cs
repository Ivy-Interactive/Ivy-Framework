using Ivy.Core;
using Ivy.Shared;
using Ivy.Themes;
using Ivy.Views;

namespace Ivy.Samples.Shared.Apps.Other;

[App(icon: Icons.Brush, path: ["Other", "UI"])]
public class ThemedAppExample : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();

        return Layout.Vertical()
            | Text.H1("Themed Application Example")
            | Text.Block("This example demonstrates how components automatically use the theme colors configured in your server.")

            // Button examples
            | Text.H2("Buttons")
            | Text.Block("Buttons automatically use theme colors based on their variants:")
            | Layout.Horizontal(
                new Button("Primary"),
                new Button("Secondary").Variant(ButtonVariant.Secondary),
                new Button("Outline").Variant(ButtonVariant.Outline),
                new Button("Ghost").Variant(ButtonVariant.Ghost),
                new Button("Link").Variant(ButtonVariant.Link)
            ).Wrap()

            // Input examples
            | Text.H2("Form Elements")
            | Text.Block("Form inputs use theme colors for borders, backgrounds, and focus states:")
            | UseState("").ToTextInput()
                .Placeholder("Text input")
            | Text.Block("Select an Option")
            | UseState("Option 1").ToSelectInput(
                new[] { "Option 1", "Option 2", "Option 3" }.ToOptions()
            )
            | UseState(false).ToBoolInput()
                .Variant(BoolInputs.Switch)
                .Label("Enable feature")
                .Description("Toggle switches use primary color when active")

            // Text variants
            | Text.H2("Text Variants")
            | Text.Block("Text components have built-in semantic colors:")
            | Layout.Vertical()
                | Text.Success("Success: Operation completed successfully")
                | Text.Warning("Warning: Please review before proceeding")
                | Text.Danger("Error: Something went wrong")
                | Text.Muted("Muted: Secondary information")

            // Cards
            | Text.H2("Cards")
            | Text.Block("Cards use theme colors for borders and backgrounds:")
            | Layout.Horizontal(
                new Card(
                    Layout.Vertical()
                        | Text.Block("Standard Card")
                        | Text.Small("This card uses default theme styling")
                ).Title("Default Card"),
                new Card(
                    Layout.Vertical()
                        | Text.Block("With Icon")
                        | Text.Small("Cards can include icons")
                ).Title("Icon Card").Icon(Icons.Star)
            )

            // Code example
            | Text.H2("Using Themes in Your App")
            | new Code(@"// Configure theme in your server startup:
var server = new Server()
    .UseTheme(theme => 
    {
        theme.Name = ""My App Theme"";
        theme.Colors = new ThemeColors
        {
            Primary = ""#0077BE"",
            PrimaryForeground = ""#FFFFFF"",
            Secondary = ""#5B9BD5"",
            SecondaryForeground = ""#FFFFFF"",
            // ... configure other colors
        };
    });

// Components automatically use theme colors
// No need to specify colors manually!", Languages.Csharp)

            // Info section
            | new Card(
                Layout.Vertical()
                    | Text.Block("💡 Theme System Benefits")
                    | Layout.Vertical()
                        | Text.Block("• Consistent colors across your entire application")
                        | Text.Block("• Easy to switch themes by changing server configuration")
                        | Text.Block("• Supports light/dark mode via CSS variables")
                        | Text.Block("• Semantic colors for success, warning, error states")
                    | new Button("View Theme Customizer")
                    {
                        OnClick = _ =>
                        {
                            client.Toast("Navigate to Other > UI > Theme Customizer", "Info");
                            return ValueTask.CompletedTask;
                        },
                        Icon = Icons.Palette
                    }
            ).Title("Learn More")
        ;
    }
}
