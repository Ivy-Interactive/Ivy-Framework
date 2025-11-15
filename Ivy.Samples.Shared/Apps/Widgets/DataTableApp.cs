using Ivy.Samples.Shared.Apps;
using Ivy.Shared;
using Ivy.Views.DataTables;

namespace Ivy.Samples.Apps.Widgets;

/// <summary>
/// Comprehensive DataTable test with all column types
/// Tests the fix for issue #1273 - column type metadata preservation
/// Tests the fix for issue #1311 - table width and height setting
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
    int? OptionalId,
    string[] Skills,
    string? WidgetLink,
    string? ProfileLink
);

[App(icon: Icons.DatabaseZap)]
public class DataTableApp : SampleBase
{
    protected override object? BuildSample()
    {
        // Manage Sheet open/close state at app level
        var isOpen = this.UseState(false);

        // Store selected cell information
        var selectedCell = this.UseState<CellClickEventArgs?>(() => null);

        // Store selected row for row actions
        var selectedRowIndex = this.UseState<int?>(() => null);
        var actionName = this.UseState<string?>(() => null);

        var allSkills = new[] { "C#", "JavaScript", "Python", "SQL", "React", "Leadership", "Communication", "Problem Solving", "Team Player", "Agile" };

        // Create the employee data once at app level (like Kanban caches its tasks)
        var employees = this.UseState(() =>
        {
            var random = new Random(42);
            var startDate = new DateTime(2020, 1, 1);

            var departments = new[] { Icons.Building, Icons.Code, Icons.Users, Icons.ShoppingCart, Icons.Headphones };
            var statuses = new[] { Icons.CircleCheck, Icons.Clock, Icons.TriangleAlert, Icons.X, Icons.Pause };
            var priorities = new[] { Icons.ArrowUp, Icons.ArrowRight, Icons.ArrowDown, Icons.Flag, Icons.Star };

            var firstNames = new[] { "John", "Jane", "Mike", "Sarah", "David", "Emily", "Chris", "Lisa", "Tom", "Anna" };
            var lastNames = new[] { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez" };

            return Enumerable.Range(1, 1000).Select(i =>
            {
                var firstName = firstNames[random.Next(firstNames.Length)];
                var lastName = lastNames[random.Next(lastNames.Length)];
                var name = $"{firstName} {lastName}";
                var email = $"employee{i}@company.com";
                var age = random.Next(22, 65);
                var salary = (decimal)(random.Next(30000, 150000) / 1000 * 1000);
                var performance = Math.Round(random.NextDouble() * 5, 2);
                var isActive = random.NextDouble() > 0.2;
                var isManager = random.NextDouble() > 0.8;
                var hireDate = startDate.AddDays(random.Next(0, 1826));
                var lastReview = DateTime.Now.AddDays(-random.Next(0, 365));
                var status = statuses[random.Next(statuses.Length)];
                var priority = priorities[random.Next(priorities.Length)];
                var department = departments[random.Next(departments.Length)];
                var notes = $"Employee notes for {i}";
                var optionalId = random.NextDouble() > 0.3 ? (int?)random.Next(1, 1000) : null;

                // Generate 2-5 random skills for each employee
                var skillCount = random.Next(2, 6);
                var skills = Enumerable.Range(0, skillCount)
                    .Select(_ => allSkills[random.Next(allSkills.Length)])
                    .Distinct()
                    .ToArray();

                // Generate link URLs
                var widgetLink = "/widgets/charts/area-chart-app"; // Internal widget link - relative URL works on any domain
                var profileLink = $"https://linkedin.com/in/{firstName.ToLower()}{lastName.ToLower()}{i}"; // External LinkedIn profile

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
                    OptionalId: optionalId,
                    Skills: skills,
                    WidgetLink: widgetLink,
                    ProfileLink: profileLink
                );
            }).ToList();
        });

        // The DataTable builder will be recreated each time, but use the cached employee data
        var dataTable = employees.Value.AsQueryable().ToDataTable()
            // Table dimensions (fix for issue #1311)
            .Width(Size.Full()) // Table width set to 120 units (30rem)
            .Height(Size.Full()) // Table height set to 120 units (30rem)

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

            // Labels column (issue #1146)
            .Header(e => e.Skills, "Skills")

            // Link columns
            .Header(e => e.WidgetLink, "Widgets")
            .Header(e => e.ProfileLink, "Profiles")

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
            .Width(e => e.Skills, Size.Px(300))
            .Width(e => e.WidgetLink, Size.Px(200))
            .Width(e => e.ProfileLink, Size.Px(250))

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
            .Align(e => e.Skills, Align.Left)
            .Align(e => e.WidgetLink, Align.Left)
            .Align(e => e.ProfileLink, Align.Left)

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
            .Group(e => e.Skills, "Personal")
            .Group(e => e.WidgetLink, "Links")
            .Group(e => e.ProfileLink, "Links")

            // Column types - Set Link type explicitly
            .DataTypeHint(e => e.WidgetLink, ColType.Link)
            .DataTypeHint(e => e.ProfileLink, ColType.Link)

            // Sorting
            .Sortable(e => e.Email, false) // Email not sortable
            .Sortable(e => e.Notes, false) // Notes not sortable

            // Configuration
            .Config(config =>
            {
                config.FreezeColumns = 2; // Freeze ID and Code
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
                config.ShowColumnTypeIcons = false; // Show type icons
                config.BatchSize = 50; // Load 50 rows at a time
                config.LoadAllRows = false; // Use pagination
                config.ShowSearch = true;
            })
            // Configure row action buttons
            .RowActions(
                new RowAction { Id = "menu", Icon = "EllipsisVertical", EventName = "OnRowMenu" }
            )
            // Handle row action clicks
            .OnRowAction(e =>
            {
                var args = e.Value;
                selectedRowIndex.Set(args.RowIndex);
                actionName.Set(args.EventName);
                selectedCell.Set((CellClickEventArgs?)null); // Clear cell selection
                isOpen.Set(true);
                return ValueTask.CompletedTask;
            });

        // Build Sheet content based on interaction type
        object? sheetContent = null;
        string sheetTitle = "Row Details";
        string sheetDescription = "";

        if (selectedRowIndex.Value != null)
        {
            // Row action button was clicked
            var employee = employees.Value.ElementAtOrDefault(selectedRowIndex.Value.Value);

            if (employee != null)
            {
                sheetTitle = "Employee Details";
                sheetDescription = $"Row {selectedRowIndex.Value} information";

                sheetContent = new Card(
                    new StackLayout([
                        $"ID: {employee.Id}",
                        $"Code: {employee.EmployeeCode}",
                        $"Name: {employee.Name}",
                        $"Email: {employee.Email}",
                        $"Age: {employee.Age}",
                        $"Salary: {employee.Salary:C}",
                        $"Performance: {employee.Performance}",
                        $"Active: {employee.IsActive}",
                        $"Manager: {employee.IsManager}",
                        $"Hire Date: {employee.HireDate:d}",
                        $"Last Review: {employee.LastReview:d}",
                        $"Department: {employee.Department}",
                        $"Status: {employee.Status}",
                        $"Priority: {employee.Priority}",
                        $"Notes: {employee.Notes}"
                    ], gap: 8)
                ).Title("Employee Information");
            }
        }
        else
        {
            sheetTitle = "Welcome";
            sheetDescription = "How to interact with the table";
            sheetContent = new StackLayout([
                new Card(
                    new StackLayout([
                        "• Hover over any row to see action buttons on the right",
                        "• Click Edit, Delete, or View icons to perform actions",
                        "• Double-click any cell to view cell details",
                        "",
                        "The action buttons demonstrate the new row actions feature!",
                    ], gap: 8)
                ).Title("Instructions")
            ]);
        }

        // Layout: DataTable is always rendered, Sheet overlays on top when open
        return new Fragment(
            dataTable,
            // Sheet appears as overlay without unmounting the DataTable
            isOpen.Value
                ? new Sheet(_ =>
                {
                    isOpen.Set(false);
                    return ValueTask.CompletedTask;
                }, sheetContent!, sheetTitle, sheetDescription).Width(Size.Fraction(0.4f))
                : null
        );
    }
}
