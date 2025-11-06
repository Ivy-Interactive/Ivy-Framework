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
        // Manage Sheet open/close state at app level
        var isOpen = this.UseState(false);

        // Store selected cell information
        var selectedCell = this.UseState<CellClickEventArgs?>(() => null);

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

            return Enumerable.Range(1, 200).Select(i => new EmployeeRecord(
                Id: i,
                EmployeeCode: $"EMP{i:D4}",
                Name: $"{firstNames[random.Next(firstNames.Length)]} {lastNames[random.Next(lastNames.Length)]}",
                Email: $"employee{i}@company.com",
                Age: random.Next(22, 65),
                Salary: (decimal)(random.Next(30000, 150000) / 1000 * 1000),
                Performance: Math.Round(random.NextDouble() * 5, 2),
                IsActive: random.NextDouble() > 0.2,
                IsManager: random.NextDouble() > 0.8,
                HireDate: startDate.AddDays(random.Next(0, 1826)),
                LastReview: DateTime.Now.AddDays(-random.Next(0, 365)),
                Status: statuses[random.Next(statuses.Length)],
                Priority: priorities[random.Next(priorities.Length)],
                Department: departments[random.Next(departments.Length)],
                Notes: $"Employee notes for {i}",
                OptionalId: random.NextDouble() > 0.3 ? random.Next(1, 1000) : null
            )).ToList();
        });

        // The DataTable builder will be recreated each time, but use the cached employee data
        var dataTable = employees.Value.AsQueryable().ToDataTable()
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

            // Table dimensions
            .Width(Size.Full())
            .Height(Size.Full())

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
            })
            // Event handler for double-click
            .OnCellActivated(e =>
            {
                selectedCell.Set(e.Value);
                isOpen.Set(true);
                return ValueTask.CompletedTask;
            });

        // Build Sheet content with cell details
        object? sheetContent = null;
        if (selectedCell.Value != null)
        {
            var cell = selectedCell.Value;
            var employee = employees.Value.ElementAtOrDefault(cell.RowIndex);

            if (employee != null)
            {
                sheetContent = new StackLayout([
                    new Card(
                        new StackLayout([
                            $"Row: {cell.RowIndex}",
                            $"Column: {cell.ColumnName}",
                            $"Value: {cell.CellValue ?? "(null)"}"
                        ], gap: 8)
                    ).Title("Cell Information"),

                    new Card(
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
                            $"Notes: {employee.Notes}"
                        ], gap: 8)
                    ).Title("Employee Details")
                ], gap: 16);
            }
        }
        else
        {
            sheetContent = "Double-click any cell in the DataTable to view employee details!";
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
                }, sheetContent!, "Cell Details", "Double-click any cell to view employee information").Width(Size.Fraction(0.4f))
                : null
        );
    }
}
