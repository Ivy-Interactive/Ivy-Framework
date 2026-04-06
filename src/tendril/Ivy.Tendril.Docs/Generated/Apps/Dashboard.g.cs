using System;
using Ivy;
using static Ivy.Layout;
using static Ivy.Text;

namespace Ivy.Tendril.Docs.Apps.Apps;

[App(order:1, icon:Icons.ChartBar, searchHints: ["dashboard", "statistics", "overview", "charts", "cost"])]
public class DashboardApp(bool onlyBody = false) : ViewBase
{
    public DashboardApp() : this(false)
    {
    }
    public override object? Build()
    {
        var appDescriptor = this.UseService<AppDescriptor>();
        var onLinkClick = this.UseLinks();
        var article = new Article().ShowToc(!onlyBody).ShowFooter(!onlyBody).Previous(appDescriptor.Previous).Next(appDescriptor.Next).DocumentSource(appDescriptor.DocumentSource).OnLinkClick(onLinkClick)
            .Headings(new List<ArticleHeading> { new ArticleHeading("dashboard", "Dashboard", 1), new ArticleHeading("overview", "Overview", 2), new ArticleHeading("filtering", "Filtering", 2), new ArticleHeading("cost-tracking", "Cost Tracking", 2), })
            | new global::Ivy.Docs.Shared.Internal.SmartSearchView()
            | new Markdown("# Dashboard").OnLinkClick(onLinkClick)
            | Lead("The Dashboard provides a high-level overview of your Tendril activity, including plan counts, cost tracking, and token usage charts.")
            | new Markdown(
                """"
                ## Overview

                The Dashboard is the default landing page when you open Tendril. It shows:
                - **Plan counts by state** — A stacked progress bar showing how many plans are in each lifecycle state
                - **Cost and token charts** — Bar charts showing spending and token usage over time
                - **Activity table** — Recent plans with their status, project, cost, and timestamps
                ## Filtering

                You can filter the dashboard by:
                - **Project** — Click on a project segment in the stacked progress bar
                - **Time period** — Select a date range to focus on specific activity
                ## Cost Tracking

                The dashboard aggregates cost data from all plan `costs.csv` files. Costs are broken down by:
                - Project (color-coded)
                - Time period (hourly/daily)
                - Promptware type
                """").OnLinkClick(onLinkClick)
            ;
        return article;
    }
}
