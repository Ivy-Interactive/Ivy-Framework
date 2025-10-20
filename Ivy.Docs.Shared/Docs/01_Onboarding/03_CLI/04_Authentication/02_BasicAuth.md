---
searchHints:
  - basic-auth
  - authentication
  - username
  - password
  - simple
  - credentials
---

# Basic Authentication Provider

<Ingress>
Secure your Ivy application with simple username/password authentication.
</Ingress>

## Overview

Basic Authentication in Ivy provides a straightforward way to secure your application using username and password credentials. It's ideal for development, testing, and simple internal applications where you need quick authentication setup with minimal complexity.

## Adding Authentication

To set up Basic Authentication with Ivy, run the following command and choose `Basic` when asked to select an auth provider:

```terminal
>ivy auth add
```

You will be prompted to provide user credentials in the following format:

```text
user1:password1; user2:password2; ...
```

Your provided passwords will be hashed, salted and peppered using the argon2id algorithm, and stored securely in .NET user secrets, along with some necessary automatically-generated secret values. Ivy then finishes configuring your application automatically:

1. Adds `server.UseAuth<BasicAuthProvider>()` to your `Program.cs`.
2. Adds `Ivy.Auth` to your global usings.

### Advanced Configuration

#### Connection Strings

To skip the interactive prompts, you can provide configuration via a connection string:

```terminal
>ivy auth add --provider Basic --connection-string "BasicAuth:Users=\"user1:password1;user2:password2\""
```

For a list of connection string parameters, see [Configuration Parameters](#configuration-parameters) below.

#### Manual Configuration

When deploying an Ivy project without using `ivy deploy`, your local .NET user secrets are not automatically transferred. In that case, you can configure basic auth by setting environment variables or .NET user secrets. See [Configuration Parameters](#configuration-parameters) below.

> **Note:** If configuration is present in both .NET user secrets and environment variables, Ivy will use the values in **.NET user secrets over environment variables**.

For more information, see [Authentication Overview](Overview.md).

#### Configuration Parameters

The following parameters are supported via connection string, environment variables, or .NET user secrets:

- **BasicAuth:Users**: Required. A semicolon-separated list of `username:password` pairs. When provided to Ivy at runtime via an environment variable or .NET user secret, passwords in this parameter must have been hashed, salted and peppered using the argon2id algorithm. When provided to `ivy auth add` via interactive prompt or connection string, passwords are treated as plaintext. For this reason, it is recommended that you configure basic auth using `ivy auth add` first, then copy required secrets from `dotnet user-secrets list`.
- **BasicAuth:HashSecret**: Required. A custom secret pepper value for verifying hashed passwords. Must be a base64-encoded string that represents at least 256 bits (or 32 bytes) of information. If not explicitly provided to `ivy auth add`, a cryptographically secure pseudorandom value will be generated automatically.
- **BasicAuth:JwtSecret**: Required. A custom secret key for token generation. Must be a base64-encoded string that represents at least 256 bits (or 32 bytes) of information. If not explicitly provided to `ivy auth add`, a cryptographically secure pseudorandom key will be generated automatically.
- **BasicAuth:JwtIssuer**: Optional. Used as the issuer of generated tokens. Default value: `ivy`.
- **BasicAuth:JwtAudience**: Optional. Used as the audience of generated tokens. Default value: `ivy-app`.

## Authentication Flow

1. User submits credentials through your application
2. Server validates credentials against the stored user list
3. If valid, an authentication token is issued to the client
4. The client includes this token in subsequent requests
5. The server validates the token for each protected resource access

## Basic Auth-Specific Features

Key features of the Basic Auth provider:

- **Simple Configuration**: Quick setup with minimal parameters
- **In-memory Authentication**: Fast credential validation without external services
- **Token-based Security**: Stateless authentication using secure tokens
- **Multiple Users**: Support for multiple user credentials

## JWT Refresh Tokens

The `BasicAuthProvider` uses JWT-based refresh tokens for improved security. Access tokens expire after **15 minutes**, while refresh tokens are valid for **24 hours** and allow users to stay logged in for up to **365 days** as long as they are refreshed before expiring. This reduces vulnerability windows while maintaining user convenience through automatic session extension.

```csharp
var authProvider = UseService<IAuthProvider>();

// Login returns both access and refresh tokens
var authToken = await authProvider.LoginAsync(email, password);
// authToken.AccessToken - Access token (15 min expiry)
// authToken.RefreshToken - Refresh token (24 hour expiry, 365 day max age)

// When access token expires, refresh:
var newToken = await authProvider.RefreshAccessTokenAsync(authToken);
```

## Security Best Practices

- **Always use HTTPS** in production environments
- **Use strong passwords** for all user accounts
- **Rotate credentials** periodically
- **Consider upgrading** to a more robust authentication provider for production applications

## Troubleshooting

### Common Issues

**Authentication Failed**

- Verify user credentials are correctly formatted (`user:password;user2:password2`)
- Check that the correct username and password are being submitted
- Ensure credentials are properly stored in user secrets

**Missing Configuration**

- Verify the Basic Auth provider is properly configured in your `Program.cs`
- Check that user secrets or environment variables are set and accessible to your application

**Token Issues**

- Access tokens have a limited lifetime (15 minutes); however, refresh tokens (valid for 24 hours) automatically extend the session without requiring re-authentication.
- If a refresh token expires (24 hours of inactivity), or 365 days have passed since initial login, users will need to log in again with their credentials.

## Related Documentation

- [Authentication Overview](Overview.md)
- [Auth0 Provider](Auth0.md)
- [Microsoft Entra Provider](MicrosoftEntra.md)
