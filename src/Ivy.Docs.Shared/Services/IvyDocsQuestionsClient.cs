using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Ivy.Docs.Shared.Services;

/// <summary>
/// Calls the Ivy docs questions API and parses response (markdown or JSON with answer).
/// </summary>
public class IvyDocsQuestionsClient : IIvyDocsQuestionsClient
{
    private const string DefaultBaseUrl = "https://mcp.ivy.app";
    private const string PackageId = "Ivy";
    private const string ClientName = "ivyDocs";

    private readonly HttpClient _httpClient;

    public IvyDocsQuestionsClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        if (string.IsNullOrEmpty(_httpClient.BaseAddress?.ToString()))
            _httpClient.BaseAddress = new Uri(DefaultBaseUrl);
    }

    public async Task<IvyDocsQuestionResult?> AskAsync(string question, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(question))
            return null;

        var query = Uri.EscapeDataString(question.Trim());
        var url = $"/questions?question={query}&packageId={PackageId}&client={ClientName}";

        try
        {
            using var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
            var raw = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = $"Response status code does not indicate success: {(int)response.StatusCode} ({response.StatusCode}).";
                if (!string.IsNullOrWhiteSpace(raw))
                {
                    try
                    {
                        using var errorDoc = JsonDocument.Parse(raw);
                        if (errorDoc.RootElement.TryGetProperty("message", out var msgProp) && msgProp.ValueKind == JsonValueKind.String)
                        {
                            errorMsg = msgProp.GetString()!;
                        }
                        else if (errorDoc.RootElement.TryGetProperty("error", out var errProp) && errProp.ValueKind == JsonValueKind.String)
                        {
                            errorMsg = errProp.GetString()!;
                        }
                        else
                        {
                            errorMsg = raw.Trim();
                        }
                    }
                    catch
                    {
                        // Not JSON, just use the raw text
                        errorMsg = raw.Trim();
                    }
                }
                throw new Exception(errorMsg);
            }

            var contentType = response.Content.Headers.ContentType?.MediaType ?? "";
            if (string.IsNullOrWhiteSpace(raw))
                return new IvyDocsQuestionResult("No answer returned.", null);

            if (contentType.Contains("json", StringComparison.OrdinalIgnoreCase) || raw.TrimStart().StartsWith("{"))
                return ParseJsonResponse(raw) ?? new IvyDocsQuestionResult(raw.Trim(), null);

            return new IvyDocsQuestionResult(raw.Trim(), null);
        }
        catch (Exception)
        {
            throw;
        }
    }

    private static IvyDocsQuestionResult? ParseJsonResponse(string raw)
    {
        try
        {
            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;
            var (answer, header) = GetAnswerAndTitle(root, raw);
            return new IvyDocsQuestionResult(answer ?? raw, header);
        }
        catch
        {
            return null;
        }
    }

    private static (string? answer, string? header) GetAnswerAndTitle(JsonElement root, string raw)
    {
        foreach (var name in new[] { "answer", "response", "text", "content", "body" })
        {
            if (root.TryGetProperty(name, out var a))
            {
                var answer = a.ValueKind == JsonValueKind.String ? a.GetString() : raw;
                string? header = null;
                foreach (var t in new[] { "title", "header", "subject" })
                    if (root.TryGetProperty(t, out var el)) { header = el.GetString(); break; }
                return (answer ?? raw, header);
            }
        }
        if (root.TryGetProperty("data", out var data))
            return GetAnswerAndTitle(data, raw);
        if (root.TryGetProperty("result", out var result))
            return GetAnswerAndTitle(result, raw);
        return (raw, null);
    }
}
