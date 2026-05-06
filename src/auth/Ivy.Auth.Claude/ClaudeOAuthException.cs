namespace Ivy.Auth.Claude;

/// <summary>Anthropic Claude OAuth error returned from the authorization or token endpoints</summary>
public class ClaudeOAuthException(string? error, string? errorDescription)
    : Exception($"Claude OAuth error: '{error}' - {errorDescription}")
{
    /// <summary>Error code from the OAuth provider</summary>
    public string? Error { get; } = error;

    /// <summary>Human-readable description</summary>
    public string? ErrorDescription { get; } = errorDescription;
}
