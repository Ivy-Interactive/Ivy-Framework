using Ivy.Shared;
using Ivy.Samples.Shared.Apps;

namespace Ivy.Samples.Shared.Apps.Tests;

[App(icon: Icons.Columns3, path: ["Tests"], isVisible: true, searchHints: ["markdown", "html", "rendering", "formatting", "comparison", "preview"])]
public class DifferentFontCasesApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Vertical()
            | Text.Block("aaaaaaaaaaaaaaaaaaaa")
            | Text.Literal("aaaaaaaaaaaaaaaaaaaa")
            | Text.P("aaaaaaaaaaaaaaaaaaaa")
            | Text.P("aaaaaaaaaaaaaaaaaaaa").Italic()
            | Text.P("aaaaaaaaaaaaaaaaaaaa").Bold()
            | Text.P("aaaaaaaaaaaaaaaaaaaa").Bold().Italic()
            | Text.P("aaaaaaaaaaaaaaaaaaaa").Muted()
            | Text.Muted("aaaaaaaaaaaaaaaaaaaa")
            | Text.Muted("aaaaaaaaaaaaaaaaaaaa").Italic()
            | Text.Muted("aaaaaaaaaaaaaaaaaaaa").Bold()
            | Text.Muted("aaaaaaaaaaaaaaaaaaaa").Bold().Italic()
            | Text.Markdown("aaaaaaaaaaaaaaaaaa");
    }
}