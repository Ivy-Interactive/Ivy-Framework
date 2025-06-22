# Ivy.Auth.Supabase

An Ivy authentication provider for Supabase.

## class SupabaseAuthProvider
The `SupabaseAuthProvider` class implements the `Ivy.Auth.IAuthProvider` interface and acts as an authentication provider allowing Ivy code to authenticate with [Supabase](https://supabase.com/), a Postgres development platform.

### Configuration Requirements During Construction
Creating an object of the `SupabaseAuthProvider` class requires that two keys be available in the system configuration:

* SUPABASE_URL: The URL to the Supabase instance to which the object will authenticate
* SUPABASE_API_KEY: The API key used to send API requests to the Supabase instance

If either or both of these entries are not available in system configuration when the object is created, then an `Exception` object will be thrown.

If all required configuration options are available, then the created `SupabaseAuthProvider` object will create a connection to a Supabase client object.

### public async Task<string?> LoginAsync(string email, string password)
This method logs in to the Supabase client created when the `SupabaseAuthProvider` object was constructed. The method is designed to be called asynchronously and its return should be `await`ed.

The code logs into the Supabase instance using the supplied `email` and `password`, and a `string`-based access token is returned.

### public async Task<Uri> GetOAuthUriAsync(string optionId, Uri callbackUri)
This method returns the URI of the OAuth client specified by the given `optionId`, which must be one of the following:

* `google`
* `apple`
* `github`

If the supplied `optionId` is not one of the options in the list above, an `Exception` object will be thrown.

If the supplied `optionId` is one of the options in the list above, then the method will log into the Supabase client using the appropriate OAuth path, depending on the client option, and will return the URI of the completed OAuth path.

### public string HandleOAuthCallback(HttpRequest request)
This method handles the OAuth callback for the supplied `HttpRequest` object by returning the value of the `code` variable available in the request.

### public async Task LogoutAsync(string _)
This method logs the caller out of the Supabase client maintained by the object. The underlying Supabase object is retained, so the caller can log in again, if necessary, using the same `SupabaseAuthProvider` object instance.

### public async Task<bool> ValidateJwtAsync(string jwt)
This method validates a supplied JSON Web Token (JWT) for use with the underlying Supabase object. The method sends the supplied `jwt` string to the connected Supabase client and requests the User property.

This method returns `true` if the supplied JWT is valid and `false` if the supplied JWT is invalid.

### public async Task<UserInfo?> GetUserInfoAsync(string jwt)
This method returns user information for the user represented by the supplied `jwt` token.  The information is returned in an `Ivy.Auth.UserInfo` record:

* user ID
* user email
* user metadata, which includes the user's full name, if available, or an emptry string if it is not

If user information is not available in the supplied `jwt` token, then `null` is returned.

### public SupabaseAuthProvider UseEmailPassword()
This method sets an option to use email password in the authorization flow. The called `SupabaseAuthProvider` object is returned.

### public SupabaseAuthProvider UseGoogle()
This method sets an option to use Google's OAuth implementation in the authorization flow. The called `SupabaseAuthProvider` object is returned.

### public SupabaseAuthProvider UseApple()
This method sets an option to use Apple's OAuth implementation in the authorization flow. The called `SupabaseAuthProvider` object is returned.

### public AuthOption[] GetAuthOptions()
This method returns an array of `Ivy.Auth.AuthOption` records.

When an `SupabaseAuthProvider` object is created, then no authorization options are set. If, during the object's lifetime, the `UseEmailPassword()`, `UseGoogle()`, or `UseApple()` methods are called, then those options are noted in the object's state. If `GetAuthOptions()` is called after one or more of those methods are called, then the options will be reflected in the array returned by the call to `GetAuthOptions()`.