using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Ivy.Docs.Shared.Services;

/// <summary>
/// Calls the Ivy docs questions API and parses response (markdown or JSON with answer/sources).
/// </summary>
public class IvyDocsQuestionsClient : IIvyDocsQuestionsClient
{
    private const string DefaultBaseUrl = "https://staging.mcp.ivy.app";
    private const string PackageId = "Ivy";
    private const string ClientName = "ivyDocs";

    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

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
            response.EnsureSuccessStatusCode();
            var contentType = response.Content.Headers.ContentType?.MediaType ?? "";
            var raw = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(raw))
                return new IvyDocsQuestionResult("No answer returned.", [], null);

            if (contentType.Contains("json", StringComparison.OrdinalIgnoreCase))
                return ParseJsonResponse(raw);

            return new IvyDocsQuestionResult(raw.Trim(), [], null);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static IvyDocsQuestionResult? ParseJsonResponse(string raw)
    {
        try
        {
            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;
            var answer = root.TryGetProperty("answer", out var a) ? a.GetString() ?? "" : raw;
            var header = root.TryGetProperty("title", out var titleEl) ? titleEl.GetString() : root.TryGetProperty("header", out var headerEl) ? headerEl.GetString() : null;
            var sources = new List<IvyDocsQuestionSource>();
            if (root.TryGetProperty("sources", out var arr) && arr.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in arr.EnumerateArray())
                {
                    var title = item.TryGetProperty("title", out var t) ? t.GetString() : null;
                    var url = item.TryGetProperty("url", out var u) ? u.GetString() : null;
                    if (!string.IsNullOrEmpty(url))
                        sources.Add(new IvyDocsQuestionSource(title ?? "Doc", url));
                }
            }
            return new IvyDocsQuestionResult(answer ?? "", sources, header);
        }
        catch
        {
            return new IvyDocsQuestionResult(raw, [], null);
        }
    }
}
