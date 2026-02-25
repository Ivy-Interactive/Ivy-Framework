# Ivy.Auth.Sliplane

Sliplane OAuth2 authentication provider for the [Ivy Framework](https://github.com/Ivy-Interactive/Ivy).

## Overview

This package implements the OAuth2 authorization code flow, allowing Ivy applications to authenticate users via Sliplane.

## OAuth Endpoints

| | URL |
|---|---|
| Authorization | `https://api.sliplane.io/web/oauth/authorize` |
| Token | `https://api.sliplane.io/web/oauth/token` |
| Validation API | `https://ctrl.sliplane.io/v0/projects` |

## Configuration

Set the following values using environment variables or .NET user secrets:

| Key | Required | Default |
|---|---|---|
| `Sliplane:ClientId` | Yes | — |
| `Sliplane:ClientSecret` | Yes | — |
| `Sliplane:AuthorizationUrl` | No | `https://api.sliplane.io/web/oauth/authorize` |
| `Sliplane:TokenUrl` | No | `https://api.sliplane.io/web/oauth/token` |
| `Sliplane:Scope` | No | `full` |

### .NET User Secrets (recommended for development)

```bash
dotnet user-secrets set "Sliplane:ClientId" "your_client_id"
dotnet user-secrets set "Sliplane:ClientSecret" "your_client_secret"
```

### Environment Variables

```powershell
# Windows (PowerShell)
$env:Sliplane__ClientId = "your_client_id"
$env:Sliplane__ClientSecret = "your_client_secret"
```

```bash
# Linux / macOS
export Sliplane__ClientId="your_client_id"
export Sliplane__ClientSecret="your_client_secret"
```

> **Note:** Use double underscore `__` to represent nested keys in environment variables.

## Usage

```csharp
// Program.cs
using Ivy.Auth.Sliplane;

var server = new Server();

server.UseAuth<SliplaneAuthProvider>();

await server.RunAsync();
```

The provider reads configuration automatically from environment variables and user secrets via the injected `IConfiguration`.

## OAuth Flow

1. User clicks the "Sliplane" login button
2. User is redirected to Sliplane's authorization URL
3. After authenticating, Sliplane redirects back with an authorization code
4. Ivy exchanges the code for an access token and optional refresh token
5. The token is stored in the session and used for subsequent API calls

## Notes

- Sliplane does not expose a user-info endpoint. A placeholder `UserInfo` is returned after token validation.
- Tokens are validated by calling `GET /v0/projects` on the Sliplane control API.
- Token lifetime is not embedded in the token — `GetAccessTokenLifetimeAsync` returns `null`.

## Security

- Always use HTTPS in production
- Store client secrets in environment variables or .NET user secrets — never in source code
- The `state` parameter is managed by Ivy's `WebhookEndpoint` to prevent CSRF attacks
