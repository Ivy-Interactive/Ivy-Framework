# Auth Login

A button that performs custom authentication for an API resource. In Retool this is a dedicated component that connects to a pre-configured resource with custom authentication. In Ivy, authentication is handled through the `IAuthService` / `IAuthProvider` services combined with standard UI widgets (buttons, forms).

## Retool

```toolscript
// Auth Login component wired to a custom-auth API resource
authLogin1.authResourceName = "myApiResource"
authLogin1.label = "Sign In"
authLogin1.hidden = false
```

## Ivy

```csharp
// Using IAuthService with a form for email/password login
var auth = UseService<IAuthService>();
var credentials = UseState(() => new LoginModel("", ""));

UseEffect(async () =>
{
    if (!string.IsNullOrEmpty(credentials.Value.Email))
    {
        await auth.LoginAsync(credentials.Value.Email, credentials.Value.Password);
    }
}, credentials);

return credentials.ToForm("Sign In")
    .Required(m => m.Email, m => m.Password);

// Or using a simple button for OAuth redirect flows
// configured in Program.cs:
// server.UseAuth<Auth0AuthProvider>(c => c.UseGoogle().UseApple());
```

## Parameters

| Parameter              | Documentation                                        | Ivy                                                                 |
|------------------------|------------------------------------------------------|---------------------------------------------------------------------|
| authResourceName       | The resource with which to authenticate               | Configured in `Program.cs` via `server.UseAuth<TProvider>()`        |
| authType               | The type of authentication to use (read-only)         | Determined by the chosen provider (Auth0, Basic, Clerk, etc.)       |
| label                  | The text label to display                             | `new Button("Sign In")` or `.ToForm("Sign In")`                    |
| hidden                 | Determines visibility of the component                | Not directly equivalent; use conditional rendering                  |
| id                     | The unique identifier (name)                          | Variable name / component identity                                  |
| isHiddenOnDesktop      | Controls desktop layout visibility                    | Not supported                                                       |
| isHiddenOnMobile       | Controls mobile layout visibility                     | Not supported                                                       |
| maintainSpaceWhenHidden| Reserves layout space when hidden                     | Not supported                                                       |
| margin                 | External spacing around component                     | Styling via layout widgets (e.g., `Spacer`, `Box`)                 |
| showInEditor           | Visibility in editor mode                             | Not supported (no editor mode)                                      |
| style                  | Custom styling options                                | Fluent API styling (`.Success()`, `.Variant()`, etc.)               |
| scrollIntoView()       | Scrolls the component into the visible area           | Not supported                                                       |
