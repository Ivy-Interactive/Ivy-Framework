namespace Ivy.Filters;

/// <summary>
/// YAML-serializable representation of field metadata
/// </summary>
public class FieldMetaYaml
{
    public string DisplayName { get; set; } = string.Empty;
    public string ColId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}
