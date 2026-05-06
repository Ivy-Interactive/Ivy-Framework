# st.user

A read-only, dict-like object for accessing information about the current user. It provides identity claims from parsed OIDC tokens, authentication status, and exposed tokens for API authentication.

## Streamlit

```python
import streamlit as st

if st.user.is_logged_in:
    st.write(st.user.email)
    st.write(st.user.name)
    user_dict = st.user.to_dict()

    # Access exposed tokens (requires expose_tokens config)
    id_token = st.user.tokens["id"]
```

## Ivy

```csharp
var auth = UseService<IAuthService>();

var user = await auth.GetUserInfoAsync();
// user info (email, name, avatar) is extracted from JWT claims
// configured per auth provider (Auth0, Clerk, Entra, etc.)
```

## Parameters

| Parameter     | Documentation                                                                                           | Ivy                                                                  |
|---------------|---------------------------------------------------------------------------------------------------------|----------------------------------------------------------------------|
| is_logged_in  | Whether a user is logged in. Only available when authentication is configured in `secrets.toml`.         | Not a direct property; auth state is managed by `IAuthService`       |
| tokens        | Read-only dict-like object for accessing exposed identity provider tokens (`id` and `access`).          | Tokens are managed internally by the auth provider                   |
| to_dict()     | Returns user information as a dictionary (`Dict[str, str]`).                                            | `GetUserInfoAsync()` returns user info from JWT claims               |
| email         | User's email address (from OIDC claims).                                                                | Available via JWT custom claims per provider                         |
| name          | User's display name (from OIDC claims).                                                                 | Available via JWT custom claims per provider                         |
| picture       | User's avatar URL (from OIDC claims).                                                                   | Available as `avatar` via JWT custom claims per provider             |
