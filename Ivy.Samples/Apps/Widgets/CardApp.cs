using Ivy.Shared;

namespace Ivy.Samples.Apps.Widgets;

[App(icon: Icons.IdCard, path: ["Widgets"])]
public class CardApp : SampleBase
{
    protected override object? BuildSample()
    {
        var client = UseService<IClientProvider>();

        var card1 = new Card(
            "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam nec purus nec nunc",
            new Button("Sign Me Up", _ => client.Toast("You have signed up!"))
        ).Title("Card App").Description("This is a card app.");

        var card2 = new Card(
            "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam nec purus nec nunc"
        );

        var menu = new List(
            new ListItem("Products", onClick: _ => { }, badge: 100.ToString()),
            new ListItem("Balance", onClick: _ => { }, icon: Icons.ChevronRight)
        );

        var card3 = new Card(
            menu
        );

        return Layout.Vertical().Width(Size.Units(100))
               | card1
               | card2
               | card3
            ;
    }
}