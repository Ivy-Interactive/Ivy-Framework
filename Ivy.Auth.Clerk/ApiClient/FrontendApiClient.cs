using System.Text.Json;
using System.Net.Http.Headers;
using Ivy.Auth.Clerk.ApiClient.Responses;

namespace Ivy.Auth.Clerk.ApiClient;

public record ClerkNewClientResponse(
    string ClientToken,
    ClerkClientResponse ClientResponse
);

public class FrontendApiClient(string? frontendApiDomain)
{
    private readonly string? _frontendApiDomain = frontendApiDomain;
    private readonly HttpClient _httpClient = new();
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

    private const string ApiVersion = "2025-11-10";

    public async Task<ClerkDevBrowserTokenResponse> CreateDevBrowserTokenAsync(CancellationToken cancellationToken = default)
    {
        var response = await RequestAsync(HttpMethod.Post, $"dev_browser", cancellationToken: cancellationToken);
        return await ParseResponseAsync<ClerkDevBrowserTokenResponse>(response);
    }

    public async Task<ClerkEnvironmentResponse> GetEnvironmentAsync(CancellationToken cancellationToken = default)
    {
        var response = await RequestAsync(
            HttpMethod.Get,
            "environment",
            cancellationToken: cancellationToken);
        return await ParseResponseAsync<ClerkEnvironmentResponse>(response);
    }

    public async Task<ClerkEnvironmentResponse> UpdateEnvironmentAsync(ClerkApiClientToken token, string origin, CancellationToken cancellationToken = default)
    {
        var response = await AuthenticatedRequestAsync(
            HttpMethod.Post,
            "environment",
            token,
            "_method=PATCH",
            headers => headers.Add("Origin", origin),
            cancellationToken: cancellationToken);
        return await ParseResponseAsync<ClerkEnvironmentResponse>(response);
    }

    public async Task<ClerkClientResponse> GetCurrentClientAsync(ClerkApiClientToken token, CancellationToken cancellationToken = default)
    {
        var response = await AuthenticatedRequestAsync(
            HttpMethod.Get,
            "client",
            token,
            cancellationToken: cancellationToken);
        return await ParseResponseAsync<ClerkClientResponse>(response);
    }

    public async Task<ClerkNewClientResponse> CreateNewClientAsync(CancellationToken cancellationToken = default)
    {
        var response = await RequestAsync(
            HttpMethod.Post,
            "client",
            cancellationToken: cancellationToken);
        string? clientToken = null;
        if (response.Headers.TryGetValues("Authorization", out var locations))
        {
            clientToken = locations.FirstOrDefault();
        }
        if (string.IsNullOrEmpty(clientToken))
        {
            throw new ClerkException("Clerk did not return a client token in the Authorization header.");
        }

        var clientResponse = await ParseResponseAsync<ClerkClientResponse>(response);

        return new ClerkNewClientResponse(clientToken, clientResponse);
    }

    public async Task<ClerkTokenResponse> CreateSessionTokenAsync(string sessionId, ClerkApiClientToken token, CancellationToken cancellationToken = default)
    {
        var content = new StringContent("organization_id")
        {
            Headers = { ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded") },
        };

        var response = await AuthenticatedRequestAsync(HttpMethod.Post, $"client/sessions/{sessionId}/tokens", token, content: content, cancellationToken: cancellationToken);
        return await ParseResponseAsync<ClerkTokenResponse>(response);
    }

    public async Task<ClerkSessionResponse> TouchSessionAsync(string sessionId, ClerkApiSessionToken token, CancellationToken cancellationToken = default)
    {
        var response = await AuthenticatedRequestAsync(HttpMethod.Post, $"client/sessions/{sessionId}/touch", token, cancellationToken: cancellationToken);
        return await ParseResponseAsync<ClerkSessionResponse>(response);
    }

    public async Task<ClerkSessionResponse> GetSessionAsync(string sessionId, ClerkApiSessionToken token, CancellationToken cancellationToken = default)
    {
        var response = await AuthenticatedRequestAsync(HttpMethod.Get, $"client/sessions/{sessionId}", token, cancellationToken: cancellationToken);
        return await ParseResponseAsync<ClerkSessionResponse>(response);
    }

    public async Task<ClerkSessionResponse> EndSessionAsync(string sessionId, ClerkApiSessionToken token, CancellationToken cancellationToken = default)
    {
        var response = await AuthenticatedRequestAsync(HttpMethod.Post, $"client/sessions/{sessionId}/end", token, cancellationToken: cancellationToken);
        return await ParseResponseAsync<ClerkSessionResponse>(response);
    }

    public async Task<ClerkClientResponse> RemoveAllSessionsAsync(ClerkApiClientToken token, CancellationToken cancellationToken = default)
    {
        var response = await AuthenticatedRequestAsync(HttpMethod.Delete, "client/sessions", token, cancellationToken: cancellationToken);
        return await ParseResponseAsync<ClerkClientResponse>(response);
    }

    public async Task<ClerkSignInResponse> CreateSignInAsync(ClerkApiClientToken token, string origin, string strategy, string redirectUrl, string? actionCompleteRedirectUrl, CancellationToken cancellationToken = default)
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

        var response = await AuthenticatedRequestAsync(HttpMethod.Post, "client/sign_ins", token, setHeaders: headers => headers.Add("Origin", origin), content: content, cancellationToken: cancellationToken);
        return await ParseResponseAsync<ClerkSignInResponse>(response);
    }

