// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Marks an IAuthTokenHandler implementation as the token handler for a specific OAuth provider.
/// Classes with this attribute will be automatically discovered and registered in the OAuth token handler registry.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class OAuthTokenHandlerAttribute : Attribute
{
    /// <summary>
    /// The OAuth provider identifier this handler is for
    /// </summary>
    public string Provider { get; }

    /// <summary>
    /// Marks an IAuthTokenHandler implementation for a specific OAuth provider
    /// </summary>
    /// <param name="provider">The OAuth provider identifier this handler is for</param>
    public OAuthTokenHandlerAttribute(string provider)
    {
        Provider = provider;
    }
}
