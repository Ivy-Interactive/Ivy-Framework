using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ivy.Auth;
using Ivy.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Okta.Sdk;
using Okta.Sdk.Configuration;

namespace Ivy.Auth.Okta;

public class OktaAuthException(string message) : Exception(message);

public class OktaAuthProvider : IAuthProvider
{
    private readonly OktaClient _oktaClient;
    private readonly string _oktaDomain;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _audience;
    private readonly List<AuthOption> _authOptions = new();
    private readonly Dictionary<string, string> _oauthStates = new();

    public OktaAuthProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets(Assembly.GetEntryAssembly()!)
            .Build();

        _oktaDomain = configuration.GetValue<string>("OKTA_DOMAIN") ?? throw new Exception("OKTA_DOMAIN is required");
        _clientId = configuration.GetValue<string>("OKTA_CLIENT_ID") ?? throw new Exception("OKTA_CLIENT_ID is required");
        _clientSecret = configuration.GetValue<string>("OKTA_CLIENT_SECRET") ?? throw new Exception("OKTA_CLIENT_SECRET is required");
        _audience = configuration.GetValue<string>("OKTA_AUDIENCE") ?? "api://default";

        var oktaConfig = new OktaClientConfiguration
        {
            OktaDomain = _oktaDomain,
            Token = configuration.GetValue<string>("OKTA_API_TOKEN") ?? throw new Exception("OKTA_API_TOKEN is required")
        };

