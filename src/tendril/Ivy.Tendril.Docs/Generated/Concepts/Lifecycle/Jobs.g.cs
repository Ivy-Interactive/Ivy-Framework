using System;
using Ivy;
using static Ivy.Layout;
using static Ivy.Text;

namespace Ivy.Tendril.Docs.Apps.Concepts.Lifecycle;

[App(order:1, searchHints: ["jobs", "execution", "running", "cost", "tokens", "monitoring"])]
public class JobsApp(bool onlyBody = false) : ViewBase
{
    public JobsApp() : this(false)
    {
    }
    public override object? Build()
    {
        var appDescriptor = this.UseService<AppDescriptor>();
        var onLinkClick = this.UseLinks();
        var article = new Article().ShowToc(!onlyBody).ShowFooter(!onlyBody).Previous(appDescriptor.Previous).Next(appDescriptor.Next).DocumentSource(appDescriptor.DocumentSource).OnLinkClick(onLinkClick)
            .Headings(new List<ArticleHeading> { new ArticleHeading("jobs", "Jobs", 1), new ArticleHeading("job-tracking", "Job Tracking", 2), new ArticleHeading("job-concurrency", "Job Concurrency", 2), new ArticleHeading("cost-tracking", "Cost Tracking", 2), })
            | new global::Ivy.Docs.Shared.Internal.SmartSearchView()
            | new Markdown("# Jobs").OnLinkClick(onLinkClick)
            | Lead("Jobs are the execution units in Tendril. Every time a promptware runs, it creates a job that tracks progress, cost, and output.")
            | new Markdown(
                """"
                ## Job Tracking

                Each job captures:
                - **Status** — Pending, Running, Completed, Failed, or Timed Out
                - **Type** — Which promptware is running (MakePlan, ExecutePlan, etc.)
                - **Plan ID** — The plan this job belongs to
                - **Cost** — Token usage and estimated cost
                - **Duration** — How long the job has been running
                - **Output** — Live status messages from the agent
                ## Job Concurrency

                Tendril supports configurable concurrent job limits. Multiple plans can be executed in parallel, each in its own isolated git worktree. The concurrency limit is set in `config.yaml`.
                ## Cost Tracking

                Every job logs its token usage to a `costs.csv` file in the plan folder. The dashboard aggregates these costs across all plans, giving you visibility into your AI spending by project, time period, and promptware type.
                """").OnLinkClick(onLinkClick)
            ;
        return article;
    }
}
