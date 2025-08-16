using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets.Primitives;

[App(icon: Icons.Text, path: ["Widgets", "Primitives"])]
public class TextApp : SampleBase
{
    protected override object? BuildSample()
    {
        var left = Layout.Vertical(
            Text.H1("H1"),
            Text.P("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum."),
            Text.H2("H2"),
            Text.P("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.")
        // Text.H2("H2"),
        // Text.H3("H3"),
        // Text.H4("H4")
        // Text.Block("Block"),
        // Text.Blockquote("Blockquote"),
        // Text.InlineCode("InlineCode"),
        // Text.Lead("Lead"),
        // Text.Large("Large"),
        // Text.Small("Small"),
        // Text.Muted("Muted"),
        // Text.Danger("Danger"),
        // Text.Warning("Warning"),
        // Text.Success("Success")
        //Text.Code("Code"),
        //Text.Markdown("# Markdown"),
        //Text.Json("{ \"Json\": \"Json\" }"),
        //Text.Xml("<xml>Xml</xml>"),
        //Text.Html("<div>Html</div>").
        //Text.Literal("Literal")
        );

        var right = Layout.Vertical(
            Text.Markdown("""
                          # H1
                          
                          Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.
                          
                          ## H2
                          
                          Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.
                          
                          """)
        );

        return Layout.Horizontal(
            left,
            right
        );
    }
}