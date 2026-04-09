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
                  | Text.Muted("View documentation at https://tendril.ivy.app or submit an issue on GitHub.")
                  | new Button("Open Documentation")
                      .Primary()
                      .Large()
                      .Icon(Icons.ExternalLink, Align.Right)
                      .OnClick(() => client.OpenUrl("https://tendril.ivy.app"))
                  | new Button("Submit Issue")
                      .Outline()
                      .Large()
                      .Icon(Icons.Bug, Align.Right)
                      .OnClick(() => client.OpenUrl("https://github.com/Ivy-Interactive/Ivy-Framework/issues/new?title=%28tendril%29%20"))
                  | Text.Muted("Join our Discord to talk directly with the team.")
                  | new Button("Join Discord")
                      .Large()
                      .Icon(Icons.Discord, Align.Right)
                      .OnClick(() => client.OpenUrl("https://discord.gg/FHgxkDga3y"))
               );
    }
}
