namespace Ivy.Auth;

public record UserInfo(string Id, string Email, string? FullName, string? AvatarUrl)
{
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