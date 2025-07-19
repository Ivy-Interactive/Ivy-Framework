# Ivy.Auth.Auth0

An Ivy authentication provider for Auth0.

## Configuration

Add the following environment variables or user secrets:

```
AUTH0_DOMAIN=your-domain.auth0.com
AUTH0_CLIENT_ID=your-client-id
AUTH0_CLIENT_SECRET=your-client-secret
AUTH0_AUDIENCE=your-api-audience (optional)
```

## Usage

```csharp
var authProvider = new Auth0AuthProvider()
    .UseEmailPassword()
    .UseGoogle()
    .UseGithub()
    .UseMicrosoft();
```

## Supported Authentication Methods

- Email/Password
- Google OAuth
- GitHub OAuth
- LinkedIn OAuth
- Twitter OAuth
- Facebook OAuth
- Microsoft OAuth