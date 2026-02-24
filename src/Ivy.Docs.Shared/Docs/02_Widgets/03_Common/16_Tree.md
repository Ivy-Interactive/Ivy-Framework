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

The `Tree` [widget](../../01_Onboarding/02_Concepts/03_Widgets.md) renders recursive data in a familiar tree view using `MenuItem` for each node. Each `MenuItem` can contain nested children, supports icons, click events, and expand/collapse behavior.

## Basic Usage

```csharp demo-below
new Tree(
    new MenuItem("src")
        .Icon(Icons.Folder)
        .Expanded()
        .Children(
            new MenuItem("components")
                .Icon(Icons.Folder)
                .Expanded()
                .Children(
                    new MenuItem("Button.tsx").Icon(Icons.Code),
                    new MenuItem("Card.tsx").Icon(Icons.Code)
                ),
            new MenuItem("App.tsx").Icon(Icons.Code)
        )
)
```

## Click Events

Use `HandleSelect` on the Tree and `Tag` on each MenuItem to handle clicks. The `DefaultSelectHandler` routes events to individual `MenuItem.OnSelect` handlers.

```csharp demo-tabs
public class TreeClickDemo : ViewBase
{
    public override object? Build()
    {
        var selected = UseState("");

        return Layout.Vertical().Gap(2)
            | Text.Block($"Selected: {(string.IsNullOrEmpty(selected.Value) ? "nothing" : selected.Value)}")
            | new Tree(
                new MenuItem("src")
                    .Icon(Icons.Folder)
                    .Expanded()
                    .Children(
                        new MenuItem("App.tsx").Icon(Icons.Code).Tag("App.tsx"),
                        new MenuItem("index.ts").Icon(Icons.Code).Tag("index.ts")
                    )
            ).HandleSelect(e => selected.Set(e.Value?.ToString() ?? ""));
    }
}
```

## Disabled Items

Individual menu items can be disabled to prevent interaction.

```csharp demo-tabs
new Tree(
    new MenuItem("Available")
        .Icon(Icons.Folder)
        .Expanded()
        .Children(
            new MenuItem("editable.txt").Icon(Icons.FileText).Tag("editable"),
            new MenuItem("read-only.txt").Icon(Icons.Lock).Disabled()
        ),
    new MenuItem("Restricted")
        .Icon(Icons.FolderLock)
        .Disabled()
)
```

<WidgetDocs Type="Ivy.Tree" ExtensionTypes="Ivy.TreeExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/src/Ivy/Widgets/Tree/Tree.cs"/>
