using System;
using Ivy;
using static Ivy.Layout;
using static Ivy.Text;

namespace Ivy.Tendril.Docs.Apps.GettingStarted.Overview;

[App(order:1, searchHints: ["overview", "what-is", "tendril", "agent", "orchestration"])]
public class IntroductionApp(bool onlyBody = false) : ViewBase
{
    public IntroductionApp() : this(false)
    {
    }
    public override object? Build()
    {
        var appDescriptor = this.UseService<AppDescriptor>();
        var onLinkClick = this.UseLinks();
        var article = new Article().ShowToc(!onlyBody).ShowFooter(!onlyBody).Previous(appDescriptor.Previous).Next(appDescriptor.Next).DocumentSource(appDescriptor.DocumentSource).OnLinkClick(onLinkClick)
            .Headings(new List<ArticleHeading> { new ArticleHeading("welcome-to-tendril", "Welcome to Tendril", 1), new ArticleHeading("key-features", "Key Features", 2), new ArticleHeading("how-it-works", "How It Works", 2), })
            | new global::Ivy.Docs.Shared.Internal.SmartSearchView()
            | new Markdown("# Welcome to Tendril").OnLinkClick(onLinkClick)
            | Lead("Tendril is a TUI-based agent orchestration platform for managing AI-driven development plans. Built on the Ivy Framework, it orchestrates Claude-based agents through a structured lifecycle — from plan creation and expansion to execution, verification, and PR generation.")
            | new Markdown(
                """"
                Tendril gives you full visibility into your AI-assisted development workflow. It tracks jobs, costs, tokens, and verification results, and presents everything in a terminal UI that lets you stay in control.
                ## Key Features

                - **Plan lifecycle management** — Draft, Execute, Review, and PR stages with state tracking
                - **Multi-project support** — Configure multiple repos with per-project verifications
                - **Job monitoring** — Live cost and token tracking for running agents
                - **Claude agent orchestration** — Promptwares for each stage (MakePlan, ExecutePlan, ExpandPlan, MakePr, etc.)
                - **Dashboard** — Activity statistics and plan counts at a glance
                - **GitHub PR integration** — Automated pull request creation from completed plans
                - **Plan review workflow** — Review diffs, run sample apps, approve or send back for revision
                ## How It Works

                Tendril manages plans through a structured lifecycle:
                1. **MakePlan** — An agent drafts a plan from a description or issue, producing a structured revision with problem, solution, tests, and verification steps.
                2. **ExpandPlan** — Optionally expands a plan with more detail, or splits large plans into smaller ones.
                3. **ExecutePlan** — An agent creates a git worktree, implements the plan, runs verifications (build, format, tests), and commits the result.
                4. **Review** — You review the diff, run sample apps, and approve or send back with comments.
                5. **MakePr** — An agent creates a GitHub pull request from the worktree branch.
                Each stage is powered by a **promptware** — a structured prompt with tools and memory that runs via the Claude CLI. Jobs are tracked with live status, cost, and token metrics.
                """").OnLinkClick(onLinkClick)
            ;
        return article;
    }
}
