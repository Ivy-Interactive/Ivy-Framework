namespace Ivy.Samples.Shared.Apps.Widgets.Wireframe;

[App(icon: Icons.Smartphone, group: ["Widgets", "Wireframe"], searchHints: ["wireframe", "mockup", "device", "phone", "browser", "monitor", "sketch"])]
public class WireframeMockupApp : SampleBase
{
    protected override object? BuildSample()
    {
        var phone = new WireframeMockup(
            Layout.Vertical().Padding(3)
            | Text.H4("Inbox")
            | new TextInput().Placeholder("Search")
            | new WireframePlaceholder("Avatar", Colors.Slate).Width(Size.Full()).Height(Size.Px(60))
            | Text.Muted("Jerry Reisndorf")
            | new Separator()
            | Text.Muted("Phil Jackson")
            | new Button("Compose").Width(Size.Full()));

        var site = new WireframeMockup(
                Layout.Vertical().Padding(4)
                | Text.H3("Product Name")
                | Text.Muted("Lorem ipsum dolor sit amet, consectetur adipisicing elit.")
                | new WireframePlaceholder("Hero", Colors.Slate).Width(Size.Px(240)).Height(Size.Px(120))
                | new Button("Buy now"))
            .Variant(MockupVariant.Website)
            .Url("https://thenewnewthing.com");

        var tablet = new WireframeMockup(
                Layout.Vertical().Padding(3)
                | Text.H4("Library")
                | new WireframePlaceholder("Cover", Colors.Slate).Width(Size.Full()).Height(Size.Px(90))
                | new Button("Open").Width(Size.Full()))
            .Variant(MockupVariant.Tablet)
            .Width(Size.Px(380))
            .Height(Size.Px(500));

        var desktop = new WireframeMockup(
                Layout.Vertical().Padding(3)
                | Text.H4("Dashboard")
                | new WireframePlaceholder("Chart", Colors.Teal).Width(Size.Px(140)).Height(Size.Px(80))
                | new Progress(62))
            .Variant(MockupVariant.Desktop)
            .Title("Analytics")
            .Width(Size.Px(520))
            .Height(Size.Px(420));

        return Layout.Vertical()
            | Text.H1("Wireframe Mockup")
            | Text.H2("Mobile and Website")
            | (Layout.Horizontal() | phone | site)
            | Text.H2("Tablet and Desktop")
            | (Layout.Horizontal() | tablet | desktop);
    }
}
