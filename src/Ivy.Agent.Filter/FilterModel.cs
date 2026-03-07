using System.Text.Json.Serialization;

namespace Ivy.Filters;

/// <summary>
/// Represents the filter model structure for grid filtering
/// </summary>
public abstract record FilterModel
{
    [JsonPropertyName("filterType")]
    public abstract string FilterType { get; }
}

/// <summary>
/// Join filter model for combining multiple conditions with AND/OR
/// </summary>
public record GroupFilterModel : FilterModel
{
    [JsonPropertyName("filterType")]
    public override string FilterType => "join";

    [JsonPropertyName("type")]
    public required string Type { get; init; } // "AND" or "OR"

    [JsonPropertyName("conditions")]
    public required List<FilterModel> Conditions { get; init; }
}

/// <summary>
/// Field filter model for leaf conditions
/// </summary>
public record FieldFilterModel(string FilterType) : FilterModel
{
    [JsonPropertyName("filterType")]
    public override string FilterType { get; } = FilterType;

    [JsonPropertyName("colId")]
    public required string ColId { get; init; }

    [JsonPropertyName("type")]
    public required string Type { get; init; } // The operation type like "contains", "equals", etc.

    [JsonPropertyName("filter")]
    public object? Filter { get; init; }

    [JsonPropertyName("filterTo")]
    public object? FilterTo { get; init; }
}

/// <summary>
/// Result of parsing a filter, containing the AST and any diagnostics
/// </summary>
public record FilterParseResult
{
    public required string Filter { get; init; }
    public Node? Ast { get; init; }
    public FilterModel? Model { get; init; }
    public IReadOnlyList<Diagnostic> Diagnostics { get; init; } = [];
    public bool HasErrors => Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);
    public UsageInfo? Usage { get; init; }
    public int Iterations { get; init; }
}

/// <summary>
/// Token usage information for cost tracking
/// </summary>
public record UsageInfo
{
    public long InputTokens { get; init; }
    public long OutputTokens { get; init; }
    public long TotalTokens { get; init; }
}