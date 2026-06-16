namespace Ivy.Plugins;

public enum ConfigFieldType
{
    String = 0,
    Integer = 1,
    Boolean = 2,
    Secret = 3, // For sensitive values like tokens
}
