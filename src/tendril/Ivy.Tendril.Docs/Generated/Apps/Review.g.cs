using System;
using Ivy;
using static Ivy.Layout;
using static Ivy.Text;

namespace Ivy.Tendril.Docs.Apps.Apps;

[App(order:2, icon:Icons.Eye, searchHints: ["review", "diff", "approve", "reject", "verification"])]
public class ReviewApp(bool onlyBody = false) : ViewBase
{
    public ReviewApp() : this(false)
    {
    }
    public override object? Build()
    {
        var appDescriptor = this.UseService<AppDescriptor>();
        var onLinkClick = this.UseLinks();
        var article = new Article().ShowToc(!onlyBody).ShowFooter(!onlyBody).Previous(appDescriptor.Previous).Next(appDescriptor.Next).DocumentSource(appDescriptor.DocumentSource).OnLinkClick(onLinkClick)
            .Headings(new List<ArticleHeading> { new ArticleHeading("review", "Review", 1), new ArticleHeading("review-workflow", "Review Workflow", 2), new ArticleHeading("actions", "Actions", 2), new ArticleHeading("verification-results", "Verification Results", 2), })
            | new global::Ivy.Docs.Shared.Internal.SmartSearchView()
            | new Markdown("# Review").OnLinkClick(onLinkClick)
            | Lead("The Review app is where you inspect completed plans, review diffs, check verification results, and approve or send plans back for revision.")
            | new Markdown(
                """"
                ## Review Workflow

                When a plan completes execution, it moves to the **Review** state. The Review app shows:
                1. **Summary** — The plan's description, problem statement, and proposed solution
                2. **Commits** — List of commits made during execution with diff views
                3. **Verifications** — Results of build, format, and test checks
                4. **Artifacts** — Screenshots and other outputs from the execution
                5. **Recommendations** — AI-generated suggestions for improvements
                ## Actions

                From the Review app, you can:
                | Action | Description |
                |--------|-------------|
                | **Approve** | Mark the plan as ready for PR creation |
                | **Suggest Changes** | Send the plan back to Draft with feedback |
                | **Discard** | Move the plan to Trash |
                | **Make PR** | Create a GitHub pull request directly |
                | **Open in VS Code** | Open the worktree in your editor |
                | **Open in Terminal** | Open a terminal at the worktree path |
                ## Verification Results

                Each verification shows a pass/fail/skip status. Click on a verification to see detailed output. Failed verifications are highlighted and should be addressed before creating a PR.
                """").OnLinkClick(onLinkClick)
            ;
        return article;
    }
}
