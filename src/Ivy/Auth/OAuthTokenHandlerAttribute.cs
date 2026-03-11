// ReSharper disable once CheckNamespace
namespace Ivy;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class OAuthTokenHandlerAttribute : Attribute
{
    public string Provider { get; }

    public OAuthTokenHandlerAttribute(string provider)
    {
        Provider = provider;
    }
}
