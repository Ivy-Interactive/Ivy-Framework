using System;
using Ivy;
using static Ivy.Layout;
using static Ivy.Text;

namespace Ivy.Tendril.Docs.Apps.Configuration.Setup;

[App(order:1, searchHints: ["config", "yaml", "configuration", "settings", "projects"])]
public class ConfigYamlApp(bool onlyBody = false) : ViewBase
{
    public ConfigYamlApp() : this(false)
    {
    }
    public override object? Build()
    {
        var appDescriptor = this.UseService<AppDescriptor>();
        var onLinkClick = this.UseLinks();
        var article = new Article().ShowToc(!onlyBody).ShowFooter(!onlyBody).Previous(appDescriptor.Previous).Next(appDescriptor.Next).DocumentSource(appDescriptor.DocumentSource).OnLinkClick(onLinkClick)
            .Headings(new List<ArticleHeading> { new ArticleHeading("configyaml", "config.yaml", 1), new ArticleHeading("location", "Location", 2), new ArticleHeading("structure", "Structure", 2), new ArticleHeading("key-fields", "Key Fields", 2), new ArticleHeading("agentcommand", "agentCommand", 3), new ArticleHeading("maxconcurrentjobs", "maxConcurrentJobs", 3), new ArticleHeading("projects", "projects", 3), new ArticleHeading("coworkers", "coworkers", 3), new ArticleHeading("verifications", "Verifications", 2), })
            | new global::Ivy.Docs.Shared.Internal.SmartSearchView()
            | new Markdown("# config.yaml").OnLinkClick(onLinkClick)
            | Lead("The `config.yaml` file is the primary configuration file for Tendril. It defines your projects, agent settings, and system preferences.")
            | new Markdown(
                """"
                ## Location

                Tendril looks for `config.yaml` in the `TENDRIL_HOME` directory. If it doesn't exist, the onboarding wizard will help you create one.
                ## Structure
                """").OnLinkClick(onLinkClick)
            | new CodeBlock(
                """"
                agentCommand: claude
                maxConcurrentJobs: 3

                projects:
                  - name: MyProject
                    repo: D:\Repos\MyProject
                    verifications:
                      - DotnetBuild
                      - DotnetFormat
                      - DotnetTest
                      - CheckResult
                    meta:
                      slackEmoji: ":rocket:"
                      color: "#3B82F6"

                coworkers:
                  - github: username
                    name: Display Name
                """",Languages.Text)
            | new Markdown(
                """"
                ## Key Fields

                ### agentCommand

                The CLI command used to invoke Claude. Typically `claude`.
                ### maxConcurrentJobs

                Maximum number of promptware jobs that can run simultaneously.
                ### projects

                Array of project configurations. Each project defines:
                | Field | Description |
                |-------|-------------|
                | `name` | Display name for the project |
                | `repo` | Absolute path to the repository |
                | `verifications` | List of verification steps to run after execution |
                | `meta.slackEmoji` | Emoji used in Slack notifications |
                | `meta.color` | Color used in the dashboard and badges |
                ### coworkers

                List of team members for PR assignment and collaboration features.
                ## Verifications

                Available verification types:
                | Type | Description |
                |------|-------------|
                | `DotnetBuild` | Runs `dotnet build` on the project |
                | `DotnetFormat` | Checks code formatting with `dotnet format` |
                | `DotnetTest` | Runs the project's test suite |
                | `CheckResult` | Validates the agent's execution result |
                | `IvyFramework` | Ivy-specific checks (samples, docs) |
                """").OnLinkClick(onLinkClick)
            ;
        return article;
    }
}
