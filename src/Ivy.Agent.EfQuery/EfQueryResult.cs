namespace Ivy.Agent.EfQuery;

public record EfQueryResult
{
    public required string Xaml { get; init; }
    public required string Sql { get; init; }
    public required string Plan { get; init; }
    public long InputTokens { get; init; }
    public long OutputTokens { get; init; }
    public int Iterations { get; init; }
}
