---
searchHints:
  - tree
  - hierarchy
  - folder
  - file
  - directory
  - structure
  - nested
---

# Tree

<Ingress>
Display hierarchical data structures like file trees, nested categories, and organizational charts with collapsible nodes.
</Ingress>

The `Tree` [widget](../../01_Onboarding/02_Concepts/03_Widgets.md) renders recursive data in a familiar tree view. Each `TreeItem` can contain nested `TreeItem`s, supports icons, click events, and expand/collapse behavior. All properties are configurable via fluent extension methods.

## Basic Usage

```csharp demo-below
new Tree(
    new TreeItem("src", items: [
        new TreeItem("components", items: [
            new TreeItem("Button.tsx").Icon(Icons.Code),
            new TreeItem("Card.tsx").Icon(Icons.Code)
        ])
            .Icon(Icons.Folder)
            .Open(),
        new TreeItem("App.tsx").Icon(Icons.Code)
    ])
        .Icon(Icons.Folder)
        .Open()
)
```

## Icons and Click Events

TreeItems support icons and click handlers for interactive trees.

```csharp demo-tabs
public class TreeClickDemo : ViewBase
{
    public override object? Build()
    {
        var selected = UseState("");

        return Layout.Vertical().Gap(2)
            | Text.Block($"Selected: {(string.IsNullOrEmpty(selected.Value) ? "nothing" : selected.Value)}")
            | new Tree(
                new TreeItem("src")
                    .Icon(Icons.Folder)
                    .Open()
                    .HandleClick(() => selected.Set("src")),
                new TreeItem("App.tsx")
                    .Icon(Icons.Code)
                    .HandleClick(() => selected.Set("App.tsx")),
                new TreeItem("index.ts")
                    .Icon(Icons.Code)
                    .HandleClick(() => selected.Set("index.ts"))
            );
    }
}
```

## Styling

### ShowLines and HideLines

Guide lines visually connect parent and child nodes. They are enabled by default and can be hidden with `.HideLines()`.

```csharp demo-tabs
Layout.Vertical().Gap(4)
    | Text.Block("With lines (default)")
    | new Tree(
        new TreeItem("Root", items: [
            new TreeItem("Child A").Icon(Icons.FileText),
            new TreeItem("Child B").Icon(Icons.FileText)
        ]).Icon(Icons.Folder).Open()
    )
    | Text.Block("Without lines")
    | new Tree(
        new TreeItem("Root", items: [
            new TreeItem("Child A").Icon(Icons.FileText),
            new TreeItem("Child B").Icon(Icons.FileText)
        ]).Icon(Icons.Folder).Open()
    ).HideLines()
```

### Disabled Items

Individual tree items can be disabled to prevent interaction.

```csharp demo-tabs
new Tree(
    new TreeItem("Available", items: [
        new TreeItem("editable.txt").Icon(Icons.FileText),
        new TreeItem("read-only.txt").Icon(Icons.Lock).Disabled()
    ]).Icon(Icons.Folder).Open(),
    new TreeItem("Restricted")
        .Icon(Icons.FolderLock)
        .Disabled()
)
```

<WidgetDocs Type="Ivy.Tree" ExtensionTypes="Ivy.TreeExtensions, Ivy.TreeItemExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/src/Ivy/Widgets/Tree/Tree.cs"/>
