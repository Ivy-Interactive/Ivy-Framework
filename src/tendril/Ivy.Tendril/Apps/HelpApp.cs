namespace Ivy.Tendril.Apps;

[App(title: "Help", icon: Icons.CircleQuestionMark, group: new[] { "Tools" }, order: MenuOrder.Help)]
public class HelpApp : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();

        return Layout.TopCenter()
               | (Layout.Vertical().Margin(0, 20).Width(150).Gap(3)
                  | Text.H1("Help")
                  | Text.Muted("View Tendril documentation and guides at https://tendril.ivy.app.")
                  | new Button("Open Documentation")
                      .Primary()
                      .Large()
                      .Icon(Icons.ExternalLink, Align.Right)
                      .OnClick(() => client.OpenUrl("https://tendril.ivy.app"))
               );
    }
}