using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets;

[App(icon: Icons.FolderTree, searchHints: ["tree", "hierarchy", "folder", "file", "structure", "directory", "nested"])]
public class TreeApp : SampleBase
{
    protected override object? BuildSample()
    {
        var selectedItem = UseState("Nothing selected");

        return Layout.Vertical()
            | Text.H1("Tree")
            | Text.Muted($"Selected: {selectedItem.Value}")
            | new Tree(
                new MenuItem("src")
                    .Icon(Icons.Folder)
                    .Expanded()
                    .Children(
                        new MenuItem("components")
                            .Icon(Icons.Folder)
                            .Expanded()
                            .Children(
                                new MenuItem("Button.tsx").Icon(Icons.Code).Tag("Button.tsx"),
                                new MenuItem("Card.tsx").Icon(Icons.Code).Tag("Card.tsx"),
                                new MenuItem("Dialog.tsx").Icon(Icons.Code).Tag("Dialog.tsx")
                            ),
                        new MenuItem("hooks")
                            .Icon(Icons.Folder)
                            .Children(
                                new MenuItem("useAuth.ts").Icon(Icons.Code).Tag("useAuth.ts"),
                                new MenuItem("useTheme.ts").Icon(Icons.Code).Tag("useTheme.ts")
                            ),
                        new MenuItem("App.tsx").Icon(Icons.Code).Tag("App.tsx"),
                        new MenuItem("index.ts").Icon(Icons.Code).Tag("index.ts")
                    ),
                new MenuItem("public")
                    .Icon(Icons.Folder)
                    .Children(
                        new MenuItem("favicon.ico").Icon(Icons.Image).Tag("favicon.ico"),
                        new MenuItem("index.html").Icon(Icons.Globe).Tag("index.html")
                    ),
                new MenuItem("package.json").Icon(Icons.Braces).Tag("package.json"),
                new MenuItem("README.md").Icon(Icons.BookOpen).Tag("README.md")
            ).HandleSelect(e => selectedItem.Set(e.Value?.ToString() ?? ""))

            | Text.H2("Disabled Items")
            | new Tree(
                new MenuItem("Available")
                    .Icon(Icons.Folder)
                    .Expanded()
                    .Children(
                        new MenuItem("editable.txt").Icon(Icons.FileText).Tag("editable.txt"),
                        new MenuItem("read-only.txt").Icon(Icons.Lock).Disabled()
                    ),
                new MenuItem("Restricted").Icon(Icons.FolderLock).Disabled()
            );
    }
}
