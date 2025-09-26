namespace Ivy.Services;

public interface IHaveSecrets
{
    public Secret[] GetSecrets();
}

public sealed record Secret(string Key);