namespace Ivy.Tendril.Services;

/// <summary>
///     Shared formatting utilities for human-readable display values.
/// </summary>
public static class FormatHelper
{
    /// <summary>
    ///     Formats a token count as a human-readable string.
    ///     Values >= 1M are formatted as "X.XM", >= 1K as "XK", otherwise as the raw number.
    /// </summary>
    public static string FormatTokens(int tokens)
    {
        return tokens >= 1_000_000 ? $"{tokens / 1_000_000.0:F1}M"
            : tokens >= 1_000 ? $"{tokens / 1_000.0:F0}K"
            : tokens.ToString();
    }

    /// <summary>
    ///     Formats a cost value as a currency string with 2 decimal places.
    ///     Returns "$X.XX" format (e.g. "$12.45" or "$0.00").
    /// </summary>
    public static string FormatCost(decimal cost)
    {
        return $"${cost:F2}";
    }
}