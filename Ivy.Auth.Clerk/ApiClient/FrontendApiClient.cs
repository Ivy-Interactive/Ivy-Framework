using System.Text.Json;
using System.Net.Http.Headers;
using Ivy.Auth.Clerk.ApiClient.Responses;

namespace Ivy.Auth.Clerk.ApiClient;

public class FrontendApiClient(string? frontendApiDomain)
{
    private readonly string? _frontendApiDomain = frontendApiDomain;
    private readonly HttpClient _httpClient = new HttpClient();
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

    private const string ApiVersion = "2025-11-10";

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

    public async Task<ClerkSessionResponse> TouchSessionAsync(string sessionId, string devBrowserJwt, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsync($"https://{_frontendApiDomain}.clerk.accounts.dev/v1/client/sessions/{sessionId}/touch?__clerk_api_version={ApiVersion}&__clerk_db_jwt={devBrowserJwt}", null, cancellationToken);
        return await ParseResponse<ClerkSessionResponse>(response);
    }

    public async Task<ClerkSessionResponse> GetSessionAsync(string sessionId, string devBrowserJwt, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"https://{_frontendApiDomain}.clerk.accounts.dev/v1/client/sessions/{sessionId}?__clerk_api_version={ApiVersion}&__clerk_db_jwt={devBrowserJwt}", cancellationToken);
        return await ParseResponse<ClerkSessionResponse>(response);
    }

    public async Task<ClerkSessionResponse> EndSessionAsync(string sessionId, string devBrowserJwt, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsync($"https://{_frontendApiDomain}.clerk.accounts.dev/v1/client/sessions/{sessionId}/end?__clerk_api_version={ApiVersion}&__clerk_db_jwt={devBrowserJwt}", null, cancellationToken);
        return await ParseResponse<ClerkSessionResponse>(response);
    }

    public async Task<ClerkClientResponse> RemoveAllSessionsAsync(string devBrowserJwt, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync($"https://{_frontendApiDomain}.clerk.accounts.dev/v1/client/sessions?__clerk_api_version={ApiVersion}&__clerk_db_jwt={devBrowserJwt}", cancellationToken);
        return await ParseResponse<ClerkClientResponse>(response);
    }

    public async Task<ClerkSignInResponse> CreateSignInAsync(string devBrowserJwt, string origin, string strategy, string redirectUrl, string? actionCompleteRedirectUrl, CancellationToken cancellationToken = default)
    {
        var content = new MultipartFormDataContent
        {
            Headers = { ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded") },
        };
        content.Headers.Add("Origin", origin);

        content.Add(new StringContent(strategy), "strategy");
        content.Add(new StringContent(redirectUrl), "redirect_url");
        if (actionCompleteRedirectUrl is not null)
        {
            content.Add(new StringContent(actionCompleteRedirectUrl), "action_complete_redirect_url");
        }

        var response = await _httpClient.PostAsync($"https://{_frontendApiDomain}.clerk.accounts.dev/v1/client/sign_ins?__clerk_api_version={ApiVersion}&__clerk_db_jwt={devBrowserJwt}", content, cancellationToken);
        return await ParseResponse<ClerkSignInResponse>(response);
    }

    public async Task<string> PrepareFirstFactorVerificationAsync(string devBrowserJwt, string origin, string signInId, string strategy, string redirectUrl, string? actionCompleteRedirectUrl, CancellationToken cancellationToken = default)
    {
        var content = new MultipartFormDataContent
        {
            Headers = { ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded") },
        };
        content.Headers.Add("Origin", origin);

        content.Add(new StringContent(strategy), "strategy");
        content.Add(new StringContent(redirectUrl), "redirect_url");
        if (actionCompleteRedirectUrl is not null)
        {
            content.Add(new StringContent(actionCompleteRedirectUrl), "action_complete_redirect_url");
        }

        var response = await _httpClient.PostAsync($"https://{_frontendApiDomain}.clerk.accounts.dev/v1/client/sign_ins/{signInId}/prepare_first_factor?__clerk_api_version={ApiVersion}&__clerk_db_jwt={devBrowserJwt}", content, cancellationToken);
        return await ProcessResponse(response);
    }

    private async Task<string> ProcessResponse(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            return json;
        }
        else
        {
            var errorResponse = JsonSerializer.Deserialize<ClerkErrorResponse>(json, _jsonSerializerOptions);

            if (errorResponse is not null)
                throw new ClerkException(errorResponse);

            throw new ClerkException($"HTTP {(int)response.StatusCode} ({response.ReasonPhrase}): {json}");
        }
    }

    private async Task<T> ParseResponse<T>(HttpResponseMessage response)
    {
        var json = await ProcessResponse(response);

        return JsonSerializer.Deserialize<T>(json, _jsonSerializerOptions)
            ?? throw new InvalidOperationException("Clerk returned an empty or invalid response.");
    }
}
