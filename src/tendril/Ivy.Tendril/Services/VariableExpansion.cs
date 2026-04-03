using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;

namespace Ivy.Tendril.Services;

/// <summary>
/// Handles variable expansion in configuration values.
/// Supports:
/// - %DotnetUserSecrets:Section:Key% - Read from .NET user secrets
/// - %TENDRIL_HOME% - Expand to Tendril data path
/// - %REPOS_HOME% - Expand to repos home path
/// - Environment variable expansion
/// </summary>
public static class VariableExpansion
{
    private static IConfigurationRoot? _userSecretsConfig;
    private static string? _userSecretsPath;

    /// <summary>
    /// Initialize user secrets from the directory containing a .csproj with UserSecretsId.
    /// </summary>
    public static void InitializeUserSecrets(string configDirectory)
    {
        try
        {
            // Look for .csproj file in config directory
            var csprojFiles = Directory.GetFiles(configDirectory, "*.csproj", SearchOption.TopDirectoryOnly);
            if (csprojFiles.Length == 0)
            {
                return;
            }

            var csprojPath = csprojFiles[0];
            var csprojContent = File.ReadAllText(csprojPath);

            // Extract UserSecretsId from .csproj
            var match = Regex.Match(csprojContent, @"<UserSecretsId>([^<]+)</UserSecretsId>");
            if (!match.Success)
            {
                return;
            }

            var userSecretsId = match.Groups[1].Value;
            _userSecretsPath = configDirectory;

            // Build configuration from user secrets
            var builder = new ConfigurationBuilder()
                .SetBasePath(configDirectory)
                .AddUserSecrets(userSecretsId);

            _userSecretsConfig = builder.Build();
        }
        catch
        {
            // Silently fail if user secrets can't be loaded
            _userSecretsConfig = null;
        }
    }

    /// <summary>
    /// Expand variables in a string value.
    /// </summary>
    public static string ExpandVariables(string value, string? tendrilHome, string? reposHome)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        // Expand DotnetUserSecrets references: %DotnetUserSecrets:Section:Key%
        value = ExpandDotnetUserSecrets(value);

        // Expand TENDRIL_HOME
        if (!string.IsNullOrEmpty(tendrilHome))
        {
            value = value.Replace("%TENDRIL_HOME%", tendrilHome);
        }

        // Expand REPOS_HOME
        if (!string.IsNullOrEmpty(reposHome))
        {
            value = value.Replace("%REPOS_HOME%", reposHome);
        }

        // Expand environment variables (both %VAR% and $VAR formats)
        value = Environment.ExpandEnvironmentVariables(value);

        return value;
    }

    /// <summary>
    /// Expand DotnetUserSecrets references in the format %DotnetUserSecrets:Section:Key% or DotnetUserSecrets:Section:Key
    /// </summary>
    private static string ExpandDotnetUserSecrets(string value)
    {
        if (_userSecretsConfig == null)
        {
            return value;
        }

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
    /// Recursively expand variables in a dictionary (for nested config objects).
    /// </summary>
    public static void ExpandInDictionary(Dictionary<string, object> dict, string? tendrilHome, string? reposHome)
    {
        foreach (var key in dict.Keys.ToList())
        {
            var value = dict[key];
            if (value is string stringValue)
            {
                dict[key] = ExpandVariables(stringValue, tendrilHome, reposHome);
            }
            else if (value is Dictionary<string, object> nestedDict)
            {
                ExpandInDictionary(nestedDict, tendrilHome, reposHome);
            }
            else if (value is List<object> list)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] is string str)
                    {
                        list[i] = ExpandVariables(str, tendrilHome, reposHome);
                    }
                    else if (list[i] is Dictionary<string, object> itemDict)
                    {
                        ExpandInDictionary(itemDict, tendrilHome, reposHome);
                    }
                }
            }
        }
    }
}
