using Ivy.Samples.Shared.Apps;
using Ivy.Shared;
using Ivy.Views.DataTables;

namespace Ivy.Samples.Apps.Widgets;

/// <summary>
/// Comprehensive DataTable test with all column types
/// Tests the fix for issue #1273 - column type metadata preservation
/// </summary>
public record EmployeeRecord(
    int Id,
    string EmployeeCode,
    string Name,
    string Email,
    int Age,
    decimal Salary,
    double Performance,
    bool IsActive,
    bool IsManager,
    DateTime HireDate,
    DateTime LastReview,
    Icons Status,
    Icons Priority,
    Icons Department,
    string Notes,
    int? OptionalId
);

[App(icon: Icons.DatabaseZap)]
public class DataTableApp : SampleBase
{
    protected override object? BuildSample()
    {
        var random = new Random(42);
        var startDate = new DateTime(2020, 1, 1);

        var departments = new[] { Icons.Building, Icons.Code, Icons.Users, Icons.ShoppingCart, Icons.Headphones };
        var statuses = new[] { Icons.CircleCheck, Icons.Clock, Icons.TriangleAlert, Icons.X, Icons.Pause };
        var priorities = new[] { Icons.ArrowUp, Icons.ArrowRight, Icons.ArrowDown, Icons.Flag, Icons.Star };

        var firstNames = new[] { "John", "Jane", "Mike", "Sarah", "David", "Emily", "Chris", "Lisa", "Tom", "Anna" };
        var lastNames = new[] { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez" };

        var employees = Enumerable.Range(1, 1000).Select(i =>
        {
            var firstName = firstNames[random.Next(firstNames.Length)];
            var lastName = lastNames[random.Next(lastNames.Length)];
            var name = $"{firstName} {lastName}";
            var email = $"{firstName.ToLower()}.{lastName.ToLower()}{i}@company.com";
            var age = random.Next(22, 65);
            var salary = (decimal)(random.Next(40, 150) * 1000);
            var performance = Math.Round(random.NextDouble() * 5, 2);
            var isActive = random.Next(100) > 10; // 90% active
            var isManager = random.Next(100) > 85; // 15% managers
            var hireDate = startDate.AddDays(random.Next(0, 1825)); // Up to 5 years ago
            var lastReview = hireDate.AddDays(random.Next(30, 1825));
            var status = statuses[random.Next(statuses.Length)];
            var priority = priorities[random.Next(priorities.Length)];
            var department = departments[random.Next(departments.Length)];
            var notes = isActive ? "Active employee" : "Inactive";
            var optionalId = random.Next(100) > 20 ? (int?)random.Next(1000, 9999) : null;

            return new EmployeeRecord(
                Id: i,
                EmployeeCode: $"EMP{i:D4}",
                Name: name,
                Email: email,
                Age: age,
                Salary: salary,
                Performance: performance,
                IsActive: isActive,
                IsManager: isManager,
                HireDate: hireDate,
                LastReview: lastReview,
                Status: status,
                Priority: priority,
                Department: department,
                Notes: notes,
                OptionalId: optionalId
            );
        }).AsQueryable();

        return employees.ToDataTable()
            // Numeric columns
            .Header(e => e.Id, "ID")
            .Header(e => e.Age, "Age")
            .Header(e => e.Salary, "Salary")
            .Header(e => e.Performance, "Performance")
            .Header(e => e.OptionalId, "Badge #")

            // Text columns (including formatted strings)
            .Header(e => e.EmployeeCode, "Code")
            .Header(e => e.Name, "Name")
            .Header(e => e.Email, "Email")
            .Header(e => e.Notes, "Notes")

            // Boolean columns
            .Header(e => e.IsActive, "Active")
            .Header(e => e.IsManager, "Manager")

            // Date columns
            .Header(e => e.HireDate, "Hire Date")
            .Header(e => e.LastReview, "Last Review")

            // Icon columns (the main fix target)
            .Header(e => e.Status, "Status")
            .Header(e => e.Priority, "Priority")
            .Header(e => e.Department, "Dept")

            // Column widths
            .Width(e => e.Id, Size.Px(40))
            .Width(e => e.EmployeeCode, Size.Px(100))
            .Width(e => e.Name, Size.Px(120))
            .Width(e => e.Email, Size.Px(250))
            .Width(e => e.Age, Size.Px(70))
            .Width(e => e.Salary, Size.Px(120))
            .Width(e => e.Performance, Size.Px(110))
            .Width(e => e.IsActive, Size.Px(80))
            .Width(e => e.IsManager, Size.Px(90))
            .Width(e => e.HireDate, Size.Px(120))
            .Width(e => e.LastReview, Size.Px(140))
            .Width(e => e.Status, Size.Px(90))
            .Width(e => e.Priority, Size.Px(90))
            .Width(e => e.Department, Size.Px(90))
            .Width(e => e.Notes, Size.Px(150))
            .Width(e => e.OptionalId, Size.Px(100))

            // Alignments
            .Align(e => e.Id, Align.Left)
            .Align(e => e.Age, Align.Left)
            .Align(e => e.Salary, Align.Left)
            .Align(e => e.Performance, Align.Left)
            .Align(e => e.Name, Align.Left)
            .Align(e => e.Email, Align.Left)
            .Align(e => e.Notes, Align.Left)
            .Align(e => e.IsActive, Align.Left)
            .Align(e => e.IsManager, Align.Left)
            .Align(e => e.HireDate, Align.Left)
            .Align(e => e.LastReview, Align.Left)
            .Align(e => e.Status, Align.Left)
            .Align(e => e.Priority, Align.Left)
            .Align(e => e.Department, Align.Left)
            .Align(e => e.OptionalId, Align.Left)

            // Groups
            .Group(e => e.Id, "Identity")
            .Group(e => e.EmployeeCode, "Identity")
            .Group(e => e.Name, "Personal")
            .Group(e => e.Email, "Personal")
            .Group(e => e.Age, "Personal")
            .Group(e => e.Salary, "Compensation")
            .Group(e => e.Performance, "Compensation")
            .Group(e => e.IsActive, "Status")
            .Group(e => e.IsManager, "Status")
            .Group(e => e.Status, "Status")
            .Group(e => e.Priority, "Status")
            .Group(e => e.Department, "Status")
            .Group(e => e.HireDate, "Timeline")
            .Group(e => e.LastReview, "Timeline")
            .Group(e => e.Notes, "Other")
            .Group(e => e.OptionalId, "Other")

            // Sorting
            .Sortable(e => e.Email, false) // Email not sortable
            .Sortable(e => e.Notes, false) // Notes not sortable

            // Configuration
            .Config(config =>
            {
                config.FreezeColumns = 2;                    // Freeze ID and Code
                config.AllowSorting = true;
                config.AllowFiltering = true;
                config.AllowLlmFiltering = true;
                config.AllowColumnReordering = true;
                config.AllowColumnResizing = true;
                config.AllowCopySelection = true;
                config.SelectionMode = SelectionModes.Columns;
                config.ShowIndexColumn = false;
                config.ShowGroups = true;
                config.ShowVerticalBorders = false;
                config.ShowColumnTypeIcons = false;           // Show type icons
                config.BatchSize = 50;                       // Load 50 rows at a time
                config.LoadAllRows = false;                  // Use pagination
                config.ShowSearch = true;
            });
    }
}
