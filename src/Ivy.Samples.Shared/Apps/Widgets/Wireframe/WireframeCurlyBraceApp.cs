namespace Ivy.Samples.Shared.Apps.Widgets.Wireframe;

[App(icon: Icons.Braces, group: ["Widgets", "Wireframe"], searchHints: ["wireframe", "brace", "curly", "bracket", "group", "sketch"])]
public class WireframeCurlyBraceApp : SampleBase
{
    protected override object? BuildSample()
    {
        var horizontal = Layout.Horizontal()
            | new WireframeCurlyBrace()
            | new WireframeCurlyBrace().Height(Size.Px(90))
            | new WireframeCurlyBrace().Width(Size.Px(40)).Height(Size.Px(200))
            | new WireframeCurlyBrace(CurlyBraceVariant.Horizontal, Colors.Red)
            | new WireframeCurlyBrace(CurlyBraceVariant.Horizontal, Colors.Sky);

        var vertical = Layout.Vertical()
            | new WireframeCurlyBrace(CurlyBraceVariant.Vertical)
            | new WireframeCurlyBrace(CurlyBraceVariant.Vertical).Width(Size.Px(300)).Height(Size.Px(40))
            | new WireframeCurlyBrace(CurlyBraceVariant.Vertical, Colors.Green).Width(Size.Px(220));

        var densities = Layout.Horizontal()
            | new WireframeCurlyBrace().Small()
            | new WireframeCurlyBrace()
            | new WireframeCurlyBrace().Large();

        var grouping = Layout.Horizontal()
            | new WireframeCurlyBrace().Height(Size.Px(120))
            | (Layout.Vertical()
                | Text.Literal("Display name")
                | Text.Literal("Email address")
                | Text.Literal("Password"))
            | new WireframeNote("These three move\nto the profile page", Colors.Yellow);

        return Layout.Vertical()
            | Text.H1("Wireframe Curly Brace")
            | Text.H2("Horizontal")
            | horizontal
            | Text.H2("Vertical")
            | vertical
            | Text.H2("Densities")
            | densities
            | Text.H2("Grouping Rows")
            | grouping;
    }
}
