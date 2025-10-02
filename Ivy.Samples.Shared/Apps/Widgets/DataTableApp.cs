using Ivy.Samples.Shared.Apps;
using Ivy.Shared;
using Ivy.Views.DataTables;

namespace Ivy.Samples.Apps.Widgets;

public record UserWithIcon(string Name, string Email, int Age, Icons Status, Icons Priority, Icons Activity);

[App(icon: Icons.DatabaseZap)]
public class DataTableApp : SampleBase
{
    protected override object? BuildSample()
    {
        // Create sample data with diverse icon columns
        var usersWithIcons = SampleData.GetUsers(1000).Select(u => new UserWithIcon(
            u.Name,
            u.Email,
            u.Age,
            // Varied status icons based on age ranges
            u.Age < 25 ? Icons.Rocket :
            u.Age < 35 ? Icons.Star :
            u.Age < 45 ? Icons.ThumbsUp :
            u.Age < 55 ? Icons.CircleCheck :
            u.Age < 60 ? Icons.Clock :
            Icons.TriangleAlert,

            // Priority icons with more variety
            u.Age % 5 == 0 ? Icons.Flame :
            u.Age % 5 == 1 ? Icons.Zap :
            u.Age % 5 == 2 ? Icons.TrendingUp :
            u.Age % 5 == 3 ? Icons.Target :
            Icons.Flag,

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
            )
        )).AsQueryable();

        return usersWithIcons.ToDataTable()
            .Header(u => u.Name, "Name")
            .Header(u => u.Email, "Email")
            .Header(u => u.Status, "Status")
            .Header(u => u.Priority, "Priority")
            .Header(u => u.Activity, "Activity");
    }
}