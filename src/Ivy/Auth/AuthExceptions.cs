namespace Ivy.Auth;

public class AuthException : Exception
{
    public AuthException(string message) : base(message) { }
    public AuthException(string message, Exception innerException) : base(message, innerException) { }
}

public class MissingAuthTokenException : AuthException
{
    public MissingAuthTokenException() : base("Missing auth token") { }
    public MissingAuthTokenException(string message) : base(message) { }
}

public class InvalidAuthTokenException : AuthException
{
    public InvalidAuthTokenException() : base("Invalid or expired auth token") { }
    public InvalidAuthTokenException(string message) : base(message) { }
}

public class AuthProviderNotConfiguredException : AuthException
{
    public AuthProviderNotConfiguredException() : base("Auth provider not configured") { }
    public AuthProviderNotConfiguredException(string message) : base(message) { }
}

public class AuthValidationException : AuthException
{
    public AuthValidationException() : base("Error validating auth token") { }
    public AuthValidationException(string message) : base(message) { }
    public AuthValidationException(string message, Exception innerException) : base(message, innerException) { }
}
