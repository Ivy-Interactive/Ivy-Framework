namespace Ivy.Auth;

/// <summary>
/// Base exception for authentication-related errors.
/// </summary>
public class AuthException : Exception
{
    public AuthException(string message) : base(message) { }
    public AuthException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when authentication is required but no token is provided or the token is empty.
/// </summary>
public class MissingAuthTokenException : AuthException
{
    public MissingAuthTokenException() : base("Missing auth token") { }
    public MissingAuthTokenException(string message) : base(message) { }
}

/// <summary>
/// Exception thrown when the provided authentication token is invalid or expired.
/// </summary>
public class InvalidAuthTokenException : AuthException
{
    public InvalidAuthTokenException() : base("Invalid or expired auth token") { }
    public InvalidAuthTokenException(string message) : base(message) { }
}

/// <summary>
/// Exception thrown when the authentication provider is not properly configured.
/// </summary>
public class AuthProviderNotConfiguredException : AuthException
{
    public AuthProviderNotConfiguredException() : base("Auth provider not configured") { }
    public AuthProviderNotConfiguredException(string message) : base(message) { }
}

/// <summary>
/// Exception thrown when an error occurs during authentication token validation.
/// </summary>
public class AuthValidationException : AuthException
{
    public AuthValidationException() : base("Error validating auth token") { }
    public AuthValidationException(string message) : base(message) { }
    public AuthValidationException(string message, Exception innerException) : base(message, innerException) { }
}
