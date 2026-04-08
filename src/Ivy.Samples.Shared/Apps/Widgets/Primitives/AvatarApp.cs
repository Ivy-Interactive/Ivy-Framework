
namespace Ivy.Samples.Shared.Apps.Widgets.Primitives;

[App(icon: Icons.CircleUser, group: ["Widgets", "Primitives"], searchHints: ["avatar", "profile", "user", "image", "photo", "picture", "circle"])]
public class AvatarApp : SampleBase
{
    protected override object? BuildSample()
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

        return Layout.Vertical()

               | Text.H1("Avatar")

               | Text.H2("Basic")
               | Layout.Horizontal()
                   | new Avatar("Niels Bosma", "https://api.images.cat/150/150?1")
                   | new Avatar("Niels Bosma")

               | Text.H2("Sizes")
               | Layout.Horizontal()
                   | new Avatar("S", "https://api.images.cat/150/150?1").Width(Size.Units(6)).Height(Size.Units(6))
                   | new Avatar("M", "https://api.images.cat/150/150?2").Width(Size.Units(10)).Height(Size.Units(10))
                   | new Avatar("L", "https://api.images.cat/150/150?3").Width(Size.Units(16)).Height(Size.Units(16))
                   | new Avatar("XL", "https://api.images.cat/150/150?4").Width(Size.Units(24)).Height(Size.Units(24))

               | Text.H2("Colors")
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
                   | new Avatar("Pink").Color(Colors.Pink)

               | Text.H2("Team")
               | teamGrid

               | Text.H2("Integration with Cards")
               | Layout.Horizontal()
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
                       .Width(Size.Units(100))
               ;
    }
}
