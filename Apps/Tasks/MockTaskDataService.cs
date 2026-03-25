namespace Ivy.Tendril.Apps.Tasks;

public class MockTaskDataService
{
    public List<TaskItem> GetTasks()
    {
        var now = DateTime.UtcNow;
        return new List<TaskItem>
        {
            new()
            {
                Id = "task-001",
                Type = "ExecutePlan",
                PlanFile = "920-IvyFramework-CRITICAL-ButtonFix.md",
                Queue = "IvyFramework",
                Status = "Running",
                StartedAt = now.AddMinutes(-12)
            },
            new()
            {
                Id = "task-002",
                Type = "MakePlan",
                PlanFile = "921-IvyAgent-NICETOHAVE-ToolImprovement.md",
                Queue = "IvyAgent",
                Status = "Running",
                StartedAt = now.AddMinutes(-3)
            },
            new()
            {
                Id = "task-003",
                Type = "ExecutePlan",
                PlanFile = "922-IvyConsole-NITPICK-Formatting.md",
                Queue = "IvyConsole",
                Status = "Pending"
            },
            new()
            {
                Id = "task-004",
                Type = "UpdatePlan",
                PlanFile = "923-IvyFramework-NICETOHAVE-Refactor.md",
                Queue = "IvyFramework",
                Status = "Pending"
            },
            new()
            {
                Id = "task-005",
                Type = "ExecutePlan",
                PlanFile = "919-IvyConsole-NITPICK-LogCleanup.md",
                Queue = "IvyConsole",
                Status = "Completed",
                StartedAt = now.AddHours(-1).AddMinutes(-12),
                CompletedAt = now.AddHours(-1),
                DurationSeconds = 720
            },
            new()
            {
                Id = "task-006",
                Type = "MakePlan",
                PlanFile = "918-IvyAgent-CRITICAL-AuthBug.md",
                Queue = "IvyAgent",
                Status = "Failed",
                StartedAt = now.AddHours(-2),
                CompletedAt = now.AddHours(-1).AddMinutes(-45),
                DurationSeconds = 900
            },
            new()
            {
                Id = "task-007",
                Type = "ExecutePlan",
                PlanFile = "917-IvyFramework-NICETOHAVE-DarkMode.md",
                Queue = "IvyFramework",
                Status = "Completed",
                StartedAt = now.AddHours(-3),
                CompletedAt = now.AddHours(-2).AddMinutes(-30),
                DurationSeconds = 1800
            }
        };
    }
}
