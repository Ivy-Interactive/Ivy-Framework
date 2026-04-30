namespace Ivy.Plugins;

public record ConfigFieldDefinition
{
    public required string Key { get; init; }
    public required ConfigFieldType Type { get; init; }
    public required bool IsRequired { get; init; }
    public string? Description { get; init; }
    public string? DefaultValue { get; init; }
}
