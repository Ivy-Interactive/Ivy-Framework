namespace Ivy.Tendril.Apps;

[App(title: "Help", icon: Icons.CircleQuestionMark, group: ["Apps"], order: MenuOrder.Help)]
public class HelpApp : ViewBase
{
    public override object Build()
    {
        var client = UseService<IClientProvider>();

        return Layout.TopCenter()
               | (Layout.Vertical().Margin(0, 20).Width(150).Gap(3)
                  | Text.H1("Help")
                  | Text.Muted("View documentation at https://tendril.ivy.app or join us on Discord for help.")
                  | (Layout.Horizontal()
                     | new Button("Open Documentation")
                         .Secondary()
                         .Icon(Icons.ExternalLink, Align.Right)
                         .OnClick(() => client.OpenUrl("https://tendril.ivy.app"))
                     | new Button("Join Discord")
                         .Secondary()
                         .Icon(Icons.Discord, Align.Right)
                         .OnClick(() => client.OpenUrl("https://discord.gg/FHgxkDga3y")))
                  | Text.Muted("View documentation at https://tendril.ivy.app or join us on Discord for help.")
                  | new Button("Submit Issue")
                      .Secondary()
                      .Icon(Icons.Bug, Align.Right)
                      .OnClick(() =>
                          client.OpenUrl(
                              "https://github.com/Ivy-Interactive/Ivy-Framework/issues/new?title=%28tendril%29%20"))
               );
    }
}

