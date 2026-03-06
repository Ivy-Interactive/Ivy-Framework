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

    public FrontendApiClient(string? frontendApiDomain, HttpMessageHandler messageHandler)
    {
        _frontendApiDomain = frontendApiDomain;
        _httpClient = new HttpClient(messageHandler, disposeHandler: false);
    }

    public async Task<ClerkDevBrowserTokenResponse> CreateDevBrowserTokenAsync(CancellationToken cancellationToken = default)
    {
        var response = await RequestAsync(HttpMethod.Post, $"dev_browser", cancellationToken: cancellationToken);
        return await ParseResponseAsync<ClerkDevBrowserTokenResponse>(response, credentials: null, cancellationToken);
    }

    public async Task<ClerkEnvironmentResponse> GetEnvironmentAsync(ClerkCredentials? credentials = null, CancellationToken cancellationToken = default)
    {
        var response = await RequestAsync(
            HttpMethod.Get,
            "environment",
            credentials,
            cancellationToken: cancellationToken);
        return await ParseResponseAsync<ClerkEnvironmentResponse>(response, credentials, cancellationToken);
    }

    public async Task<ClerkEnvironmentResponse> UpdateEnvironmentAsync(ClerkCredentials credentials, string origin, CancellationToken cancellationToken = default)
    {
        var response = await RequestAsync(
            HttpMethod.Post,
            "environment",
            credentials,
            additionalQueryParameters: "_method=PATCH",
            setHeaders: headers => headers.Add("Origin", origin),
            cancellationToken: cancellationToken);
        return await ParseResponseAsync<ClerkEnvironmentResponse>(response, credentials, cancellationToken);
    }

    public async Task<ClerkClientResponse> GetCurrentClientAsync(ClerkCredentials credentials, CancellationToken cancellationToken = default)
    {
        var response = await RequestAsync(
            HttpMethod.Get,
            "client",
            credentials,
            cancellationToken: cancellationToken);
        return await ParseResponseAsync<ClerkClientResponse>(response, credentials, cancellationToken);
    }

    public async Task<ClerkClientResponse> CreateNewClientAsync(ClerkCredentials credentials, CancellationToken cancellationToken = default)
    {
        var response = await RequestAsync(
            HttpMethod.Post,
            "client",
            cancellationToken: cancellationToken);
        return await ParseResponseAsync<ClerkClientResponse>(response, credentials, cancellationToken);
    }

    public async Task<ClerkTokenResponse> CreateSessionTokenAsync(string sessionId, ClerkCredentials credentials, CancellationToken cancellationToken = default)
    {
        var content = new StringContent("organization_id")
        {
            Headers = { ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded") },
        };

        var response = await RequestAsync(HttpMethod.Post, $"client/sessions/{sessionId}/tokens", credentials, content: content, cancellationToken: cancellationToken);
        return await ParseResponseAsync<ClerkTokenResponse>(response, credentials, cancellationToken);
    }

    public async Task<ClerkSessionResponse> TouchSessionAsync(string sessionId, ClerkCredentials credentials, CancellationToken cancellationToken = default)
    {
        var response = await RequestAsync(HttpMethod.Post, $"client/sessions/{sessionId}/touch", credentials, cancellationToken: cancellationToken);
        return await ParseResponseAsync<ClerkSessionResponse>(response, credentials, cancellationToken);
    }

    public async Task<ClerkSessionResponse> GetSessionAsync(string sessionId, ClerkCredentials credentials, CancellationToken cancellationToken = default)
    {
        var response = await RequestAsync(HttpMethod.Get, $"client/sessions/{sessionId}", credentials, cancellationToken: cancellationToken);
        return await ParseResponseAsync<ClerkSessionResponse>(response, credentials, cancellationToken);
    }

    public async Task<ClerkSessionResponse> EndSessionAsync(string sessionId, ClerkCredentials credentials, CancellationToken cancellationToken = default)
    {
        var response = await RequestAsync(HttpMethod.Post, $"client/sessions/{sessionId}/end", credentials, cancellationToken: cancellationToken);
        return await ParseResponseAsync<ClerkSessionResponse>(response, credentials, cancellationToken);
    }

    public async Task<ClerkClientResponse> RemoveAllSessionsAsync(ClerkCredentials credentials, CancellationToken cancellationToken = default)
    {
        var response = await RequestAsync(HttpMethod.Delete, "client/sessions", credentials, cancellationToken: cancellationToken);
        return await ParseResponseAsync<ClerkClientResponse>(response, credentials, cancellationToken);
    }

    public async Task<ClerkSignInResponse> CreateSignInAsync(ClerkCredentials credentials, string origin, string strategy, string redirectUrl, string? actionCompleteRedirectUrl, CancellationToken cancellationToken = default)
    {
        var formData = new Dictionary<string, string>
        {
            { "strategy", strategy },
            { "redirect_url", redirectUrl }
        };

        if (actionCompleteRedirectUrl is not null)
        {
            formData.Add("action_complete_redirect_url", actionCompleteRedirectUrl);
        }

        var content = new FormUrlEncodedContent(formData);

        var response = await RequestAsync(HttpMethod.Post, "client/sign_ins", credentials, setHeaders: headers => headers.Add("Origin", origin), content: content, cancellationToken: cancellationToken);
        return await ParseResponseAsync<ClerkSignInResponse>(response, credentials, cancellationToken);
    }

    public async Task<ClerkSignInResponse> RetrieveSignInAsync(string signInId, ClerkCredentials credentials, CancellationToken cancellationToken = default)
    {
        var response = await RequestAsync(HttpMethod.Get, $"client/sign_ins/{signInId}", credentials, cancellationToken: cancellationToken);
        return await ParseResponseAsync<ClerkSignInResponse>(response, credentials, cancellationToken);
    }

    public async Task<ClerkSignInResponse> PrepareFirstFactorVerificationAsync(ClerkCredentials credentials, string origin, string signInId, string strategy, string redirectUrl, string? actionCompleteRedirectUrl, CancellationToken cancellationToken = default)
    {
        var formData = new Dictionary<string, string>
        {
            { "strategy", strategy },
            { "redirect_url", redirectUrl }
        };

        if (actionCompleteRedirectUrl is not null)
        {
            formData.Add("action_complete_redirect_url", actionCompleteRedirectUrl);
        }

        var content = new FormUrlEncodedContent(formData);

        var response = await RequestAsync(HttpMethod.Post, $"client/sign_ins/{signInId}/prepare_first_factor", credentials, setHeaders: headers => headers.Add("Origin", origin), content: content, cancellationToken: cancellationToken);
        return await ParseResponseAsync<ClerkSignInResponse>(response, credentials, cancellationToken);
    }

    public async Task<ClerkSignInResponse> CreatePasswordSignInAsync(ClerkCredentials credentials, string identifier, string password, CancellationToken cancellationToken = default)
    {
        var formData = new Dictionary<string, string>
        {
            { "identifier", identifier },
            { "password", password },
            { "strategy", "password" }
        };

        var content = new FormUrlEncodedContent(formData);

        var response = await RequestAsync(HttpMethod.Post, "client/sign_ins", credentials, content: content, cancellationToken: cancellationToken);
        return await ParseResponseAsync<ClerkSignInResponse>(response, credentials, cancellationToken);
    }

    public async Task<ClerkSignUpResponse> CreateSignUpAsync(ClerkCredentials credentials, string origin, string? strategy, string? redirectUrl, string? actionCompleteRedirectUrl, bool transfer, CancellationToken cancellationToken = default)
    {
        var formData = new Dictionary<string, string>();

        if (strategy is not null)
        {
            formData.Add("strategy", strategy);
        }
        if (redirectUrl is not null)
        {
            formData.Add("redirect_url", redirectUrl);
        }
        if (actionCompleteRedirectUrl is not null)
        {
            formData.Add("action_complete_redirect_url", actionCompleteRedirectUrl);
        }
        if (transfer)
        {
            formData.Add("transfer", "true");
        }

        var content = new FormUrlEncodedContent(formData);

        var response = await RequestAsync(HttpMethod.Post, "client/sign_ups", credentials, setHeaders: headers => headers.Add("Origin", origin), content: content, cancellationToken: cancellationToken);
        return await ParseResponseAsync<ClerkSignUpResponse>(response, credentials, cancellationToken);
    }

    public async Task<ClerkSignUpResponse> PrepareSignUpExternalAccountAsync(ClerkCredentials credentials, string signUpId, string origin, string strategy, string redirectUrl, string? actionCompleteRedirectUrl, CancellationToken cancellationToken = default)
    {
        var formData = new Dictionary<string, string>
        {
            { "strategy", strategy },
            { "redirect_url", redirectUrl }
        };

        if (actionCompleteRedirectUrl is not null)
        {
            formData.Add("action_complete_redirect_url", actionCompleteRedirectUrl);
        }

        var content = new FormUrlEncodedContent(formData);

        var response = await RequestAsync(HttpMethod.Post, $"client/sign_ups/{signUpId}/prepare_verification", credentials, setHeaders: headers => headers.Add("Origin", origin), content: content, cancellationToken: cancellationToken);
        return await ParseResponseAsync<ClerkSignUpResponse>(response, credentials, cancellationToken);
    }

    private async Task<HttpResponseMessage> RequestAsync(HttpMethod method, string endpoint, ClerkCredentials? credentials = null, bool sendSessionToken = true, string? additionalQueryParameters = null, Action<HttpRequestHeaders>? setHeaders = null, HttpContent? content = null, CancellationToken cancellationToken = default)
    {
        if (credentials?.DevBrowserToken != null)
        {
            additionalQueryParameters = string.IsNullOrEmpty(additionalQueryParameters)
                ? $"__clerk_db_jwt={credentials.DevBrowserToken}"
                : $"{additionalQueryParameters}&__clerk_db_jwt={credentials.DevBrowserToken}";
        }

        var requestUrl = $"https://{_frontendApiDomain}/v1/{endpoint}?__clerk_api_version={ApiVersion}";
        if (!string.IsNullOrEmpty(additionalQueryParameters))
        {
            requestUrl += $"&{additionalQueryParameters}";
        }

        var request = new HttpRequestMessage(method, requestUrl)
        {
            Content = content
        };

        setHeaders?.Invoke(request.Headers);
        if (sendSessionToken && credentials?.SessionToken != null)
        {
            request.Headers.Add("__session", credentials.SessionToken);
        }

        return await _httpClient.SendAsync(request, cancellationToken);
    }

    private async Task<string> ProcessResponseAsync(HttpResponseMessage response, ClerkCredentials? credentials, CancellationToken cancellationToken)
    {
        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return json;
        }
        else
        {
            ClerkErrorResponse? errorResponse;
            try
            {
                errorResponse = JsonSerializer.Deserialize<ClerkErrorResponse>(json, _jsonSerializerOptions);
            }
            catch (JsonException)
            {
                errorResponse = null;
            }

            if (errorResponse is not null)
                throw new ClerkException(errorResponse);

            throw new ClerkException($"HTTP {(int)response.StatusCode} ({response.ReasonPhrase}): {json}");
        }
    }

    private async Task<T> ParseResponseAsync<T>(HttpResponseMessage response, ClerkCredentials? credentials, CancellationToken cancellationToken)
    {
        var json = await ProcessResponseAsync(response, credentials, cancellationToken);

        try
        {
            return JsonSerializer.Deserialize<T>(json, _jsonSerializerOptions)
                ?? throw new InvalidOperationException("Clerk returned an empty or invalid response.");
        }
        catch (JsonException ex)
        {
            throw new ClerkException($"Failed to deserialize Clerk response: {json}", ex);
        }
    }
}
