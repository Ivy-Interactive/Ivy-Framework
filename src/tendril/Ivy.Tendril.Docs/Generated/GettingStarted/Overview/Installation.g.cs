using System;
using Ivy;
using static Ivy.Layout;
using static Ivy.Text;

namespace Ivy.Tendril.Docs.Apps.GettingStarted.Overview;

[App(order:2, icon:Icons.Download, searchHints: ["install", "setup", "prerequisites", "getting-started"])]
public class InstallationApp(bool onlyBody = false) : ViewBase
{
    public InstallationApp() : this(false)
    {
    }
    public override object? Build()
    {
        var appDescriptor = this.UseService<AppDescriptor>();
        var onLinkClick = this.UseLinks();
        var article = new Article().ShowToc(!onlyBody).ShowFooter(!onlyBody).Previous(appDescriptor.Previous).Next(appDescriptor.Next).DocumentSource(appDescriptor.DocumentSource).OnLinkClick(onLinkClick)
            .Headings(new List<ArticleHeading> { new ArticleHeading("installation", "Installation", 1), new ArticleHeading("prerequisites", "Prerequisites", 2), new ArticleHeading("for-running-tendril", "For Running Tendril", 3), new ArticleHeading("for-development", "For Development", 3), new ArticleHeading("setup", "Setup", 2), new ArticleHeading("1-clone-the-repo", "1. Clone the repo", 3), new ArticleHeading("2-configure-configyaml", "2. Configure config.yaml", 3), new ArticleHeading("3-set-tendril_home-environment-variable", "3. Set TENDRIL_HOME environment variable", 3), new ArticleHeading("4-run", "4. Run", 3), })
            | new global::Ivy.Docs.Shared.Internal.SmartSearchView()
            | new Markdown("# Installation").OnLinkClick(onLinkClick)
            | Lead("Get Tendril up and running on your machine.")
            | new Markdown(
                """"
                ## Prerequisites

                ### For Running Tendril

                - [Claude CLI](https://docs.anthropic.com/en/docs/claude-code) (`claude`)
                - [GitHub CLI](https://cli.github.com/) (`gh`)
                - PowerShell
                - Git
                ### For Development

                - [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
                ## Setup

                ### 1. Clone the repo
                """").OnLinkClick(onLinkClick)
            | new CodeBlock(
                """"
                git clone https://github.com/Ivy-Interactive/Ivy-Framework.git
                cd Ivy-Framework/src/tendril/Ivy.Tendril
                """",Languages.Text)
            | new Markdown(
                """"
                ### 2. Configure config.yaml

                Copy the example config and edit it:
                """").OnLinkClick(onLinkClick)
            | new CodeBlock("cp example.config.yaml config.yaml",Languages.Text)
            | new Markdown(
                """"
                Key fields:
                - `projects` — List of projects with their repo paths, verifications, and context
                - `agentCommand` — The Claude CLI command used to run agents
                ### 3. Set TENDRIL_HOME environment variable

                Point `TENDRIL_HOME` to your Tendril data directory:
                """").OnLinkClick(onLinkClick)
            | new CodeBlock(
                """"
                export TENDRIL_HOME=~/.tendril
                mkdir -p "$TENDRIL_HOME"
                """",Languages.Text)
            | new Markdown(
                """"
                Tendril will populate this with `Plans/`, `Inbox/`, `Trash/`, and `config.yaml` at runtime. If `TENDRIL_HOME` is not set, Tendril will launch the onboarding wizard.
                ### 4. Run
                """").OnLinkClick(onLinkClick)
            | new CodeBlock("dotnet run",Languages.Text)
            ;
        return article;
    }
}
