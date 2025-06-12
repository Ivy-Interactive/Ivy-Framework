# Authelia Auth Provider

{description}
The `AutheliaAuthProvider` class provides authentication integration with Authelia, a powerful authentication and authorization server. This provider enables your Ivy applications to authenticate users through Authelia's web-based login system with automatic session management and user information retrieval.
{/description}

{overview}
Authelia is an open-source authentication and authorization server providing two-factor authentication and single sign-on (SSO) for your applications. The `AutheliaAuthProvider` integrates with Authelia's REST API to handle user authentication, session management, and user information retrieval.
{/overview}

## Key Features

{feature-list}
- Username/Password Authentication: Supports traditional email and password login flows
- Session Management: Handles Authelia session cookies automatically  
- User Information Retrieval: Fetches user details from Authelia's user info endpoint
- Automatic Cookie Handling: Manages authentication cookies transparently
- Configuration-Based Setup: Uses environment variables and user secrets for configuration
{/feature-list}

## Installation

{installation}
First, install the Authelia auth provider package:

```bash
dotnet add package Ivy.Auth.Authelia
```
{/installation}

## Configuration

{config-section}
The `AutheliaAuthProvider` requires configuration through environment variables or user secrets. The provider automatically loads configuration using .NET's configuration system.

### Required Settings

- **`AUTHELIA_URL`**: The base URL of your Authelia server (e.g., `https://auth.example.com`)

### Environment Variables
```bash
export AUTHELIA_URL=https://auth.example.com
```

### User Secrets (Development)
```bash
dotnet user-secrets set "AUTHELIA_URL" "https://auth.example.com"
```

### appsettings.json
```json
{
  "AUTHELIA_URL": "https://auth.example.com"
}
```
{/config-section}

## Basic Usage

{usage-section}
### Registering the Provider

Register the `AutheliaAuthProvider` in your Ivy application:

{code-example}
// In your application startup
services.AddScoped<IAuthProvider, AutheliaAuthProvider>();
{/code-example}

### Authentication Flow

The provider supports email/password authentication through Authelia's first-factor endpoint:

{interactive-example}
var authProvider = new AutheliaAuthProvider();

// Authenticate user
string? token = await authProvider.LoginAsync("user@example.com", "password");

if (token != null)
{
    // Authentication successful - token contains the session ID
    Console.WriteLine("User authenticated successfully");
}
else
{
    // Authentication failed
    Console.WriteLine("Invalid credentials");
}
{/interactive-example}
{/usage-section}

## API Reference

{api-section}
### Constructor

{method-signature}
public AutheliaAuthProvider()
{/method-signature}

{method-description}
Creates a new instance of the Authelia auth provider. Automatically configures the HTTP client with the Authelia base URL from configuration.

**Throws:** `Exception` when `AUTHELIA_URL` configuration is not provided
{/method-description}

### LoginAsync

{method-signature}
public async Task<string?> LoginAsync(string username, string password)
{/method-signature}

{method-description}
Authenticates a user with Authelia using username and password.

**Parameters:**
- `username`: The user's email address or username
- `password`: The user's password

**Returns:** The Authelia session token if authentication succeeds, `null` if it fails
{/method-description}

{code-example}
string? token = await provider.LoginAsync("john.doe@example.com", "mypassword");
if (token != null)
{
    // User authenticated successfully
    Console.WriteLine($"Session token: {token}");
}
{/code-example}

### ValidateJwtAsync

{method-signature}
public async Task<bool> ValidateJwtAsync(string jwt)
{/method-signature}

{method-description}
Validates whether a session token is still valid by checking with Authelia.

**Parameters:**
- `jwt`: The session token to validate

**Returns:** `true` if the session is valid, `false` otherwise
{/method-description}

{code-example}
bool isValid = await provider.ValidateJwtAsync(sessionToken);
if (isValid)
{
    Console.WriteLine("Session is still active");
}
else
{
    Console.WriteLine("Session expired - user needs to login again");
}
{/code-example}

### GetUserInfoAsync

{method-signature}
public async Task<UserInfo?> GetUserInfoAsync(string jwt)
{/method-signature}

{method-description}
Retrieves user information from Authelia for the given session.

**Parameters:**
- `jwt`: The session token

**Returns:** `UserInfo` object if successful, `null` if the session is invalid
{/method-description}

{code-example}
UserInfo? user = await provider.GetUserInfoAsync(sessionToken);
if (user != null)
{
    Console.WriteLine($"User ID: {user.Id}");
    Console.WriteLine($"Email: {user.Email}");
    Console.WriteLine($"Display Name: {user.DisplayName}");
}
{/code-example}

### LogoutAsync

{method-signature}
public async Task LogoutAsync(string token)
{/method-signature}

{method-description}
Logs out the user and invalidates their Authelia session.

**Parameters:**
- `token`: The session token (currently unused, but maintained for interface compliance)
{/method-description}

{code-example}
await provider.LogoutAsync(sessionToken);
Console.WriteLine("User logged out successfully");
{/code-example}

### GetAuthOptions

{method-signature}
public AuthOption[] GetAuthOptions()
{/method-signature}

{method-description}
Returns the supported authentication options for this provider.

**Returns:** Array containing email/password authentication option
{/method-description}
{/api-section}

## Integration with Ivy Framework

{ivy-integration}
### Using in Ivy Components

Here's how to use the auth provider within your Ivy components:

{interactive-example}
public class LoginComponent : Component
{
    [Inject] private IAuthProvider AuthProvider { get; set; } = null!;
    
    private string email = "";
    private string password = "";
    private string errorMessage = "";
    
