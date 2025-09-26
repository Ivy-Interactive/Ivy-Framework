namespace Ivy.Auth;

/// <summary>
/// Represents user information retrieved from an authentication provider.
/// </summary>
/// <param name="Id">Unique user identifier</param>
/// <param name="Email">User's email address</param>
/// <param name="FullName">User's full name</param>
/// <param name="AvatarUrl">URL to the user's avatar image</param>
public record UserInfo(string Id, string Email, string? FullName, string? AvatarUrl)
{
    /// <summary>
    /// Gets the user's initials derived from their full name or email.
    /// </summary>
    public string Initials
    {
        get
        {
            if (!string.IsNullOrEmpty(FullName))
            {
                var names = FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (names.Length >= 2)
                {
                    return $"{char.ToUpper(names[0][0])}{char.ToUpper(names[1][0])}";
                }

                if (names.Length == 1)
                {
                    return char.ToUpper(names[0][0]).ToString();
                }
            }

            if (!string.IsNullOrEmpty(Email))
            {
                return char.ToUpper(Email[0]).ToString();
            }

            return string.Empty;
        }
    }
}