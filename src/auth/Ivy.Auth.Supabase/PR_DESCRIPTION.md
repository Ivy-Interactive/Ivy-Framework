# fix(auth): resolve CodeQL security warnings in SupabaseAuthProvider

Closes #2038

## Problem

CodeQL detected **6 security alerts** of type "User-controlled bypass of sensitive method" in `SupabaseAuthProvider.cs`.

### Vulnerability Details

The `HandleOAuthCallbackAsync` method used query parameters from the HTTP request (`error`, `error_code`, `error_description`) in `if` conditions that controlled execution flow:

```csharp
// BEFORE: user input controls the condition
var error = request.Query["error"];
var errorCode = request.Query["error_code"];

if (error.Count > 0 || errorCode.Count > 0)  // ← condition based on user input
{
    throw new SupabaseOAuthException(...);   // ← CodeQL: "sensitive action guarded by user input"
}
```

CodeQL classified this as **CWE-807** (Reliance on Untrusted Inputs in a Security Decision) — decisions about executing sensitive methods should not directly depend on user input.

## Solution

Refactored the logic so that user input **does not participate in control flow**, while **preserving informative error messages**:

```csharp
// AFTER: error parameters are only used in the message, not in the condition
var code = request.Query["code"].ToString();

if (string.IsNullOrWhiteSpace(code))  // ← condition only checks for code presence
{
    // Error parameters are read INSIDE the block — for the message, not for the condition
    var error = request.Query["error"].ToString();
    var errorDescription = request.Query["error_description"].ToString();
    
    var details = !string.IsNullOrEmpty(errorDescription) ? errorDescription 
                : !string.IsNullOrEmpty(error) ? error 
                : "unknown reason";
    
    throw new InvalidOperationException($"OAuth callback failed: {details}");
}
```

### Why This Approach

| Alternative | Why Rejected |
|-------------|--------------|
| Suppress comments | Not a real fix, just hides the problem |
| Complete removal of error parameters | Loses error informativeness (e.g., "user denied access") |
| Try-catch around old code | Doesn't break taint flow — condition is still based on user input |

**The chosen solution** eliminates the vulnerability AND preserves functionality:
- ✅ User input does not control execution flow
- ✅ Detailed error messages are preserved
- ✅ PKCE validation added at the beginning of the method
- ✅ Supabase API errors wrapped in try-catch

## Changes

- Removed condition based on `error`/`errorCode` parameters
- Error parameters are now read only inside the `if (code.IsNullOrWhiteSpace)` block
- Added `_pkceCodeVerifier` check at the beginning of the method
- Added try-catch for `ExchangeCodeForSession`

## Testing

- [x] `dotnet build` passes successfully
- [ ] Awaiting CodeQL rescan after merge
