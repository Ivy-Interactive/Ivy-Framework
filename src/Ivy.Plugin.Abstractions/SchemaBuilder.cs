namespace Ivy.Plugins;

public class SchemaBuilder
{
    private readonly List<ConfigFieldDefinition> _fields = [];

    public SchemaBuilder AddString(string key, string? defaultValue = null, string? description = null, bool isRequired = false)
    {
        _fields.Add(new ConfigFieldDefinition
        {
            Key = key,
            Type = ConfigFieldType.String,
            IsRequired = isRequired,
            DefaultValue = defaultValue,
            Description = description
        });
        return this;
    }

    public SchemaBuilder AddSecret(string key, string? description = null, bool isRequired = false)
    {
        _fields.Add(new ConfigFieldDefinition
        {
            Key = key,
            Type = ConfigFieldType.Secret,
            IsRequired = isRequired,
            Description = description
        });
        return this;
    }

    public SchemaBuilder AddInteger(string key, int? defaultValue = null, string? description = null, bool isRequired = false)
    {
        _fields.Add(new ConfigFieldDefinition
        {
            Key = key,
            Type = ConfigFieldType.Integer,
            IsRequired = isRequired,
            DefaultValue = defaultValue?.ToString(),
            Description = description
        });
        return this;
    }

    public SchemaBuilder AddBoolean(string key, bool? defaultValue = null, string? description = null, bool isRequired = false)
    {
        _fields.Add(new ConfigFieldDefinition
        {
            Key = key,
            Type = ConfigFieldType.Boolean,
            IsRequired = isRequired,
            DefaultValue = defaultValue?.ToString()?.ToLowerInvariant(),
            Description = description
        });
        return this;
    }

    public PluginConfigurationSchema Build() => new() { Fields = _fields.AsReadOnly() };
}
