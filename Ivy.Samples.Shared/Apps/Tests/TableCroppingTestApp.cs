using Ivy.Hooks;
using Ivy.Shared;
using Ivy.Views.Blades;
using Ivy.Views.Builders;
using Ivy.Views.Tables;

namespace Ivy.Samples.Shared.Apps.Tests;

[App(icon: Icons.Table, path: ["Tests"], isVisible: false, searchHints: ["table", "cropping", "layout", "blade", "truncation"])]
public class TableCroppingTestApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new TableTestBlade(), "Search");
    }
}

public class TableTestBlade : ViewBase
{
    public override object? Build()
    {
        var projects = new[]
        {
            new ProjectRecord("Landskrona BoIS", "155", new DateTime(2024, 1, 15), new DateTime(2024, 12, 31), "Active", 3),
            new ProjectRecord("IFK Göteborg", "140", new DateTime(2024, 2, 1), new DateTime(2024, 11, 30), "Active", 2),
            new ProjectRecord("Allsvenskan Championship", "120", new DateTime(2024, 3, 1), new DateTime(2024, 10, 31), "Active", 2),
            new ProjectRecord("GAIS Football Club", "3000", new DateTime(2024, 1, 1), new DateTime(2024, 12, 31), "Active", 2),
            new ProjectRecord("Football Training Camp", "100", new DateTime(2024, 4, 1), new DateTime(2024, 8, 31), "Active", 2),
            new ProjectRecord("Youth Development Program", "85", new DateTime(2024, 2, 15), new DateTime(2024, 11, 15), "Active", 1),
            new ProjectRecord("Community Sports Initiative", "200", new DateTime(2024, 3, 15), new DateTime(2024, 9, 15), "Active", 2),
            new ProjectRecord("Professional League Match", "5000", new DateTime(2024, 5, 1), new DateTime(2024, 10, 31), "Active", 3),
            new ProjectRecord("Stadium Renovation Project", "2500", new DateTime(2024, 1, 1), new DateTime(2024, 6, 30), "Active", 3),
            new ProjectRecord("Fan Engagement Program", "150", new DateTime(2024, 2, 1), new DateTime(2024, 12, 31), "Active", 1)
        };

        var projectsCard = new Card(
            content: projects.ToTable()
                .Order(e => e.Name, e => e.Description, e => e.StartDate, e => e.EndDate, e => e.Status, e => e.Priority)
                .Header(e => e.Name, "Name")
                .Header(e => e.Description, "Description")
                .Header(e => e.StartDate, "Start Date")
                .Header(e => e.EndDate, "End Date")
                .Header(e => e.Status, "Status")
                .Header(e => e.Priority, "Priority"),
            footer:
                Layout.Horizontal().Gap(2).Width(Size.Full())
                | new Button("Add Project").Icon(Icons.Plus)
                | Icons.RefreshCw.ToButton()
            ).Title("Projects Table - Testing Cropping Issue");

        return Layout.Vertical().Gap(4) | projectsCard;
    }
}

public record ProjectRecord(string Name, string Description, DateTime StartDate, DateTime EndDate, string Status, int Priority);
