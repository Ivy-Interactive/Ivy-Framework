namespace Ivy.Plugins;

public interface IIvyPluginConfig
{
    string? GetValue(string key);
    void SetValue(string key, string value);
    void RemoveValue(string key);
    void Save();
}
