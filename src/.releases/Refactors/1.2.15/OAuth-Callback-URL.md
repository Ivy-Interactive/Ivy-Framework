# OAuth Callback URL Changed - v1.2.15

## Summary

The OAuth callback URL has changed from `/ivy/webhook` to `/ivy/auth/callback`. If you use OAuth authentication (Auth0, GitHub, Microsoft Entra, Supabase, Clerk), you must update the callback/redirect URL in your provider's settings.

## What Changed

### Before (v1.2.14 and earlier)

```
http://localhost:5010/ivy/webhook
```

### After (v1.2.15+)

```
http://localhost:5010/ivy/auth/callback
```

## How to Find Affected Code

This is a **configuration change**, not a code change. Search your OAuth provider dashboard settings.

You can also search your codebase for hardcoded webhook URLs:

```regex
/ivy/webhook
```

## How to Refactor

### Update your OAuth provider settings

| Provider | Setting to Update |
|----------|------------------|
| **Auth0** | Applications → Settings → Allowed Callback URLs |
| **GitHub** | OAuth Apps → Authorization callback URL |
| **Microsoft Entra** | App registrations → Authentication → Redirect URIs |
| **Supabase** | Authentication → URL Configuration → Redirect URLs |
| **Clerk** | No manual change needed (handled by SDK) |

### Example URL Updates

| Old URL | New URL |
|---------|---------|
| `http://localhost:5010/ivy/webhook` | `http://localhost:5010/ivy/auth/callback` |
| `https://myapp.com/ivy/webhook` | `https://myapp.com/ivy/auth/callback` |
| `https://myapp.com/ivy/webhook/*` | `https://myapp.com/ivy/auth/callback/*` |

## Verification

After updating your provider settings, test the OAuth login flow to confirm authentication completes successfully.