    public override void Render()
    {
        Heading("Login", 2);
        
        TextInput("Email", email, value => email = value);
        PasswordInput("Password", password, value => password = value);
        
        if (!string.IsNullOrEmpty(errorMessage))
        {
            Alert(errorMessage, AlertType.Error);
        }
        
        Button("Login", async () => await HandleLogin());
    }
    
    private async Task HandleLogin()
    {
        string? token = await AuthProvider.LoginAsync(email, password);
        if (token != null)
        {
            // Store token and redirect to dashboard
            StateManager.SetAuthToken(token);
            Navigate("/dashboard");
        }
        else
        {
            errorMessage = "Invalid credentials";
            StateHasChanged();
        }
    }
}
{/interactive-example}

### Protected Routes

Use the auth provider to protect routes in your Ivy application:

{code-example}
public class AuthenticatedRoute : Component
{
    [Inject] private IAuthProvider AuthProvider { get; set; } = null!;
    
    public override async Task OnInitAsync()
    {
        var token = StateManager.GetAuthToken();
        if (token == null || !await AuthProvider.ValidateJwtAsync(token))
        {
            Navigate("/login");
            return;
        }
        
        // Load user information
        var userInfo = await AuthProvider.GetUserInfoAsync(token);
        StateManager.SetCurrentUser(userInfo);
    }
}
{/code-example}
{/ivy-integration}

## Data Models

{models-section}
### AutheliaUser

Internal model representing user data from Authelia:

{type-reference}
public class AutheliaUser
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}
{/type-reference}

This class is used internally by the provider to deserialize JSON responses from Authelia's user info endpoint. The data is then mapped to the framework's `UserInfo` class.
{/models-section}

## Error Handling

{error-handling}
The provider handles various error scenarios gracefully:

### Configuration Errors

{code-example}
try
{
    var provider = new AutheliaAuthProvider();
}
catch (Exception ex)
{
    // Handle missing AUTHELIA_URL configuration
    Console.WriteLine($"Configuration error: {ex.Message}");
    // Log error and provide fallback or setup instructions
}
{/code-example}

### Authentication Failures

{code-example}
string? token = await provider.LoginAsync(username, password);
if (token == null)
{
    // Handle authentication failure
    // Could be invalid credentials, server unavailable, or network issues
    ShowErrorMessage("Login failed. Please check your credentials and try again.");
}
{/code-example}

### Session Validation

{code-example}
bool isValid = await provider.ValidateJwtAsync(token);
if (!isValid)
{
    // Handle invalid or expired session
    // Clear stored token and redirect to login
    StateManager.ClearAuthToken();
    Navigate("/login");
}
{/code-example}
{/error-handling}

## Security Considerations

{security-section}
### HTTPS Requirements

{warning}
Always use HTTPS for your Authelia server URL in production. HTTP connections expose authentication cookies and session tokens to potential interception.
{/warning}

{code-example}
// Good - Secure HTTPS connection
AUTHELIA_URL=https://auth.example.com

// Bad - Never use HTTP in production
// AUTHELIA_URL=http://auth.example.com
{/code-example}

### Token Storage

Store authentication tokens securely in your Ivy applications:

- Use secure, HTTP-only cookies when possible
- Implement proper token expiration handling
- Clear tokens completely on logout
- Never store tokens in localStorage or sessionStorage in production

### Error Message Security

Avoid exposing sensitive information in error messages:

{code-example}
// Good - Generic error message
if (token == null)
{
    return "Authentication failed";
}

// Bad - Exposes internal server details
if (token == null)
{
    return $"Authelia server at {_baseUrl} rejected credentials";
}
{/code-example}
{/security-section}

## Troubleshooting

{troubleshooting}
### Common Issues

**Configuration Not Found**
```
Exception: AUTHELIA_URL is required
```

{solution}
- Ensure `AUTHELIA_URL` is set in environment variables or user secrets
- Verify the configuration key matches exactly (case-sensitive)
- Check that the configuration is accessible to your application context
{/solution}

**Connection Issues**

Test connectivity to your Authelia server:

{code-example}
// Add this to your startup or health check
try
{
    using var httpClient = new HttpClient();
    var response = await httpClient.GetAsync($"{autheliaUrl}/api/health");
    Console.WriteLine($"Authelia health check: {response.StatusCode}");
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"Cannot reach Authelia server: {ex.Message}");
}
{/code-example}

**Authentication Always Fails**

{checklist}
- Verify Authelia server configuration allows your application's origin
- Check if two-factor authentication is required (not currently supported by this provider)
- Ensure user credentials are correct in Authelia's user database
- Verify network connectivity and firewall settings
- Check Authelia server logs for authentication attempts
{/checklist}
{/troubleshooting}

## Limitations and Future Enhancements

{limitations}
### Current Limitations

- **OAuth/OIDC Support**: OAuth integration methods are not implemented (`GetOAuthUriAsync`, `HandleOAuthCallback`)
- **Two-Factor Authentication**: Second-factor authentication flows are not supported
- **Token Refresh**: No automatic token refresh mechanism
- **Multi-Domain Support**: Limited to single Authelia instance configuration

### Planned Enhancements

Future versions may include:

- Complete OAuth/OIDC flow implementation
- Support for Authelia's second-factor authentication
- Automatic token refresh and renewal
- Multi-tenant Authelia configuration support
- Enhanced error handling and detailed logging
- Integration with Authelia's group-based authorization
{/limitations}

## Related Documentation

{related-links}
- [Authentication Overview](./auth-overview)
- [IAuthProvider Interface](./iauth-provider)
- [User Management](./user-management)
- [Security Best Practices](./security)
{/related-links}

{external-links}
- [Authelia Official Documentation](https://www.authelia.com/)
- [Authelia API Reference](https://www.authelia.com/reference/api/)
- [Authelia Configuration Guide](https://www.authelia.com/configuration/prologue/introduction/)
{/external-links}