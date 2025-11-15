using System.Text.Json;
using System.Net.Http.Headers;
using Ivy.Auth.Clerk.ApiClient.Responses;

namespace Ivy.Auth.Clerk.ApiClient;

public class FrontendApiClient
{
    private readonly string? _frontendApiDomain;
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

    private const string ApiVersion = "2025-11-10";

    public FrontendApiClient(string? frontendApiDomain, string secretKey)
    {
        _frontendApiDomain = frontendApiDomain;
        _httpClient = new HttpClient();
    }

    public async Task RecreateFrontendPackageApiCalls(string origin, CancellationToken cancellationToken = default)
    {
        var browserToken = await CreateDevBrowserTokenAsync(cancellationToken);
        var environment = await UpdateEnvironmentAsync(browserToken.Id, origin, cancellationToken);
        var client = await GetCurrentClient(browserToken.Id, cancellationToken);
        // var sessionId = client.Sessions.First().Id;
        // var sessionToken = await CreateSessionTokenAsync(sessionId, browserToken.Id, cancellationToken);
    }

    public async Task<ClerkDevBrowserTokenResponse> CreateDevBrowserTokenAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsync($"https://{_frontendApiDomain}.clerk.accounts.dev/v1/dev_browser?__clerk_api_version={ApiVersion}", null, cancellationToken);
        return await ParseResponse<ClerkDevBrowserTokenResponse>(response);
    }

    public async Task<ClerkEnvironmentResponse> GetEnvironmentAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"https://{_frontendApiDomain}.clerk.accounts.dev/v1/environment?__clerk_api_version={ApiVersion}", cancellationToken);
        return await ParseResponse<ClerkEnvironmentResponse>(response);
    }

    public async Task<ClerkEnvironmentResponse> UpdateEnvironmentAsync(string devBrowserJwt, string origin, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"https://{_frontendApiDomain}.clerk.accounts.dev/v1/environment?__clerk_api_version={ApiVersion}&_method=PATCH&__clerk_db_jwt={devBrowserJwt}");
        request.Headers.Add("Origin", origin);
        var response = await _httpClient.SendAsync(request, cancellationToken);
        return await ParseResponse<ClerkEnvironmentResponse>(response);
    }

    public async Task<ClerkClientResponse> GetCurrentClient(string devBrowserJwt, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"https://{_frontendApiDomain}.clerk.accounts.dev/v1/client?__clerk_api_version={ApiVersion}&__clerk_db_jwt={devBrowserJwt}", cancellationToken);
        return await ParseResponse<ClerkClientResponse>(response);
    }

    public async Task<ClerkTokenResponse> CreateSessionTokenAsync(string sessionId, string devBrowserJwt, CancellationToken cancellationToken = default)
    {
        var content = new StringContent("organization_id")
        {
            Headers = { ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded") },
        };

        var response = await _httpClient.PostAsync($"https://{_frontendApiDomain}.clerk.accounts.dev/v1/client/sessions/{sessionId}/tokens?__clerk_api_version={ApiVersion}&__clerk_db_jwt={devBrowserJwt}", content, cancellationToken);
        return await ParseResponse<ClerkTokenResponse>(response);
    }

    private async Task<T> ParseResponse<T>(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            return JsonSerializer.Deserialize<T>(json, _jsonSerializerOptions)
                ?? throw new InvalidOperationException("Clerk returned an empty or invalid response.");
        }
        else
        {
            var errorResponse = JsonSerializer.Deserialize<ClerkErrorResponse>(json, _jsonSerializerOptions);

            if (errorResponse is not null)
                throw new ClerkException(errorResponse);

            throw new ClerkException($"HTTP {(int)response.StatusCode} ({response.ReasonPhrase}): {json}");
        }
    }
}