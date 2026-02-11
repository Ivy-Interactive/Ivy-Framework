using System.Text.RegularExpressions;
using Ivy.Shared;

namespace Ivy.Chrome;

public static class ChromeUtils
{
    private static string NormalizeForSearch(string s)
    {
        if (string.IsNullOrEmpty(s))
            return "";
        var normalized = Regex.Replace(s, @"[\s\-_]+", "");
        return normalized.ToLowerInvariant();
    }

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
        var normalizedLabel = NormalizeForSearch(label);
        var normalizedSearch = NormalizeForSearch(searchString);

        // Exact match gets highest priority (score 5)
        if (string.Equals(label, searchString, StringComparison.OrdinalIgnoreCase))
            return 5;

        // Normalized exact match (score 5)
        if (normalizedSearch.Length > 0 && normalizedLabel == normalizedSearch)
            return 5;

        // Label starts with search string (score 4)
        if (label.StartsWith(searchString, StringComparison.OrdinalIgnoreCase))
            return 4;

        // Normalized label starts with normalized search (score 4)
        if (normalizedSearch.Length > 0 && normalizedLabel.StartsWith(normalizedSearch))
            return 4;

        // Label contains search as whole word - e.g. "Layout" in "Footer Layout" (score 3)
        if (LabelContainsAsWord(label, searchString))
            return 3;

        // Label contains search as substring (score 2)
        if (label.Contains(searchString, StringComparison.OrdinalIgnoreCase))
            return 2;

        // Normalized label contains normalized search: "datatables" matches "Data Table" (score 2)
        if (normalizedSearch.Length > 0 && normalizedLabel.Contains(normalizedSearch))
            return 2;

        // Search hints match (also check normalized form)
        if (item.SearchHints?.Any(tag =>
            IsWordMatch(tag, searchString) ||
            (normalizedSearch.Length > 0 && NormalizeForSearch(tag).Contains(normalizedSearch))) == true)
            return 1;

        // No match
        return 0;
    }
}