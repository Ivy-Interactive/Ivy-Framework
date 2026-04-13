namespace Ivy.Tendril.Helpers;

public static class ProjectHelper
{
    public static string[] ParseProjects(string? projectValue)
    {
        if (string.IsNullOrWhiteSpace(projectValue))
            return Array.Empty<string>();

        return projectValue
            .Split(',')
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrEmpty(p))
            .ToArray();
    }

    public static bool ContainsProject(string? projectValue, string projectToFind)
    {
        if (string.IsNullOrWhiteSpace(projectValue) || string.IsNullOrWhiteSpace(projectToFind))
            return false;

        var projects = ParseProjects(projectValue);
        return projects.Contains(projectToFind, StringComparer.OrdinalIgnoreCase);
    }

    public static string FormatProjectsForDisplay(string? projectValue)
    {
        var projects = ParseProjects(projectValue);
        return projects.Length > 0 ? string.Join(", ", projects) : "Auto";
    }
}
