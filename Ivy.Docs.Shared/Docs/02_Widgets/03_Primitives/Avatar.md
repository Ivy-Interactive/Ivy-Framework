# Avatar

<Ingress>
Display user or entity representations with automatic fallbacks from images to initials to placeholders for consistent visual identity.
</Ingress>

`Avatars` are graphical representations of users or entities. They display an image if available or fall back to initials or a placeholder when no image is provided.

To create a new avatar, it is recommended to use a layout.

Make sure to define a name and supply a `url` to fetch the image.

If no image is provided, a default avatar will be used, showing the first letters of the name.

```csharp demo-tabs
Layout.Horizontal()
    | new Avatar("Niels Bosma", "https://api.images.cat/150/150?1")
    | new Avatar("Niels Bosma")
```

## Practical Usage

It's possible to create a dictionary where each object contains a name and an associated avatar.

`Avatars` can be used to showcase Teams like this.

```csharp demo-tabs 
public class IvyTeamDemo : ViewBase
{
    public override object? Build()
    {
        var team = new Dictionary<string,string>()
        {
             {"Niels Bosma",
                 "https://api.images.cat/150/150?1"},
             {"Mikael Rinne",
                 "https://api.images.cat/150/150?2"},
             {"Renco Smeding",
                 "https://api.images.cat/150/150?3"},
             {"Jesper",
                 "https://api.images.cat/150/150?4"},
             {"Frida Bosma",
                 "https://api.images.cat/150/150?5"},
             {"Viktor Bolin",
                 "https://api.images.cat/150/150?6"},
        };

        var layout = Layout.Grid()
                           .Columns(3)
                           .Rows(2);


        foreach(var key in team.Keys)
        {
            layout = layout
                      |new Card(new Avatar(key, team[key]).Height(200).Width(100))
                            .Title(key);
        }
        return Layout.Vertical()
                      | H3("Ivy Team")
                      | layout;
    }
}
```

### Integration with Other Widgets

Avatars can be integrated into other widgets, including cards, add buttons, and more.

```csharp demo-tabs 
public class AvatarAsFoodIcon : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical()
                |
                new Card(
                new Avatar("Köttbullar","https://api.images.cat/150/150?7"),
                new Button("Add to order")).Title("Köttbullar")
                    .Description("The quintessential Swedish food, these meatballs are more than just a dish; they're a cultural icon.")
                    .Width(Size.Units(100))
                |
                new Card(
                new Avatar("Pytt i Panna","https://api.images.cat/150/150?8"),
                new Button("Add to order")).Title("Pytt i Panna")
                    .Description("Translating to small pieces in a pan, this hearty hash of potatoes, onions, and meat is a beloved comfort food. It's a brilliant way to use leftovers and is often crowned with a fried egg.")
                    .Width(Size.Units(100));
    }
}

```

<WidgetDocs Type="Ivy.Avatar" ExtensionTypes="Ivy.AvatarExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Primitives/Avatar.cs"/>
