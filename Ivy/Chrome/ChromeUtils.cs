using Ivy.Shared;

namespace Ivy.Chrome;

public static class ChromeUtils
{
    private static bool IsWordMatch(string tag, string searchString)
    {
        var words = System.Text.RegularExpressions.Regex.Split(tag, @"[-_\s]+");
        return words.Any(word => word.StartsWith(searchString, StringComparison.OrdinalIgnoreCase));
    }

    public static int ItemMatchScore(MenuItem item, string searchString)
    {
        var label = item.Label ?? "";

        // Exact match gets highest priority (score 3)
        if (string.Equals(label, searchString, StringComparison.OrdinalIgnoreCase))
        {
            return 3;
        }

        // Label contains search string gets medium priority (score 2)
        if (label.Contains(searchString, StringComparison.OrdinalIgnoreCase))
        {
            return 2;
        }

        // Search hints match gets lowest priority (score 1)
        if (item.SearchHints?.Any(tag => IsWordMatch(tag, searchString)) == true)
        {
            return 1;
        }

        // No match
        return 0;
    }
}