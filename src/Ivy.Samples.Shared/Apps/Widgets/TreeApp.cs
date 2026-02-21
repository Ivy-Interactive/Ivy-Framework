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
                new TreeItem("src", items: [
                    new TreeItem("components", items: [
                        new TreeItem("Button.tsx")
                            .Icon(Icons.Code)
                            .HandleClick(() => selectedItem.Set("Button.tsx")),
                        new TreeItem("Card.tsx")
                            .Icon(Icons.Code)
                            .HandleClick(() => selectedItem.Set("Card.tsx")),
                        new TreeItem("Dialog.tsx")
                            .Icon(Icons.Code)
                            .HandleClick(() => selectedItem.Set("Dialog.tsx"))
                    ])
                        .Icon(Icons.Folder)
                        .Open(),
                    new TreeItem("hooks", items: [
                        new TreeItem("useAuth.ts")
                            .Icon(Icons.Code)
                            .HandleClick(() => selectedItem.Set("useAuth.ts")),
                        new TreeItem("useTheme.ts")
                            .Icon(Icons.Code)
                            .HandleClick(() => selectedItem.Set("useTheme.ts"))
                    ]).Icon(Icons.Folder),
                    new TreeItem("App.tsx")
                        .Icon(Icons.Code)
                        .HandleClick(() => selectedItem.Set("App.tsx")),
                    new TreeItem("index.ts")
                        .Icon(Icons.Code)
                        .HandleClick(() => selectedItem.Set("index.ts"))
                ])
                    .Icon(Icons.Folder)
                    .Open(),
                new TreeItem("public", items: [
                    new TreeItem("favicon.ico")
                        .Icon(Icons.Image)
                        .HandleClick(() => selectedItem.Set("favicon.ico")),
                    new TreeItem("index.html")
                        .Icon(Icons.Globe)
                        .HandleClick(() => selectedItem.Set("index.html"))
                ]).Icon(Icons.Folder),
                new TreeItem("package.json")
                    .Icon(Icons.Braces)
                    .HandleClick(() => selectedItem.Set("package.json")),
                new TreeItem("README.md")
                    .Icon(Icons.BookOpen)
                    .HandleClick(() => selectedItem.Set("README.md"))
            )

            | Text.H2("Without Lines")
            | new Tree(
                new TreeItem("Documents", items: [
                    new TreeItem("Reports", items: [
                        new TreeItem("Q1 Report.pdf").Icon(Icons.FileText),
                        new TreeItem("Q2 Report.pdf").Icon(Icons.FileText)
                    ]).Icon(Icons.Folder).Open(),
                    new TreeItem("Photos", items: [
                        new TreeItem("vacation.jpg").Icon(Icons.Image),
                        new TreeItem("profile.png").Icon(Icons.Image)
                    ]).Icon(Icons.Folder)
                ]).Icon(Icons.Folder).Open()
            ).HideLines()

            | Text.H2("Disabled Items")
            | new Tree(
                new TreeItem("Available", items: [
                    new TreeItem("editable.txt")
                        .Icon(Icons.FileText)
                        .HandleClick(() => selectedItem.Set("editable.txt")),
                    new TreeItem("read-only.txt")
                        .Icon(Icons.Lock)
                        .Disabled()
                ])
                    .Icon(Icons.Folder)
                    .Open(),
                new TreeItem("Restricted")
                    .Icon(Icons.FolderLock)
                    .Disabled()
            );
    }
}
