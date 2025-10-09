using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets.Layouts;

[App(icon: Icons.PanelLeft, path: ["Widgets", "Layouts"])]
public class SidebarEmbedTestApp : SampleBase
{
    protected override object? BuildSample()
    {
        return new SidebarLayout(
            // MainContent
            Layout.Vertical()
                | Text.H2("Sidebar Embed Test")
                | Text.H4("Features being tested:")
                | new Embed("https://github.com/microsoft/vscode")
                | new Embed("https://github.com/Ivy-Interactive/Ivy-Framework/issues/935")
                | new Embed("https://github.com/Ivy-Interactive/Ivy-Framework/pull/123")
                | new Embed("https://gist.github.com/username/gistid")
                | new Embed("https://github.com/codespaces/new?hide_repo_select=true&ref=main&repo=Ivy-Interactive%2FIvy-Examples&machine=standardLinux32gb&devcontainer_path=.devcontainer%2Fqrcoder%2Fdevcontainer.json&location=EuropeWest")
                | new Embed("https://pin.it/i/4yA1hkh77/")
                | new Embed("https://www.facebook.com/share/p/1NRYEoLAnJ/"),

            // SidebarContent
            Layout.Vertical()
                | new Embed("https://github.com/microsoft/vscode")
                | new Embed("https://github.com/Ivy-Interactive/Ivy-Framework/issues/935")
                | new Embed("https://github.com/Ivy-Interactive/Ivy-Framework/pull/123")
                | new Embed("https://gist.github.com/username/gistid")
                | new Embed("https://github.com/codespaces/new?hide_repo_select=true&ref=main&repo=Ivy-Interactive%2FIvy-Examples&machine=standardLinux32gb&devcontainer_path=.devcontainer%2Fqrcoder%2Fdevcontainer.json&location=EuropeWest")
                | new Embed("https://pin.it/i/4yA1hkh77/")
                | new Embed("https://www.facebook.com/share/p/1NRYEoLAnJ/"),

            // SidebarHeader
            Text.H4("GitHub Repositories"),

            // SidebarFooter
            Text.Small("Test Footer")
        );
    }
}
