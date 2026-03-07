using System.Text.RegularExpressions;

namespace Ivy.Agent.EfQuery;

internal static partial class SqlValidator
{
    /// <summary>
    /// Returns null if valid, error message if invalid.
    /// </summary>
    public static string? Validate(string sql)
    {
        var trimmed = sql.Trim();

        if (!SelectRegex().IsMatch(trimmed))
            return "SQL must start with SELECT.";

        if (DangerousKeywordRegex().IsMatch(trimmed))
            return "SQL contains a disallowed keyword (INSERT, UPDATE, DELETE, DROP, ALTER, CREATE, TRUNCATE, EXEC, EXECUTE, GRANT, REVOKE).";

        if (trimmed.Contains("--") || trimmed.Contains("/*"))
            return "SQL comments are not allowed.";

        if (MultipleStatementsRegex().IsMatch(trimmed))
            return "Multiple SQL statements are not allowed.";

        return null;
    }

    [GeneratedRegex(@"^\s*SELECT\b", RegexOptions.IgnoreCase)]
    private static partial Regex SelectRegex();

    [GeneratedRegex(@"\b(INSERT|UPDATE|DELETE|DROP|ALTER|CREATE|TRUNCATE|EXEC|EXECUTE|GRANT|REVOKE)\b", RegexOptions.IgnoreCase)]
    private static partial Regex DangerousKeywordRegex();

    [GeneratedRegex(@";\s*\S")]
    private static partial Regex MultipleStatementsRegex();
}
