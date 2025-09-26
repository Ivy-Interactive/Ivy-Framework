using System.Text.Json.Serialization;

namespace Ivy.Formulas;

/// <summary>
/// Represents the filter model structure for grid filtering
/// </summary>
public abstract record FormulaModel
{
    [JsonPropertyName("filterType")]
    public abstract string FilterType { get; }
}

/// <summary>
/// Join filter model for combining multiple conditions with AND/OR
/// </summary>
public record GroupFormulaModel : FormulaModel
{
    [JsonPropertyName("filterType")]
    public override string FilterType => "join";

    [JsonPropertyName("type")]
    public required string Type { get; init; } // "AND" or "OR"

    [JsonPropertyName("conditions")]
    public required List<FormulaModel> Conditions { get; init; }
}

/// <summary>
/// Field filter model for leaf conditions
/// </summary>
public record FieldFormulaModel : FormulaModel
{
    [JsonPropertyName("filterType")]
    public override string FilterType { get; }

    [JsonPropertyName("colId")]
    public required string ColId { get; init; }

    [JsonPropertyName("type")]
    public required string Type { get; init; } // The operation type like "contains", "equals", etc.

    [JsonPropertyName("filter")]
    public object? Filter { get; init; }

    [JsonPropertyName("filterTo")]
    public object? FilterTo { get; init; }

    public FieldFormulaModel(string filterType)
    {
        FilterType = filterType;
    }
}

/// <summary>
/// Result of parsing a formula, containing the AST and any diagnostics
/// </summary>
public record FormulaParseResult
{
    public Node? Ast { get; init; }
    public FormulaModel? Model { get; init; }
    public IReadOnlyList<Diagnostic> Diagnostics { get; init; } = [];
    public bool HasErrors => Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);
}