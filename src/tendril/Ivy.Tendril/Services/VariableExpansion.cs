using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace Ivy.Tendril.Services;

/// <summary>
///     Handles variable expansion in configuration values.
///     Supports:
///     - %DotnetUserSecrets:Section:Key% - Read from .NET user secrets
///     - %TENDRIL_HOME% - Expand to Tendril home path
///     - %ANY_ENV_VAR% - Standard environment variable expansion
/// </summary>
public static class VariableExpansion
{
    private static IConfigurationRoot? _userSecretsConfig;

    /// <summary>
    ///     Initialize user secrets from the directory containing a .csproj with UserSecretsId.
    /// </summary>
    public static void InitializeUserSecrets(string configDirectory)
    {
        try
        {
            // Look for .csproj file in config directory
            var csprojFiles = Directory.GetFiles(configDirectory, "*.csproj", SearchOption.TopDirectoryOnly);
            if (csprojFiles.Length == 0) return;

            var csprojPath = csprojFiles[0];
            var csprojContent = FileHelper.ReadAllText(csprojPath);

            // Extract UserSecretsId from .csproj
            var match = Regex.Match(csprojContent, @"<UserSecretsId>([^<]+)</UserSecretsId>");
            if (!match.Success) return;

            var userSecretsId = match.Groups[1].Value;

            // Build configuration from user secrets
            var builder = new ConfigurationBuilder()
                .SetBasePath(configDirectory)
                .AddUserSecrets(userSecretsId);

            _userSecretsConfig = builder.Build();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to initialize user secrets from '{configDirectory}': {ex.Message}");
            _userSecretsConfig = null;
        }
    }

    /// <summary>
    ///     Expand variables in a string value.
    /// </summary>
    public static string ExpandVariables(string value, string? tendrilHome)
    {
        if (string.IsNullOrEmpty(value)) return value;

        // Expand DotnetUserSecrets references: %DotnetUserSecrets:Section:Key%
        value = ExpandDotnetUserSecrets(value);

        // Expand TENDRIL_HOME
        if (!string.IsNullOrEmpty(tendrilHome)) value = value.Replace("%TENDRIL_HOME%", tendrilHome);

        // Expand environment variables (both %VAR% and $VAR formats)
        // This handles %REPOS_HOME% and any other env vars
        value = Environment.ExpandEnvironmentVariables(value);

        // Normalize path separators: convert forward slashes to backslashes on Windows,
        // and remove any double slashes that might have been created during expansion
        value = NormalizePath(value);

        return value;
    }

    /// <summary>
    ///     Normalize path separators and clean up double separators.
    /// </summary>
    private static string NormalizePath(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;

        // Only normalize if the string contains path-like patterns
        // (contains both path separators or environment variable markers)
        if (!value.Contains('/') && !value.Contains('\\')) return value;

        // On Windows, standardize to backslashes
        if (Path.DirectorySeparatorChar == '\\')
            value = value.Replace('/', '\\');
        // On Unix, standardize to forward slashes
        else
            value = value.Replace('\\', '/');

        // Remove double separators (e.g., \\ or //) — but skip URI scheme separators (e.g., https://)
        var sep = Path.DirectorySeparatorChar.ToString();
        var doubleSep = sep + sep;
        if (value.Contains(doubleSep))
            value = Regex.Replace(value, @"(?<!:)(" + Regex.Escape(doubleSep) + @")", sep);

        return value;
    }

    /// <summary>
    ///     Expand DotnetUserSecrets references in the format %DotnetUserSecrets:Section:Key% or DotnetUserSecrets:Section:Key
    /// </summary>
    private static string ExpandDotnetUserSecrets(string value)
    {
        if (_userSecretsConfig == null) return value;

        // Pattern: %DotnetUserSecrets:Section:Key% or DotnetUserSecrets:Section:Key
        var pattern = @"%?DotnetUserSecrets:([^%\s]+)%?";
        return Regex.Replace(value, pattern, match =>
        {
            var configPath = match.Groups[1].Value;
            var secretValue = _userSecretsConfig[configPath];
            return secretValue ?? match.Value; // Keep original if not found
        });
    }

    /// <summary>
    ///     Recursively expand variables in a dictionary (for nested config objects).
    /// </summary>
    public static void ExpandInDictionary(Dictionary<string, object> dict, string? tendrilHome)
    {
        foreach (var key in dict.Keys.ToList())
        {
            var value = dict[key];
            if (value is string stringValue)
                dict[key] = ExpandVariables(stringValue, tendrilHome);
            else if (value is Dictionary<string, object> nestedDict)
                ExpandInDictionary(nestedDict, tendrilHome);
            else if (value is List<object> list)
                for (var i = 0; i < list.Count; i++)
                    if (list[i] is string str)
                        list[i] = ExpandVariables(str, tendrilHome);
                    else if (list[i] is Dictionary<string, object> itemDict) ExpandInDictionary(itemDict, tendrilHome);
        }
    }
}
