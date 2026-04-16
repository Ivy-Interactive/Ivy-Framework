# IAuthProvider LoginAsync Return Type Change

## Summary

The `LoginAsync` method in the `IAuthProvider` interface has been updated to return `Task<LoginResult>` instead of `Task<AuthToken?>`. This change allows auth providers to communicate specific failure reasons (like invalid credentials or rate limiting) to the caller.

## What Changed

### Before
```csharp
public interface IAuthProvider : IAuthTokenHandler
{
    Task<AuthToken?> LoginAsync(IAuthSession authSession, string email, string password, CancellationToken cancellationToken = default);
}
```

### After
```csharp
public interface IAuthProvider : IAuthTokenHandler
{
    Task<LoginResult> LoginAsync(IAuthSession authSession, string email, string password, CancellationToken cancellationToken = default);
}
```

## How to Find Affected Code

Run `dotnet build`. Any implementation of `IAuthProvider` that has not been updated will fail with compiler error `CS0738` or `CS0535`.

## How to Migrate

Update your `LoginAsync` implementation to return a `LoginResult`.

### Example Migration

#### Before
```csharp
public Task<AuthToken?> LoginAsync(IAuthSession authSession, string email, string password, CancellationToken cancellationToken)
{
    var token = await MyAuthApi.Login(email, password);
    return token;
}
```

#### After
```csharp
public async Task<LoginResult> LoginAsync(IAuthSession authSession, string email, string password, CancellationToken cancellationToken)
{
    var token = await MyAuthApi.Login(email, password);
    if (token == null)
    {
        return LoginResult.InvalidCredentials();
    }
    return LoginResult.Success(token);
}
```

Use `LoginResult.Success(token)`, `LoginResult.InvalidCredentials()`, or `LoginResult.RateLimited(TimeSpan)` as appropriate.
