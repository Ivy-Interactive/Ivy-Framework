using System.Globalization;

namespace Ivy.Plugins;

public interface IIvyPluginConfig
{
    string? GetValue(string key);
    void SetValue(string key, string value);
    void RemoveValue(string key);
    void Save();

    int? GetInt(string key)
    {
        var value = GetValue(key);
        return int.TryParse(value, CultureInfo.InvariantCulture, out var result) ? result : null;
    }

    bool? GetBool(string key)
    {
        var value = GetValue(key);
        return bool.TryParse(value, out var result) ? result : null;
    }

    T? Get<T>(string key) where T : struct
    {
        var value = GetValue(key);
        if (value is null) return null;

        try
        {
            return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
        }
        catch
        {
            return null;
        }
    }
}
