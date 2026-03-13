using System.Text.RegularExpressions;

namespace Ivy;

public static class ValidationHelper
{
    public static bool IsValidRequired(object? e)
    {
        return e switch
        {
            null => false,
            Guid guid => guid != Guid.Empty,
            DateTime dt => dt != DateTime.MinValue,
            string str => !string.IsNullOrWhiteSpace(str),
            int i => i != 0,
            double i => i != 0.0,
            _ => true
        };
    }

    public static bool IsEmptyContent(object? obj)
    {
        if (obj == null)
        {
            return true;
        }

        if (obj is string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        if (obj is bool b)
        {
            return !b;
        }

        return false;
    }

    public static string? ValidateRedirectUrl(string? url, bool allowExternal = false, string? currentOrigin = null)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        url = url.Trim();

        // Allow relative paths (starting with /)
        if (url.StartsWith('/'))
        {
            // Validate it's a safe relative path (no protocol, no javascript:, etc.)
            if (url.Contains(':'))
            {
                return null;
            }
            return url;
        }

        // For external URLs, validate protocol and optionally origin
        try
        {
            var uri = new Uri(url, UriKind.Absolute);

            // Only allow http and https protocols
            if (uri.Scheme != "http" && uri.Scheme != "https")
            {
                return null;
            }

            // If external URLs are not allowed, only allow same-origin
            if (!allowExternal)
            {
                if (string.IsNullOrEmpty(currentOrigin))
                {
                    // Reject external URLs when allowExternal is false and no origin is provided
                    return null;
                }

                var currentUri = new Uri(currentOrigin);
                if (uri.Scheme != currentUri.Scheme || uri.Host != currentUri.Host || uri.Port != currentUri.Port)
                {
                    return null;
                }
            }

            return uri.ToString();
        }
        catch (UriFormatException)
        {
            // Invalid URL format
            return null;
        }
    }

    public static string? ValidateLinkUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        url = url.Trim();

        // Allow mailto: URLs for email links
        if (url.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
        {
            // Basic validation: must have at least one character after mailto:
            // and should not contain dangerous characters or protocol injection
            var afterProtocol = url.Substring(7); // After "mailto:"
            if (string.IsNullOrWhiteSpace(afterProtocol))
            {
                return null;
            }

            // Check for protocol injection
            if (afterProtocol.Contains("://"))
            {
                return null;
            }

            // Validate mailto format: mailto:email@domain.com[?subject=...&body=...]
            // Allow query parameters for subject, body, cc, bcc
            if (!Regex.IsMatch(url, @"^mailto:[^#]+$", RegexOptions.IgnoreCase))
            {
                return null;
            }

            return url;
        }

        // Allow app:// URLs (Ivy internal navigation)
        if (url.StartsWith("app://", StringComparison.OrdinalIgnoreCase))
        {
            // Validate app:// URLs don't contain dangerous characters
            // Allow query parameters (? and &) but prevent fragments (#) and protocol injection (multiple colons)
            // Pattern: app://[app-id][?query-params] where query-params can contain & but not #
            if (url.Contains('#'))
            {
                return null; // Fragments not allowed in app:// URLs
            }

            // Check for protocol injection (multiple colons after app://)
            var afterProtocol = url.Substring(7); // After "app://"
            if (afterProtocol.Contains("://") || Regex.IsMatch(afterProtocol, @":[^?&/]"))
            {
                return null; // Protocol injection attempt
            }

            // Validate format: app://[app-id][?query-params]
            if (!Regex.IsMatch(url, @"^app://[^:#]*(\?[^#]*)?$", RegexOptions.IgnoreCase))
            {
                return null;
            }

            return url;
        }

        // Allow anchor links (starting with #)
        if (url.StartsWith('#'))
        {
            // Validate anchor links are safe
            // Allow colons in anchor IDs (HTML5 allows this), but prevent query params and fragments
            // Pattern: #[anchor-id] where anchor-id can contain colons but not ? or &
            if (url.Contains('?') || url.Contains('&'))
            {
                return null; // Query parameters not allowed in anchor links
            }

            // Additional check: prevent protocol injection attempts
            var afterHash = url.Substring(1);
            if (afterHash.Contains("://"))
            {
                return null; // Protocol injection attempt
            }

            // Validate format: #[anchor-id] where anchor-id can contain colons
            if (!Regex.IsMatch(url, @"^#[^?&]*$"))
            {
                return null;
            }

            return url;
        }

        // Allow relative paths (starting with /)
        if (url.StartsWith('/'))
        {
            // Validate it's a safe relative path
            if (url.Contains(':'))
            {
                return null;
            }
            return url;
        }

        // For absolute URLs, validate protocol
        try
        {
            var uri = new Uri(url, UriKind.Absolute);

            // Only allow http and https protocols (prevent javascript:, data:, etc.)
            if (uri.Scheme != "http" && uri.Scheme != "https")
            {
                return null;
            }

            return uri.ToString();
        }
        catch (UriFormatException)
        {
            // Invalid URL format - treat as relative if it doesn't contain colons
            if (!url.Contains(':'))
            {
                // Might be a relative path without leading slash
                return url.StartsWith('/') ? url : $"/{url}";
            }
            return null;
        }
    }

    public static bool IsSafeAppId(string? appId)
    {
        if (string.IsNullOrWhiteSpace(appId))
        {
            return false;
        }

        // AppId should not contain protocol separators, query parameters, or other URL components
        if (appId.Contains(':') || appId.Contains('?') || appId.Contains('#') || appId.Contains('&') || appId.Contains('%') || appId.Contains('\\'))
        {
            return false;
        }

        // AppId should not start with / (handled separately in NavigateArgs)
        if (appId.StartsWith('/'))
        {
            return false;
        }

        // AppId should not contain control characters or dangerous patterns
        if (appId.Any(char.IsControl))
        {
            return false;
        }

        return true;
    }
}
