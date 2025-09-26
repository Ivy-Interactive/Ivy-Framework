# Icon

<Ingress>
Display beautiful vector icons from the comprehensive Lucide icon set with customizable colors, sizes, and styling options.
</Ingress>

The `Icon` widget displays vector icons from Ivy's built-in icon set. Icons enhance visual communication and can be customized with different colors and sizes.

## Lucide Icons

We use the [Lucide Icons](https://lucide.dev/icons/) set, which is a collection of 1000+ free icons. You can find the full set [here](https://lucide.dev/icons/).

```csharp demo-tabs 
Layout.Horizontal()
    | new Icon(Icons.Clipboard)
    | new Icon(Icons.Settings)
    | new Icon(Icons.User)
    | new Icon(Icons.Calendar)
    | new Icon(Icons.Mail)
```

## Colors

You can customize the color of the icons using the `Color` parameter.

```csharp demo-tabs 
Layout.Horizontal()
    | new Icon(Icons.Clipboard, Colors.Red)
    | new Icon(Icons.Settings, Colors.Green)
    | new Icon(Icons.User, Colors.Blue)
    | new Icon(Icons.Calendar, Colors.Purple)
    | new Icon(Icons.Mail, Colors.Orange)
```

## Sizes

You can also customize the size of the icons using the `Size` parameter.

```csharp demo-tabs 
Layout.Horizontal()
    | new Icon(Icons.Cloud).Small()
    | new Icon(Icons.Cloud)
    | new Icon(Icons.Cloud).Large()
```

<WidgetDocs Type="Ivy.Icon" ExtensionTypes="Ivy.IconExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Primitives/Icon.cs"/>
