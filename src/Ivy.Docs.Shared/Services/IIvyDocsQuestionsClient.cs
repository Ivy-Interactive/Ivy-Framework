namespace Ivy.Docs.Shared.Services;

/// <summary>
/// Client for the Ivy docs AI questions API (e.g. staging.mcp.ivy.app/questions).
/// </summary>
public interface IIvyDocsQuestionsClient
{
    Task<IvyDocsQuestionResult?> AskAsync(string question, CancellationToken cancellationToken = default);
}

public record IvyDocsQuestionResult(string Answer, IReadOnlyList<IvyDocsQuestionSource> Sources);

public record IvyDocsQuestionSource(string Title, string Url);
