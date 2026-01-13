namespace Ivy.Abstractions;

public interface IVolume
{
    public string GetAbsolutePath(params string[] parts);
}

