# Ivy.Auth

Authentication providers for [Ivy Framework](https://github.com/Ivy-Interactive/Ivy-Framework).

This single package contains every first-party Ivy auth provider. Each provider keeps its own
namespace, so existing code continues to compile unchanged after switching from the per-provider
packages (`Ivy.Auth.Auth0`, `Ivy.Auth.Supabase`, ...) to `Ivy.Auth`.

```bash
dotnet add package Ivy.Auth
```

| Provider | Namespace | Type |
| --- | --- | --- |
| Auth0 | `Ivy.Auth.Auth0` | `Auth0AuthProvider` |
| Authelia | `Ivy.Auth.Authelia` | `AutheliaAuthProvider` |
| Clerk | `Ivy.Auth.Clerk` | `ClerkAuthProvider` |
| GitHub | `Ivy.Auth.GitHub` | `GitHubAuthProvider` |
| Google | `Ivy.Auth.Google` | `GoogleAuthTokenHandler` |
| Microsoft Entra | `Ivy.Auth.MicrosoftEntra` | `MicrosoftEntraAuthProvider` |
| Sliplane | `Ivy.Auth.Sliplane` | `SliplaneAuthProvider` |
| Supabase | `Ivy.Auth.Supabase` | `SupabaseAuthProvider` |

Setup instructions for each provider are in the
[Ivy Authentication documentation](https://docs.ivy.app/onboarding/cli/authentication/authentication-overview).
