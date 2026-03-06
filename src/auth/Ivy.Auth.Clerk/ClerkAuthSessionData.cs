using System.Text.Json;

namespace Ivy.Auth.Clerk;

public class ClerkAuthSessionData
{
    public string? DevBrowserToken { get; set; }
    public string? PendingSignInId { get; set; }

    public static ClerkAuthSessionData? FromString(string? json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        // Handle legacy format (plain dev browser token string)
        if (json.StartsWith("dvb_"))
        {
            return new ClerkAuthSessionData { DevBrowserToken = json };
        }

        try
        {
            return JsonSerializer.Deserialize<ClerkAuthSessionData>(json);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public string? ToJson()
    {
        // Don't serialize if both fields are null
        if (DevBrowserToken == null && PendingSignInId == null)
        {
            return null;
        }

        return JsonSerializer.Serialize(this);
    }
}
