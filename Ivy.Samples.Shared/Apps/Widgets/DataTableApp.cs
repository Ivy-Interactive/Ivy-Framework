using Ivy.Samples.Shared.Apps;
using Ivy.Shared;
using Ivy.Views.DataTables;

namespace Ivy.Samples.Apps.Widgets;

public record UserWithIcon(
    string Name,
    string Email,
    int Age,
    DateTime CreatedAt,
    DateTime LastLogin,
    Icons Status,
    bool Priority,
    Icons Activity,
    string InternalId  // Hidden column
);

[App(icon: Icons.DatabaseZap)]
public class DataTableApp : SampleBase
{
    protected override object? BuildSample()
    {
        // Create sample data with diverse icon columns and datetime fields
        var usersWithIcons = SampleData.GetUsers(1000).Select(u => new UserWithIcon(
            u.Name,
            u.Email,
            u.Age,
            u.CreatedAt,
            // LastLogin - random time in the last 30 days
            u.CreatedAt.AddDays(new Random(u.Name.GetHashCode()).Next(0, 30)),
            // Varied status icons based on age ranges
            u.Age < 25 ? Icons.Rocket :
            u.Age < 35 ? Icons.Star :
            u.Age < 45 ? Icons.ThumbsUp :
            u.Age < 55 ? Icons.CircleCheck :
            u.Age < 60 ? Icons.Clock :
            Icons.TriangleAlert,

            // Priority as boolean (checkbox)
            u.Age % 2 == 0,

            // Activity type icons
            u.IsActive ? (
                u.Age % 4 == 0 ? Icons.Coffee :
                u.Age % 4 == 1 ? Icons.Heart :
                u.Age % 4 == 2 ? Icons.Sparkles :
                Icons.Award
            ) : (
                u.Age % 3 == 0 ? Icons.Moon :
                u.Age % 3 == 1 ? Icons.CloudOff :
                Icons.Ban
            ),

            // Internal ID - this will be hidden
            $"USR-{u.Name.GetHashCode():X8}"
        )).AsQueryable();

        return usersWithIcons.ToDataTable()
            .Header(u => u.Name, "Name")
            .Header(u => u.Email, "Email")
            .Header(u => u.Age, "Age")
            .Header(u => u.CreatedAt, "Created")
            .Header(u => u.LastLogin, "Last Login")
            .Header(u => u.Status, "Status")
            .Header(u => u.Priority, "Priority")
            .Header(u => u.Activity, "Activity")
            .Header(u => u.InternalId, "Internal ID")
            // Set custom column widths
            .Width(u => u.Name, Size.Px(150))
            .Width(u => u.Email, Size.Px(250))
            .Width(u => u.Age, Size.Px(80))
            .Width(u => u.CreatedAt, Size.Px(200))
            .Width(u => u.LastLogin, Size.Px(200))
            .Width(u => u.Status, Size.Px(80))
            .Width(u => u.Priority, Size.Px(80))
            .Width(u => u.Activity, Size.Px(80))
            .Width(u => u.InternalId, Size.Px(150))
            // Set all columns to left alignment
            .Align(u => u.Name, Align.Left)
            .Align(u => u.Email, Align.Left)
            .Align(u => u.Age, Align.Left)
            .Align(u => u.CreatedAt, Align.Left)
            .Align(u => u.LastLogin, Align.Left)
            .Align(u => u.Status, Align.Left)
            .Align(u => u.Priority, Align.Left)
            .Align(u => u.Activity, Align.Left)
            .Align(u => u.InternalId, Align.Left)
            // Email is not sortable
            .Sortable(u => u.Email, false)
            // InternalId is hidden
            .Hidden([u => u.InternalId])
            // Add icons to column headers
            .Icon(u => u.Name, Icons.User)
            .Icon(u => u.Email, Icons.Mail)
            .Icon(u => u.Age, Icons.Hash)
            .Icon(u => u.CreatedAt, Icons.Calendar)
            .Icon(u => u.LastLogin, Icons.Clock)
            .Icon(u => u.Status, Icons.Activity)
            .Icon(u => u.Priority, Icons.Flag)
            .Icon(u => u.Activity, Icons.Zap)
            // Groups
            .Group(u => u.Name, "Basic Info")
            .Group(u => u.Email, "Basic Info")
            .Group(u => u.Age, "Basic Info")
            .Group(u => u.CreatedAt, "Timestamps")
            .Group(u => u.LastLogin, "Timestamps")
            .Group(u => u.Status, "Metrics")
            .Group(u => u.Priority, "Metrics")
            .Group(u => u.Activity, "Metrics");
    }
}