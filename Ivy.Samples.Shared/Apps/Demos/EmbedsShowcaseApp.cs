using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Demos;

[App(icon: Icons.Link, path: ["Demos"])]
public class EmbedsShowcaseApp : ViewBase
{
    public override object? Build()
    {
        // Sidebar content with embeds
        var sidebarContent = Layout.Vertical().Gap(4)
            | Text.H4("Quick Links")
            | Text.Muted("GitHub Repository")
            | new Embed("https://github.ck")
            | new Embed("https://github.com/codespaces/new?hide_repo_select=true&ref=main&repo=Ivy-Interactive%2FIvy-Examples&machine=standardLinux32gb&devcontainer_path=.devcontainer%2Fqrcoder%2Fdevcontainer.json&location=EuropeWest")
            | new Card("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam nec purus nec nunc")
            | new Embed("https://github.com/codespaces/new?hide_repo_select=true&ref=main&repo=Ivy-Interactive%2FIvy-Examples&machine=standardLinux32gb&devcontainer_path=.devcontainer%2Fqrcoder%2Fdevcontainer.json&location=EuropeWest")
            | new Card("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam nec purus nec nunc")
            | new Embed("https://github.com/codespaces/new?hide_repo_select=true&ref=main&repo=Ivy-Interactive%2FIvy-Examples&machine=standardLinux32gb&devcontainer_path=.devcontainer%2Fqrcoder%2Fdevcontainer.json&location=EuropeWest")
            | new Card("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam nec purus nec nunc")
            | new Embed("https://github.com/codespaces/new?hide_repo_select=true&ref=main&repo=Ivy-Interactive%2FIvy-Examples&machine=standardLinux32gb&devcontainer_path=.devcontainer%2Fqrcoder%2Fdevcontainer.json&location=EuropeWest")
            | new Card("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam nec purus nec nunc")
            | new Embed("https://github.com/codespaces/new?hide_repo_select=true&ref=main&repo=Ivy-Interactive%2FIvy-Examples&machine=standardLinux32gb&devcontainer_path=.devcontainer%2Fqrcoder%2Fdevcontainer.json&location=EuropeWest")
            | new Card("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam nec purus nec nunc")
            | new Embed("https://github.com/codespaces/new?hide_repo_select=true&ref=main&repo=Ivy-Interactive%2FIvy-Examples&machine=standardLinux32gb&devcontainer_path=.devcontainer%2Fqrcoder%2Fdevcontainer.json&location=EuropeWest")
            | new Card("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam nec purus nec nunc")
            | new Embed("https://github.com/codespaces/new?hide_repo_select=true&ref=main&repo=Ivy-Interactive%2FIvy-Examples&machine=standardLinux32gb&devcontainer_path=.devcontainer%2Fqrcoder%2Fdevcontainer.json&location=EuropeWest")
            | new Card("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam nec purus nec nunc")
            | new Embed("https://github.com/codespaces/new?hide_repo_select=true&ref=main&repo=Ivy-Interactive%2FIvy-Examples&machine=standardLinux32gb&devcontainer_path=.devcontainer%2Fqrcoder%2Fdevcontainer.json&location=EuropeWest")
            | new Card("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam nec purus nec nunc")
            | new Embed("https://github.com/codespaces/new?hide_repo_select=true&ref=main&repo=Ivy-Interactive%2FIvy-Examples&machine=standardLinux32gb&devcontainer_path=.devcontainer%2Fqrcoder%2Fdevcontainer.json&location=EuropeWest")
            | new Card("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam nec purus nec nunc")
            | new Embed("https://github.com/codespaces/new?hide_repo_select=true&ref=main&repo=Ivy-Interactive%2FIvy-Examples&machine=standardLinux32gb&devcontainer_path=.devcontainer%2Fqrcoder%2Fdevcontainer.json&location=EuropeWest")
            | new Card("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam nec purus nec nunc")
            | new Embed("https://github.com/codespaces/new?hide_repo_select=true&ref=main&repo=Ivy-Interactive%2FIvy-Examples&machine=standardLinux32gb&devcontainer_path=.devcontainer%2Fqrcoder%2Fdevcontainer.json&location=EuropeWest")
            | new Card("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam nec purus nec nunc")
        ;

        // Main content with more embeds and information
        var mainContent = Layout.Vertical().Gap(6)
            | Text.H1("Embeds Showcase")
            | Text.P("This demo shows how embeds work in a sidebar layout. The sidebar contains quick links with embeds, while the main content shows various embed types.")

            // Social Media Section
            | Text.H2("Social Media Embeds")
            | (Layout.Vertical().Gap(4)
                    | new Embed("https://gitvy-Framework")
                    | new Embed("https://github.com/Ivy-Interactive/Ivy-Framework/issues/935")
            )
        ;

        var sidebarHeader = Layout.Vertical().Gap(2)
            | Text.H3("Resources")
            | Text.Small("External links and media");

        return new SidebarLayout(
            mainContent,
            sidebarContent,
            sidebarHeader,
            null
        ).MainAppSidebar(false);
    }
}

