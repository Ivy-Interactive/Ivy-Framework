using System;
using Ivy;
using static Ivy.Layout;
using static Ivy.Text;

namespace Ivy.Tendril.Docs.Apps.Configuration.Projects;

[App(order:1, searchHints: ["project", "repo", "repository", "multi-project"])]
public class ProjectSetupApp(bool onlyBody = false) : ViewBase
{
    public ProjectSetupApp() : this(false)
    {
    }
    public override object? Build()
    {
        var appDescriptor = this.UseService<AppDescriptor>();
        var onLinkClick = this.UseLinks();
        var article = new Article().ShowToc(!onlyBody).ShowFooter(!onlyBody).Previous(appDescriptor.Previous).Next(appDescriptor.Next).DocumentSource(appDescriptor.DocumentSource).OnLinkClick(onLinkClick)
            .Headings(new List<ArticleHeading> { new ArticleHeading("project-setup", "Project Setup", 1), new ArticleHeading("adding-a-project", "Adding a Project", 2), new ArticleHeading("project-isolation", "Project Isolation", 2), new ArticleHeading("per-project-verifications", "Per-Project Verifications", 2), new ArticleHeading("context-files", "Context Files", 2), })
            | new global::Ivy.Docs.Shared.Internal.SmartSearchView()
            | new Markdown("# Project Setup").OnLinkClick(onLinkClick)
            | Lead("Tendril supports managing multiple projects simultaneously. Each project maps to a git repository and has its own verification and configuration settings.")
            | new Markdown(
                """"
                ## Adding a Project

                Add a new project entry to the `projects` array in `config.yaml`:
                """").OnLinkClick(onLinkClick)
            | new CodeBlock(
                """"
                projects:
                  - name: My App
                    repo: D:\Repos\MyApp
                    verifications:
                      - DotnetBuild
                      - DotnetFormat
                      - CheckResult
                """",Languages.Text)
            | new Markdown(
                """"
                ## Project Isolation

                When a plan is executed, Tendril creates a **git worktree** for the target project. This provides complete isolation:
                - The main branch remains untouched during execution
                - Multiple plans can execute in parallel on different worktrees
                - Failed executions don't affect your working copy
                Worktrees are created under the plan's folder at `worktrees/` and are cleaned up after the PR is merged or the plan is discarded.
                ## Per-Project Verifications

                Each project can have its own set of verifications. After an agent completes execution, Tendril runs the configured verifications in order. If any verification fails, the plan is flagged for review.
                ## Context Files

                Projects can include context files (like `CLAUDE.md` or `DEVELOPER.md`) that are automatically included in the agent's prompt. These files provide project-specific instructions and conventions that guide the agent's behavior.
                """").OnLinkClick(onLinkClick)
            ;
        return article;
    }
}
