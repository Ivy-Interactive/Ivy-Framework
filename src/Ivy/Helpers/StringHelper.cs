using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Ivy;

public static class StringHelper
{
    public static string? NullIfEmpty(this string? input) => string.IsNullOrWhiteSpace(input) ? null : input;

    public static string TitleCaseToCamelCase(string titleCase)
    {
        if (string.IsNullOrWhiteSpace(titleCase))
            return string.Empty;

        string camelCase = char.ToLower(titleCase[0]) + titleCase[1..];

        return camelCase;
    }

    public static string CamelCaseToTitleCase(string camelCase)
    {
        if (string.IsNullOrWhiteSpace(camelCase))
            return string.Empty;

        string titleCase = char.ToUpper(camelCase[0]) + camelCase[1..];

        return titleCase;
    }

    public static string? SplitPascalCase(string? input)
    {
        if (input == null) return null;
        string[] words = Regex
            .Matches(input, "([A-Z]+[a-z]+|[0-9]+|[a-z]+|[A-Z]+)")
            .Select(m => char.ToUpper(m.Value[0]) + m.Value[1..])
            .ToArray();
        return string.Join(" ", words);
    }

    public static string TitleCaseToFriendlyUrl(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        if (input.EndsWith("app", StringComparison.InvariantCultureIgnoreCase))
        {
            input = input[..^3];
        }

        bool hadUnderscore = input.StartsWith("_");
        if (hadUnderscore)
        {
            input = input[1..];
        }

        var withWordBoundaries = Regex.Replace(input, @"([A-Z]+)([A-Z][a-z])", "$1-$2");
        withWordBoundaries = Regex.Replace(withWordBoundaries, @"([a-z0-9])([A-Z])", "$1-$2");
        withWordBoundaries = withWordBoundaries
            .Replace('_', '-')
            .Replace(' ', '-');

        var normalized = Regex
            .Replace(withWordBoundaries, "-{2,}", "-")
            .Trim('-')
            .ToLowerInvariant();

        if (hadUnderscore)
        {
            normalized = "_" + normalized;
        }

        return normalized;
    }

    public static string TitleCaseToReadable(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        if (input.EndsWith("app", StringComparison.InvariantCultureIgnoreCase))
        {
            input = input[..^3];
        }

        // Check if this is a hook
        bool isHook = input.Length >= 4 &&
                      input.StartsWith("Use", StringComparison.Ordinal) &&
                      char.IsUpper(input[3]);

        StringBuilder sb = new();

        for (int i = 0; i < input.Length; i++)
        {
            if (char.IsUpper(input[i]) && i > 0)
            {
                // For hooks, don't add space between "Use" and the next word
                if (isHook && i == 3)
                {
                    // Skip adding space after "Use"
                }
                else
                {
                    bool prevIsUpper = char.IsUpper(input[i - 1]);
                    bool nextIsLower = (i + 1 < input.Length) && char.IsLower(input[i + 1]);

                    if (input[i - 1] != ' ' && (!prevIsUpper || nextIsLower))
                    {
                        sb.Append(' ');
                    }
                }
            }

            sb.Append(input[i]);
        }

        return sb.ToString();
    }

    public static string TrimEnd(this string source, string value)
    {
        if (!source.EndsWith(value))
            return source;
        return source.Remove(source.LastIndexOf(value));
    }

    public static string EatRight(this string input, char food)
    {
        return input.EatRight(c => c == food);
    }

    public static string EatRight(this string input, Func<char, bool> foodType)
    {
        if (string.IsNullOrEmpty(input)) return input;
        int i = input.Length - 1;
        while (i >= 0)
        {
            if (foodType(input[i]))
            {
                i--;
            }
            else
            {
                break;
            }
        }
        return input.Substring(0, i + 1);
    }

    public static string EatRight(this string input, string food, StringComparison stringComparison = StringComparison.CurrentCulture)
    {
        if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(food)) return input;

        int cursor = input.Length;
        while (true)
        {
            if (cursor - food.Length >= 0)
            {
                if (input.Substring(cursor - food.Length, food.Length).Equals(food, stringComparison))
                {
                    cursor -= food.Length;
                }
                else
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }

        return input[..cursor];
    }

    public static string EatLeft(this string input, char food)
    {
        return EatLeft(input, c => c == food);
    }

    public static string EatLeft(this string input, Func<char, bool> foodType)
    {
        if (string.IsNullOrEmpty(input)) return input;
        int i = 0;
        while (i < input.Length)
        {
            if (foodType(input[i]))
            {
                i++;
            }
            else
            {
                break;
            }
        }
        return input[i..];
    }

    public static string EatLeft(this string input, string food, StringComparison stringComparison = StringComparison.CurrentCulture)
    {
        if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(food)) return input;

        int cursor = 0;
        int n = input.Length;
        while (true)
        {
            if (cursor + food.Length < n)
            {
                if (input.Substring(cursor, food.Length).Equals(food, stringComparison))
                {
                    cursor += food.Length;
                }
                else
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }

        return input.Substring(cursor, n - cursor);
    }

    public static string LabelFor(string name, Type? type)
    {
        if (type != null)
        {
            if (type.IsDate() && Regex.IsMatch(name, @"[a-z]At$"))
            {
                //remove the 'At' suffix for date fields
                name = name[..^2];
            }
        }
        return SplitPascalCase(name) ?? name;
    }

    public static string GetShortHash(string input, int length = 8)
    {
        using var sha256 = SHA256.Create();
        byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        string base64 = System.Convert.ToBase64String(hash);
        string filtered = new string(base64.ToLower().Where(char.IsLetterOrDigit).ToArray());
        return filtered.Length >= length ? filtered[..length] : filtered.PadRight(length, '0');
    }

    public static string FormatBytes(long bytes)
    {
        string[] sizes = ["B", "KB", "MB", "GB", "TB"];
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }

    public static string FormatNumber(double number, int decimalPlaces = 2)
    {
        static string TrimInvariant(double value, int decimalPlaces)
        {
            string format = decimalPlaces > 0 ? "0." + new string('#', Math.Max(0, decimalPlaces)) : "0";
            var s = value.ToString(format, System.Globalization.CultureInfo.InvariantCulture);
            if (s.Contains('.'))
                s = s.TrimEnd('0').TrimEnd('.');
            return s;
        }

        if (number >= 1_000_000_000)
            return TrimInvariant(number / 1_000_000_000D, decimalPlaces) + "B";
        if (number >= 1_000_000)
            return TrimInvariant(number / 1_000_000D, decimalPlaces) + "M";
        if (number >= 1_000)
            return TrimInvariant(number / 1_000D, decimalPlaces) + "K";
        return TrimInvariant(number, decimalPlaces);
    }
}
