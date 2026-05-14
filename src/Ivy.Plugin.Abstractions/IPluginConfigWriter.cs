namespace Ivy.Plugins;

public interface IPluginConfigWriter
{
    void SetValue(string key, string value);
    void RemoveValue(string key);
    void Save();
}
