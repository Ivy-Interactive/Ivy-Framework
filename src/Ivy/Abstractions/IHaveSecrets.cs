// ReSharper disable once CheckNamespace
namespace Ivy;

public interface IHaveSecrets
{
    public Secret[] GetSecrets();
}

public sealed record Secret(string Key, string? Preset = null);