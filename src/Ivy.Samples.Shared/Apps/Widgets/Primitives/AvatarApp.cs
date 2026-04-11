
namespace Ivy.Samples.Shared.Apps.Widgets.Primitives;

[App(icon: Icons.CircleUser, group: ["Widgets", "Primitives"], searchHints: ["avatar", "profile", "user", "image", "photo", "picture", "circle"])]
public class AvatarApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Tabs(
            new Tab("Basic", new AvatarBasicExample()),
            new Tab("Sizes", new AvatarSizesExample()),
            new Tab("Colors", new AvatarColorsExample()),
            new Tab("Team", new AvatarTeamExample()),
            new Tab("Cards", new AvatarCardsExample())
        ).Variant(TabsVariant.Content);
    }
}

public class AvatarBasicExample : ViewBase
{
    public override object? Build()
    {
        return Layout.Horizontal()
            | new Avatar("Niels Bosma", "https://api.images.cat/150/150?1")
            | new Avatar("Niels Bosma");
    }
}

public class AvatarSizesExample : ViewBase
{
    public override object? Build()
    {
        return Layout.Horizontal()
            | new Avatar("Small", "https://api.images.cat/150/150?1").Small()
            | new Avatar("Medium", "https://api.images.cat/150/150?2").Medium()
            | new Avatar("Large", "https://api.images.cat/150/150?3").Large();
    }
}

public class AvatarColorsExample : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical()
            | Text.Muted("When the image fails to load (or is not provided), the fallback uses the specified color.")
            | Layout.Horizontal()
                | new Avatar("Primary").Color(Colors.Primary)
                | new Avatar("Secondary").Color(Colors.Secondary)
                | new Avatar("Destructive").Color(Colors.Destructive)
                | new Avatar("Success").Color(Colors.Success)
                | new Avatar("Warning").Color(Colors.Warning)
                | new Avatar("Info").Color(Colors.Info)
                | new Avatar("Blue").Color(Colors.Blue)
                | new Avatar("Green").Color(Colors.Green)
                | new Avatar("Pink").Color(Colors.Pink);
    }
}

public class AvatarTeamExample : ViewBase
{
    public override object? Build()
    {
        var team = new Dictionary<string, string>
        {
            { "Niels Bosma",    "https://api.images.cat/150/150?1" },
            { "Mikael Rinne",   "https://api.images.cat/150/150?2" },
            { "Renco Smeding",  "https://api.images.cat/150/150?3" },
            { "Jesper",         "https://api.images.cat/150/150?4" },
            { "Frida Bosma",    "https://api.images.cat/150/150?5" },
            { "Viktor Bolin",   "https://api.images.cat/150/150?6" },
        };

        var teamGrid = Layout.Grid().Columns(3).Rows(2);
        foreach (var (name, url) in team)
        {
            teamGrid = teamGrid
                | new Card(new Avatar(name, url).Height(Size.Units(200)).Width(Size.Units(100)))
                      .Title(name);
        }

        return teamGrid;
    }
}

public class AvatarCardsExample : ViewBase
{
    public override object? Build()
    {
        return Layout.Horizontal()
            | new Card(
                  new Avatar("Köttbullar", "https://api.images.cat/150/150?7"),
                  new Button("Add to order"))
                .Title("Köttbullar")
                .Description("The quintessential Swedish food.")
                .Width(Size.Units(100))
            | new Card(
                  new Avatar("Pytt i Panna", "https://api.images.cat/150/150?8"),
                  new Button("Add to order"))
                .Title("Pytt i Panna")
                .Description("Hearty hash of potatoes, onions, and meat.")
                .Width(Size.Units(100));
    }
}
