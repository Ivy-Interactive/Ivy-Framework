---
title: Claude (Anthropic)
searchHints:
  - claude
  - anthropic
  - authentication
  - oauth
  - pkce
  - claude.ai
---
# Claude (Anthropic) authentication provider

<Ingress>
Sign in to your Ivy application with Anthropic Claude using OAuth 2.0 and PKCE—the same style of browser login used by Claude Code and Claude on the web.
</Ingress>

## Overview

The Claude provider uses Anthropic’s OAuth endpoints to authenticate users with a **Claude.ai** account. The flow is an **authorization code** grant with **PKCE** (no static client secret required for public clients). Ivy exchanges the authorization code at the token endpoint and stores access (and optional refresh) tokens in the Ivy auth session.

> **Note:** You must register an OAuth client in the [Anthropic Console](https://console.anthropic.com/) and add your Ivy app’s callback URL. Exact console steps may change; follow Anthropic’s current documentation for creating an OAuth application and allowed redirect URIs.

## Configuration

### 1. Register your OAuth client

In the Anthropic Console, create an OAuth application and set the **redirect URI** to your Ivy auth callback, for example:

`https://localhost:5010/ivy/auth/callback`

Use the same scheme, host, port, and path that your app serves (HTTPS in production).

### 2. Install the package

```terminal
dotnet add package Ivy.Auth.Claude
```

### 3. Enable the provider

```csharp
using Ivy.Auth.Claude;

var server = new Server();

server.UseAuth<ClaudeAuthProvider>();

await server.RunAsync();
```

### 4. Secrets or environment variables

Configure these keys via [.NET user secrets](../../02_Concepts/14_Secrets.md) or environment variables.

| Key | Required | Description |
|-----|----------|-------------|
| **Claude:ClientId** | Yes | OAuth client ID from the Anthropic Console |
| **Claude:RedirectUri** | Yes | Must match the registered redirect URI (for example `https://localhost:5010/ivy/auth/callback`) |
| **Claude:ClientSecret** | No | Use if Anthropic issued a confidential client secret |
| **Claude:AuthorizationUrl** | No | Default: `https://claude.ai/oauth/authorize` |
| **Claude:TokenUrl** | No | Default: `https://console.anthropic.com/v1/oauth/token` |
| **Claude:Scope** | No | Default: `user:profile user:inference` |
| **Claude:UserInfoUrl** | No | Default: `https://api.anthropic.com/api/oauth/claude_cli/client_data` (profile JSON for `GetUserInfoAsync`; Anthropic may change this API) |
| **Claude:UserAgent** | Optional | Overrides the `User-Agent` header on HTTP calls (defaults to Ivy’s version string) |

**User secrets (development):**

```terminal
dotnet user-secrets set "Claude:ClientId" "your_client_id"
dotnet user-secrets set "Claude:RedirectUri" "https://localhost:5010/ivy/auth/callback"
```

If your client has a secret:

```terminal
dotnet user-secrets set "Claude:ClientSecret" "your_client_secret"
```

**Environment variables (production):** use double underscores, for example `Claude__ClientId`, `Claude__RedirectUri`.

As elsewhere in Ivy, values in user secrets take precedence over environment variables when both are set.

## Authentication flow

1. The user chooses **Claude** on the Ivy login screen.
2. Ivy opens the authorize URL with PKCE (`code_challenge` / `code_verifier`).
3. The user signs in on Anthropic and approves the app.
4. Anthropic redirects to `/ivy/auth/callback` with an authorization `code`.
5. Ivy exchanges the code at the token endpoint (JSON body, PKCE verifier).
6. Ivy resolves the user profile for `IAuthService` using the configured user-info URL where possible.

## Brokered sessions and token handler

The package includes `ClaudeAuthTokenHandler` registered for `OAuthProviders.Claude`, so brokered account tooling can refresh and validate Claude OAuth tokens when a refresh token is present.

## Troubleshooting

- **Redirect URI mismatch:** The value of **Claude:RedirectUri** must exactly match a redirect URI allowed for your OAuth client (including trailing slashes, `http` vs `https`, and port).
- **PKCE errors:** The code verifier is kept in memory for the login request. If the server restarts between starting OAuth and completing the callback, start sign-in again.
- **Profile or user info:** If `Claude:UserInfoUrl` returns 404 or a new JSON shape, set **Claude:UserInfoUrl** to the URL Anthropic documents for profile access, or rely on Ivy’s fallback identity until you adjust the URL.

## Related documentation

- [Authentication overview](01_AuthenticationOverview.md)
- [GitHub authentication](02_GitHub.md) (similar manual OAuth setup)
- [Sliplane authentication](02_Sliplane.md) (OAuth code flow reference)
