namespace Ivy.Tendril.Apps;

[App(isVisible: false)]
public class WallpaperApp : ViewBase
{
    public override object Build()
    {
        var navigator = UseNavigation();

        return Layout.Center()
               | (Layout.Vertical().Gap(2).AlignContent(Align.Center)
                  | new Image("/tendril/assets/Tendril.svg").Width(Size.Units(30)).Height(Size.Auto())
                  | Text.H2("Welcome to Ivy Tendril")
                  | Text.Muted("Manage your plans, track jobs, and review pull requests.")
                  | new Button("Create your first Plan", () => navigator.Navigate<PlansApp>())
                      .Variant(ButtonVariant.Primary)
               );
    }
}