    public async Task<ClerkSignInResponse> PrepareFirstFactorVerificationAsync(ClerkApiClientToken token, string origin, string signInId, string strategy, string redirectUrl, string? actionCompleteRedirectUrl, CancellationToken cancellationToken = default)
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

        var response = await AuthenticatedRequestAsync(HttpMethod.Post, $"client/sign_ins/{signInId}/prepare_first_factor", token, setHeaders: headers => headers.Add("Origin", origin), content: content, cancellationToken: cancellationToken);
        return await ParseResponseAsync<ClerkSignInResponse>(response);
    }

    private async Task<HttpResponseMessage> AuthenticatedRequestAsync(HttpMethod method, string endpoint, ClerkApiClientToken token, string? additionalQueryParameters = null, Action<HttpRequestHeaders>? setHeaders = null, HttpContent? content = null, CancellationToken cancellationToken = default)
    {
        if (token.TokenType == ClerkApiClientTokenType.DevBrowser)
        {
            additionalQueryParameters = string.IsNullOrEmpty(additionalQueryParameters)
                ? $"__clerk_db_jwt={token.Token}"
                : $"{additionalQueryParameters}&__clerk_db_jwt={token.Token}";
        }
        return await RequestAsync(
            method,
            endpoint,
            additionalQueryParameters,
            headers =>
            {
                setHeaders?.Invoke(headers);
                if (token.TokenType == ClerkApiClientTokenType.ClientJwt)
                {
                    headers.Add("Authorization", token.Token);
                }
            },
            content,
            cancellationToken);
    }

    private async Task<HttpResponseMessage> AuthenticatedRequestAsync(HttpMethod method, string endpoint, ClerkApiSessionToken token, string? additionalQueryParameters = null, Action<HttpRequestHeaders>? setHeaders = null, HttpContent? content = null, CancellationToken cancellationToken = default)
    {
        if (token.TokenType == ClerkApiSessionTokenType.DevBrowser)
        {
            additionalQueryParameters = string.IsNullOrEmpty(additionalQueryParameters)
                ? $"__clerk_db_jwt={token.Token}"
                : $"{additionalQueryParameters}&__clerk_db_jwt={token.Token}";
        }
        return await RequestAsync(
            method,
            endpoint,
            additionalQueryParameters,
            headers =>
            {
                setHeaders?.Invoke(headers);
                if (token.TokenType == ClerkApiSessionTokenType.SessionJwt)
                {
                    headers.Add("__session", token.Token);
                }
            },
            content,
            cancellationToken);
    }

    private async Task<HttpResponseMessage> RequestAsync(HttpMethod method, string endpoint, string? additionalQueryParameters = null, Action<HttpRequestHeaders>? setHeaders = null, HttpContent? content = null, CancellationToken cancellationToken = default)
    {
        var requestUrl = ConstructBaseRequestUrl(endpoint);
        if (!string.IsNullOrEmpty(additionalQueryParameters))
        {
            requestUrl += $"&{additionalQueryParameters}";
        }

        var request = new HttpRequestMessage(method, requestUrl)
        {
            Content = content
        };

        setHeaders?.Invoke(request.Headers);

        return await _httpClient.SendAsync(request, cancellationToken);
    }

    private string ConstructBaseRequestUrl(string endpoint) => $"https://{_frontendApiDomain}/v1/{endpoint}?__clerk_api_version={ApiVersion}";

    private async Task<string> ProcessResponseAsync(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();

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

    private async Task<T> ParseResponseAsync<T>(HttpResponseMessage response)
    {
        var json = await ProcessResponseAsync(response);

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