        _oktaClient = new OktaClient(oktaConfig);
    }

    public async Task<AuthToken?> LoginAsync(string email, string password)
    {
        try
        {
            var authUrl = $"https://{_oktaDomain}/oauth2/default/v1/token";
            using var client = new HttpClient();
            
            var parameters = new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["username"] = email,
                ["password"] = password,
                ["client_id"] = _clientId,
                ["client_secret"] = _clientSecret,
                ["scope"] = "openid email profile"
            };

            var content = new FormUrlEncodedContent(parameters);
            var response = await client.PostAsync(authUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                throw new OktaAuthException("Login failed: Invalid credentials");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<OktaTokenResponse>(responseContent);

            if (tokenResponse?.AccessToken == null)
            {
                throw new OktaAuthException("Login failed: No access token returned");
            }

            return new AuthToken(
                tokenResponse.AccessToken,
                tokenResponse.RefreshToken,
                tokenResponse.ExpiresIn.HasValue ? DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn.Value) : null
            );
        }
        catch (Exception ex)
        {
            throw new OktaAuthException($"Login failed: {ex.Message}");
        }
    }

    public async Task<Uri> GetOAuthUriAsync(AuthOption option, Uri callbackUri)
    {
        var state = Guid.NewGuid().ToString();
        _oauthStates[state] = option.Id ?? "";

        var baseUrl = $"https://{_oktaDomain}/oauth2/default/v1/authorize";
        var parameters = new Dictionary<string, string>
        {
            ["client_id"] = _clientId,
            ["response_type"] = "code",
            ["scope"] = "openid email profile",
            ["redirect_uri"] = callbackUri.ToString(),
            ["state"] = state
        };

        var queryString = string.Join("&", parameters.Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value)}"));
        return new Uri($"{baseUrl}?{queryString}");
    }

    public async Task<AuthToken?> HandleOAuthCallbackAsync(HttpRequest request)
    {
        var code = request.Query["code"].ToString();
        var state = request.Query["state"].ToString();
        var error = request.Query["error"].ToString();

        if (!string.IsNullOrEmpty(error))
        {
            throw new OktaAuthException($"OAuth error: {error}");
        }

        if (string.IsNullOrEmpty(code))
        {
            throw new OktaAuthException("No authorization code received");
        }

        if (string.IsNullOrEmpty(state) || !_oauthStates.ContainsKey(state))
        {
            throw new OktaAuthException("Invalid state parameter");
        }

        try
        {
            var tokenUrl = $"https://{_oktaDomain}/oauth2/default/v1/token";
            using var client = new HttpClient();

            var parameters = new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["code"] = code,
                ["redirect_uri"] = request.GetDisplayUrl().Split('?')[0],
                ["client_id"] = _clientId,
                ["client_secret"] = _clientSecret
            };

            var content = new FormUrlEncodedContent(parameters);
            var response = await client.PostAsync(tokenUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                throw new OktaAuthException("Failed to exchange authorization code for token");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<OktaTokenResponse>(responseContent);

            if (tokenResponse?.AccessToken == null)
            {
                throw new OktaAuthException("No access token returned");
            }

            _oauthStates.Remove(state);

            return new AuthToken(
                tokenResponse.AccessToken,
                tokenResponse.RefreshToken,
                tokenResponse.ExpiresIn.HasValue ? DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn.Value) : null
            );
        }
        catch (Exception ex)
        {
            throw new OktaAuthException($"Token exchange failed: {ex.Message}");
        }
    }

    public async Task LogoutAsync(string jwt)
    {
        try
        {
            var logoutUrl = $"https://{_oktaDomain}/oauth2/default/v1/logout";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);
            
            var parameters = new Dictionary<string, string>
            {
                ["client_id"] = _clientId
            };

            var content = new FormUrlEncodedContent(parameters);
            await client.PostAsync(logoutUrl, content);
        }
        catch (Exception)
        {
            // Logout failures are not critical
        }
    }

    public async Task<AuthToken?> RefreshJwtAsync(AuthToken jwt)
    {
        if (jwt.ExpiresAt == null || jwt.RefreshToken == null || DateTimeOffset.UtcNow < jwt.ExpiresAt)
        {
            return jwt;
        }

        try
        {
            var tokenUrl = $"https://{_oktaDomain}/oauth2/default/v1/token";
            using var client = new HttpClient();

            var parameters = new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = jwt.RefreshToken,
                ["client_id"] = _clientId,
                ["client_secret"] = _clientSecret
            };

            var content = new FormUrlEncodedContent(parameters);
            var response = await client.PostAsync(tokenUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<OktaTokenResponse>(responseContent);

            if (tokenResponse?.AccessToken == null)
            {
                return null;
            }

            return new AuthToken(
                tokenResponse.AccessToken,
                tokenResponse.RefreshToken ?? jwt.RefreshToken,
                tokenResponse.ExpiresIn.HasValue ? DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn.Value) : null
            );
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<bool> ValidateJwtAsync(string jwt)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jsonToken = tokenHandler.ReadJwtToken(jwt);
            
            // Check if token is expired
            if (jsonToken.ValidTo < DateTime.UtcNow)
            {
                return false;
            }

            // Validate with Okta's introspection endpoint
            var introspectUrl = $"https://{_oktaDomain}/oauth2/default/v1/introspect";
            using var client = new HttpClient();
            
            var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_clientId}:{_clientSecret}"));
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authValue);

            var parameters = new Dictionary<string, string>
            {
                ["token"] = jwt,
                ["token_type_hint"] = "access_token"
            };

            var content = new FormUrlEncodedContent(parameters);
            var response = await client.PostAsync(introspectUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var introspectResponse = JsonSerializer.Deserialize<OktaIntrospectResponse>(responseContent);

            return introspectResponse?.Active == true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<UserInfo?> GetUserInfoAsync(string jwt)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jsonToken = tokenHandler.ReadJwtToken(jwt);
            
            var userId = jsonToken.Claims.FirstOrDefault(x => x.Type == "sub")?.Value;
            var email = jsonToken.Claims.FirstOrDefault(x => x.Type == "email")?.Value;
            var fullName = jsonToken.Claims.FirstOrDefault(x => x.Type == "name")?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email))
            {
                return null;
            }

            // Optionally get additional user info from Okta API
            try
            {
                var user = await _oktaClient.Users.GetUserAsync(userId);
                return new UserInfo(
                    user.Id,
                    user.Profile.Email ?? email,
                    user.Profile.GetProperty<string>("displayName") ?? fullName ?? 
                    $"{user.Profile.FirstName} {user.Profile.LastName}".Trim(),
                    null // Avatar URL not typically available in Okta
                );
            }
            catch (Exception)
            {
                // If API call fails, use token claims
                return new UserInfo(userId, email, fullName, null);
            }
        }
        catch (Exception)
        {
            return null;
        }
    }

    public AuthOption[] GetAuthOptions()
    {
        return _authOptions.ToArray();
    }

    public OktaAuthProvider UseEmailPassword()
    {
        _authOptions.Add(new AuthOption(AuthFlow.EmailPassword));
        return this;
    }

    public OktaAuthProvider UseOAuth()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Okta", "okta", Icons.None));
        return this;
    }
}

public class OktaTokenResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }
    
    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }
    
    [JsonPropertyName("expires_in")]
    public int? ExpiresIn { get; set; }
    
    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; }
    
    [JsonPropertyName("scope")]
    public string? Scope { get; set; }
}

public class OktaIntrospectResponse
{
    [JsonPropertyName("active")]
    public bool Active { get; set; }
    
    [JsonPropertyName("client_id")]
    public string? ClientId { get; set; }
    
    [JsonPropertyName("username")]
    public string? Username { get; set; }
    
    [JsonPropertyName("scope")]
    public string? Scope { get; set; }
    
    [JsonPropertyName("exp")]
    public long? Exp { get; set; }
    
    [JsonPropertyName("iat")]
    public long? Iat { get; set; }
    
    [JsonPropertyName("sub")]
    public string? Sub { get; set; }
    
    [JsonPropertyName("aud")]
    public string? Aud { get; set; }
    
    [JsonPropertyName("iss")]
    public string? Iss { get; set; }
}