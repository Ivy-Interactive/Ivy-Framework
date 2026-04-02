namespace Ivy.Samples.Shared.Apps.Widgets;

[App(icon: Icons.Menu, group: ["Widgets"], searchHints: ["toolbar", "actions", "buttons", "controls"])]
public class ToolbarApp : SampleBase
{
    protected override object? BuildSample()
    {
        var client = UseService<IClientProvider>();
        var activeTools = UseState<HashSet<string>>(new HashSet<string> { "bold" });

        void ToggleTool(string tool)
        {
            var tools = new HashSet<string>(activeTools.Value);
            if (tools.Contains(tool))
                tools.Remove(tool);
            else
                tools.Add(tool);
            activeTools.Set(tools);
        }

        return Layout.Vertical()
            | Text.H1("Toolbar")

            | Text.H2("Basic Toolbar")
            | Text.P("Organize actions with groups and separators using MenuItem objects.")
            | (new Toolbar()
                | new MenuItem(Label: "Save", Icon: Icons.Save, Tag: "save")
                    .OnSelect(_ => client.Toast("Saved!"))
                | new MenuItem(Label: "Undo", Icon: Icons.Undo, Tag: "undo")
                    .OnSelect(_ => client.Toast("Undo"))
                | new MenuItem(Label: "Redo", Icon: Icons.Redo, Tag: "redo")
                    .OnSelect(_ => client.Toast("Redo"))
                | MenuItem.Separator()
                | MenuItem.Default(Icons.ZoomIn, tag: "zoom-in")
                    .Tooltip("Zoom In")
                    .OnSelect(_ => client.Toast("Zoom In"))
                | MenuItem.Default(Icons.ZoomOut, tag: "zoom-out")
                    .Tooltip("Zoom Out")
                    .OnSelect(_ => client.Toast("Zoom Out"))
            )

            | Text.H2("Text Editor Toolbar")
            | Text.P("Use MenuItem.Group() to create logical groupings and Checked state for toggle buttons.")
            | (new Toolbar(
                    new MenuItem(
                        Variant: MenuItemVariant.Group,
                        Children: [
                            MenuItem.Default(Icons.Bold, tag: "bold")
                                .Tooltip("Bold")
                                .Checked(activeTools.Value.Contains("bold"))
                                .OnSelect(_ => ToggleTool("bold")),
                            MenuItem.Default(Icons.Italic, tag: "italic")
                                .Tooltip("Italic")
                                .Checked(activeTools.Value.Contains("italic"))
                                .OnSelect(_ => ToggleTool("italic")),
                            MenuItem.Default(Icons.Underline, tag: "underline")
                                .Tooltip("Underline")
                                .Checked(activeTools.Value.Contains("underline"))
                                .OnSelect(_ => ToggleTool("underline"))
                        ]
                    ),
                    MenuItem.Separator(),
                    new MenuItem(
                        Variant: MenuItemVariant.Group,
                        Children: [
                            MenuItem.Default(Icons.TextAlignStart, tag: "align-left")
                                .Tooltip("Align Left")
                                .OnSelect(_ => client.Toast("Align Left")),
                            MenuItem.Default(Icons.TextAlignCenter, tag: "align-center")
                                .Tooltip("Align Center")
                                .OnSelect(_ => client.Toast("Align Center")),
                            MenuItem.Default(Icons.TextAlignEnd, tag: "align-right")
                                .Tooltip("Align Right")
                                .OnSelect(_ => client.Toast("Align Right"))
                        ]
                    ),
                    MenuItem.Separator(),
                    new MenuItem(
                        Variant: MenuItemVariant.Group,
                        Children: [
                            MenuItem.Default(Icons.Link, tag: "link")
                                .Tooltip("Insert Link")
                                .OnSelect(_ => client.Toast("Insert Link")),
                            MenuItem.Default(Icons.Image, tag: "image")
                                .Tooltip("Insert Image")
                                .OnSelect(_ => client.Toast("Insert Image"))
                        ]
                    )
                )
            )
            | Text.P($"Active formatting: {string.Join(", ", activeTools.Value)}").Muted()

            | Text.H2("Disabled Toolbar")
            | Text.P("The entire toolbar can be disabled.")
            | (new Toolbar()
                | MenuItem.Default("Action 1", tag: "action1").Icon(Icons.Wand)
                | MenuItem.Default("Action 2", tag: "action2").Icon(Icons.Sparkles)
            ).Disabled()

            | Text.H2("Icon-Only Toolbar")
            | Text.P("Icon-only buttons are compact. Always provide tooltips for accessibility.")
            | (new Toolbar()
                | MenuItem.Default(Icons.Copy, tag: "copy")
                    .Tooltip("Copy")
                    .OnSelect(_ => client.Toast("Copied"))
                | MenuItem.Default(Icons.Scissors, tag: "cut")
                    .Tooltip("Cut")
                    .OnSelect(_ => client.Toast("Cut"))
                | MenuItem.Default(Icons.ClipboardPaste, tag: "paste")
                    .Tooltip("Paste")
                    .OnSelect(_ => client.Toast("Pasted"))
            );
    }
}
