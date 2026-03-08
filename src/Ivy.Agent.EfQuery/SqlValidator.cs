using System.Text.RegularExpressions;

namespace Ivy.Agent.EfQuery;

internal static partial class SqlValidator
{
    /// <summary>
    /// Returns null if valid, error message if invalid.
    /// </summary>
    public static string? Validate(string sql)
    {
        // Strip comments before validation to prevent hiding malicious keywords
        var stripped = StripComments(sql);
        var trimmed = stripped.Trim();

        if (!SelectRegex().IsMatch(trimmed))
            return "SQL must start with SELECT.";

        if (DangerousKeywordRegex().IsMatch(trimmed))
            return "SQL contains a disallowed keyword (INSERT, UPDATE, DELETE, DROP, ALTER, CREATE, TRUNCATE, EXEC, EXECUTE, GRANT, REVOKE, MERGE).";

        if (MultipleStatementsRegex().IsMatch(trimmed))
            return "Multiple SQL statements are not allowed.";

        return null;
    }

    /// <summary>
    /// Strips single-line (--) and block (/* */) comments from SQL, then collapses whitespace.
    /// </summary>
    internal static string StripComments(string sql)
    {
        var result = BlockCommentRegex().Replace(sql, " ");
        result = LineCommentRegex().Replace(result, " ");
        result = WhitespaceRegex().Replace(result, " ");
        return result;
    }

    [GeneratedRegex(@"^\s*SELECT\b", RegexOptions.IgnoreCase)]
    private static partial Regex SelectRegex();

    [GeneratedRegex(@"\b(INSERT|UPDATE|DELETE|DROP|ALTER|CREATE|TRUNCATE|EXEC|EXECUTE|GRANT|REVOKE|MERGE)\b", RegexOptions.IgnoreCase)]
    private static partial Regex DangerousKeywordRegex();

    [GeneratedRegex(@";\s*\S")]
    private static partial Regex MultipleStatementsRegex();

    [GeneratedRegex(@"/\*[\s\S]*?\*/")]
    private static partial Regex BlockCommentRegex();

    [GeneratedRegex(@"--[^\r\n]*")]
    private static partial Regex LineCommentRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
