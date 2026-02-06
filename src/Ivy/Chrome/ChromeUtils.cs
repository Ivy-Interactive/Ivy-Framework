using System.Text.RegularExpressions;
using Ivy.Shared;

namespace Ivy.Chrome;

public static class ChromeUtils
{
    private static bool IsWordMatch(string tag, string searchString)
    {
        var words = Regex.Split(tag, @"[-_\s]+");
        return words.Any(word => word.StartsWith(searchString, StringComparison.OrdinalIgnoreCase));
    }

    private static bool LabelContainsAsWord(string label, string searchString)
    {
        if (string.IsNullOrEmpty(label) || string.IsNullOrEmpty(searchString))
            return false;

        var pattern = $@"\b{Regex.Escape(searchString)}\b";
        return Regex.IsMatch(label, pattern, RegexOptions.IgnoreCase);
    }

    public static int ItemMatchScore(MenuItem item, string searchString)
    {
        var label = item.Label ?? "";

        // Exact match gets highest priority (score 5)
        if (string.Equals(label, searchString, StringComparison.OrdinalIgnoreCase))
            return 5;

        // Label starts with search string (score 4)
        if (label.StartsWith(searchString, StringComparison.OrdinalIgnoreCase))
            return 4;

        // Label contains search as whole word - e.g. "Layout" in "Footer Layout" (score 3)
        if (LabelContainsAsWord(label, searchString))
            return 3;

        // Label contains search as substring (score 2)
        if (label.Contains(searchString, StringComparison.OrdinalIgnoreCase))
            return 2;

        // Search hints match gets lowest priority (score 1)
        if (item.SearchHints?.Any(tag => IsWordMatch(tag, searchString)) == true)
            return 1;

        // No match
        return 0;
    }
}