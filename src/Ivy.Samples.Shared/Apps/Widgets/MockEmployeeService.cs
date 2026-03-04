using Ivy.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ivy.Samples.Shared.Apps.Widgets;

public class EmployeeRecord
{
    public int Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
    public decimal Salary { get; set; }
    public double Performance { get; set; }
    public bool IsActive { get; set; }
    public bool IsManager { get; set; }
    public DateTime HireDate { get; set; }
    public DateTime LastReview { get; set; }
    public Icons Status { get; set; }
    public Icons Priority { get; set; }
    public Icons Department { get; set; }
    public string Notes { get; set; } = string.Empty;
    public int? OptionalId { get; set; }
    public string[] Skills { get; set; } = Array.Empty<string>();
    public string? WidgetLink { get; set; }
    public string? ProfileLink { get; set; }
}

public class MockEmployeeService
{
    private readonly List<EmployeeRecord> _employees;

    public MockEmployeeService()
    {
        var allSkills = new[] { "C#", "JavaScript", "Python", "SQL", "React", "Leadership", "Communication", "Problem Solving", "Team Player", "Agile" };

        var random = new Random(42);
        var startDate = new DateTime(2020, 1, 1);

        var departments = new[] { Icons.Building, Icons.Code, Icons.Users, Icons.ShoppingCart, Icons.Headphones };
        var statuses = new[] { Icons.CircleCheck, Icons.Clock, Icons.TriangleAlert, Icons.X, Icons.Pause };
        var priorities = new[] { Icons.ArrowUp, Icons.ArrowRight, Icons.ArrowDown, Icons.Flag, Icons.Star };

        var firstNames = new[] { "John", "Jane", "Mike", "Sarah", "David", "Emily", "Chris", "Lisa", "Tom", "Anna" };
        var lastNames = new[] { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez" };

        _employees = Enumerable.Range(1, 1000).Select(i =>
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

            var skillCount = random.Next(2, 6);
            var skills = Enumerable.Range(0, skillCount)
                .Select(_ => allSkills[random.Next(allSkills.Length)])
                .Distinct()
                .ToArray();

            var widgetLink = "/widgets/charts/area-chart";
            var profileLink = $"https://linkedin.com/in/{firstName.ToLower()}{lastName.ToLower()}{i}";

            return new EmployeeRecord
            {
                Id = i,
                EmployeeCode = $"EMP{i:D4}",
                Name = name,
                Email = email,
                Age = age,
                Salary = salary,
                Performance = performance,
                IsActive = isActive,
                IsManager = isManager,
                HireDate = hireDate,
                LastReview = lastReview,
                Status = status,
                Priority = priority,
                Department = department,
                Notes = notes,
                OptionalId = optionalId,
                Skills = skills,
                WidgetLink = widgetLink,
                ProfileLink = profileLink
            };
        }).ToList();
    }

    public List<EmployeeRecord> GetEmployees() => _employees;

    public void UpdateEmployee(EmployeeRecord updated)
    {
        var original = _employees.FirstOrDefault(e => e.Id == updated.Id);
        if (original != null)
        {
            var index = _employees.IndexOf(original);
            if (index != -1)
            {
                _employees[index] = updated;
            }
        }
    }

    public void DeleteEmployee(int id)
    {
        var employee = _employees.FirstOrDefault(emp => emp.Id == id);
        if (employee != null)
        {
            _employees.Remove(employee);
        }
    }
}
