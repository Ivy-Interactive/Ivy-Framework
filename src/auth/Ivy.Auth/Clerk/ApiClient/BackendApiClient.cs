using System.Net.Http.Headers;
using System.Text.Json;
using Ivy.Auth.Clerk.ApiClient.Models;
using Ivy.Auth.Clerk.ApiClient.Responses;

namespace Ivy.Auth.Clerk.ApiClient;

public class BackendApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _secretKey;

    public BackendApiClient(string secretKey)
    {
        _secretKey = secretKey;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://api.clerk.com/")
        };
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", secretKey);
    }

    public async Task<ClerkUser?> GetUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"v1/users/{userId}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<ClerkUser>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<ClerkOAuthAccessTokenResponse?> GetOAuthAccessTokenAsync(
        string userId,
        string provider,
        CancellationToken cancellationToken = default)
    {
        var url = $"v1/users/{userId}/oauth_access_tokens/{provider}";
        var response = await _httpClient.GetAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        // Clerk returns an array of tokens, we take the first one
        var results = JsonSerializer.Deserialize<ClerkOAuthAccessTokenResponse[]>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return results?.FirstOrDefault();
    }
}
